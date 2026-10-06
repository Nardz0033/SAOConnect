using System.Security.Claims;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using NardzSAO.Data;
using NardzSAO.Models;

namespace NardzSAO.Controllers;

public class EventsController(ApplicationDbContext context) : Controller
{
    [AllowAnonymous]
    public async Task<IActionResult> Index(string? search)
    {
        var query = context.Events.Where(e => !e.IsArchived).AsQueryable();

        if (!string.IsNullOrWhiteSpace(search))
        {
            query = query.Where(e => e.Name.Contains(search) || e.Location.Contains(search) || e.Category.Contains(search));
        }

        var events = await query.OrderBy(e => e.StartDateTime).ToListAsync();
        return View(events);
    }

    [AllowAnonymous]
    public async Task<IActionResult> Details(int id)
    {
        var eventItem = await context.Events
            .Include(e => e.Registrations)
            .FirstOrDefaultAsync(e => e.Id == id);

        if (eventItem == null)
        {
            return NotFound();
        }

        var userId = User.FindFirstValue(ClaimTypes.NameIdentifier);
        ViewBag.IsRegistered = userId != null && await context.EventRegistrations.AnyAsync(r => r.EventId == id && r.UserId == userId);

        return View(eventItem);
    }

    [Authorize(Roles = "Student")]
    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Register(int id)
    {
        var eventItem = await context.Events.FirstOrDefaultAsync(e => e.Id == id);
        if (eventItem == null)
        {
            return NotFound();
        }

        var userId = User.FindFirstValue(ClaimTypes.NameIdentifier);
        if (string.IsNullOrEmpty(userId))
        {
            return Challenge();
        }

        if (eventItem.RegistrationDeadline < DateTime.UtcNow || eventItem.EndDateTime < DateTime.UtcNow)
        {
            TempData["Error"] = "Registration is no longer available for this event.";
            return RedirectToAction(nameof(Details), new { id });
        }

        if (await context.EventRegistrations.CountAsync(r => r.EventId == id) >= eventItem.MaximumParticipants)
        {
            TempData["Error"] = "This event is full.";
            return RedirectToAction(nameof(Details), new { id });
        }

        if (await context.EventRegistrations.AnyAsync(r => r.EventId == id && r.UserId == userId))
        {
            TempData["Info"] = "You are already registered for this event.";
            return RedirectToAction(nameof(Details), new { id });
        }

        context.EventRegistrations.Add(new EventRegistration
        {
            EventId = id,
            UserId = userId,
            RegisteredAt = DateTime.UtcNow,
            Status = "Registered"
        });

        context.Notifications.Add(new Notification
        {
            UserId = userId,
            Title = "Event Registration",
            Message = $"You registered for {eventItem.Name}.",
            Type = "Event",
            IsRead = false
        });

        await context.SaveChangesAsync();
        TempData["Success"] = "Registration submitted successfully.";
        return RedirectToAction(nameof(Details), new { id });
    }

    [Authorize(Roles = "Staff,Admin")]
    public IActionResult Create()
    {
        return View();
    }

    [HttpPost]
    [Authorize(Roles = "Staff,Admin")]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Create(Event model, IFormFile? posterFile)
    {
        var userId = User.FindFirstValue(ClaimTypes.NameIdentifier);
        if (string.IsNullOrEmpty(userId)) return Challenge();

        model.CreatedByUserId = userId;
        model.CreatedAt = DateTime.UtcNow;
        if (posterFile != null && posterFile.Length > 0)
        {
            var safeName = Guid.NewGuid() + Path.GetExtension(posterFile.FileName);
            var path = Path.Combine(Directory.GetCurrentDirectory(), "wwwroot", "images", safeName);
            await using var stream = new FileStream(path, FileMode.Create);
            await posterFile.CopyToAsync(stream);
            model.PosterUrl = "/images/" + safeName;
        }

        context.Events.Add(model);
        await context.SaveChangesAsync();
        return RedirectToAction(nameof(Index));
    }

    [Authorize(Roles = "Staff,Admin")]
    public async Task<IActionResult> Attendance(int id)
    {
        var eventItem = await context.Events.FirstOrDefaultAsync(e => e.Id == id);
        if (eventItem == null) return NotFound();

        var records = await context.Attendances
            .Where(a => a.EventId == id)
            .Include(a => a.User)
            .ToListAsync();

        var registered = await context.EventRegistrations
            .Where(r => r.EventId == id)
            .Include(r => r.User)
            .ToListAsync();

        ViewBag.RegisteredCount = registered.Count;
        ViewBag.PresentCount = records.Count(r => r.AttendanceStatus == "Present");
        ViewBag.AbsentCount = Math.Max(0, registered.Count - records.Count(r => r.AttendanceStatus == "Present"));
        ViewBag.QrValue = $"SAOConnect-Event-{id}";
        ViewData["Event"] = eventItem;

        return View();
    }

    [HttpPost]
    [Authorize(Roles = "Student")]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> MarkAttendance(int eventId, string qrCode)
    {
        var userId = User.FindFirstValue(ClaimTypes.NameIdentifier);
        if (string.IsNullOrEmpty(userId)) return Challenge();

        var expectedQr = $"SAOConnect-Event-{eventId}";
        if (!string.Equals(qrCode, expectedQr, StringComparison.OrdinalIgnoreCase))
        {
            TempData["Error"] = "Invalid attendance QR code.";
            return RedirectToAction(nameof(Index));
        }

        var eventItem = await context.Events.FirstOrDefaultAsync(e => e.Id == eventId);
        if (eventItem == null) return NotFound();

        var exists = await context.Attendances.AnyAsync(a => a.EventId == eventId && a.UserId == userId);
        if (exists)
        {
            TempData["Info"] = "Attendance already recorded.";
            return RedirectToAction(nameof(Index));
        }

        context.Attendances.Add(new Attendance
        {
            EventId = eventId,
            UserId = userId,
            AttendanceStatus = "Present",
            DateRecorded = DateTime.UtcNow,
            QrCodeValue = qrCode,
            Remarks = "Scanned via QR code"
        });

        await context.SaveChangesAsync();
        TempData["Success"] = "Attendance recorded successfully.";
        return RedirectToAction(nameof(Index));
    }
}
