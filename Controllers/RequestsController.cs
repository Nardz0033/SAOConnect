using System.Security.Claims;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using NardzSAO.Data;
using NardzSAO.Models;

namespace NardzSAO.Controllers;

public class RequestsController(ApplicationDbContext context) : Controller
{
    [Authorize]
    public async Task<IActionResult> Index()
    {
        var userId = User.FindFirstValue(ClaimTypes.NameIdentifier);
        var isStaff = User.IsInRole("Staff") || User.IsInRole("Admin");

        var query = context.Requests
            .Include(r => r.RequestedByUser)
            .AsQueryable();

        if (!isStaff)
        {
            query = query.Where(r => r.RequestedByUserId == userId);
        }

        var requests = await query.OrderByDescending(r => r.DateSubmitted).ToListAsync();
        ViewBag.Statuses = Enum.GetValues<RequestStatus>();
        return View(requests);
    }

    [Authorize(Roles = "Student")]
    public IActionResult Create()
    {
        ViewBag.Categories = Enum.GetValues<RequestCategory>();
        return View();
    }

    [HttpPost]
    [Authorize(Roles = "Student")]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Create(RequestItem model, IFormFile? attachment)
    {
        if (!ModelState.IsValid)
        {
            return View(model);
        }

        var userId = User.FindFirstValue(ClaimTypes.NameIdentifier);
        if (string.IsNullOrEmpty(userId))
        {
            return Challenge();
        }

        model.RequestedByUserId = userId;
        model.DateSubmitted = DateTime.UtcNow;
        model.Status = RequestStatus.Pending;

        if (attachment != null && attachment.Length > 0)
        {
            var safeName = Guid.NewGuid() + Path.GetExtension(attachment.FileName);
            var path = Path.Combine(Directory.GetCurrentDirectory(), "wwwroot", "uploads", safeName);
            await using var stream = new FileStream(path, FileMode.Create);
            await attachment.CopyToAsync(stream);
            model.AttachmentPath = "/uploads/" + safeName;
            model.AttachmentFileName = attachment.FileName;
        }

        context.Requests.Add(model);
        await context.SaveChangesAsync();

        context.Notifications.Add(new Notification
        {
            UserId = userId,
            Title = "Request Submitted",
            Message = $"Your {model.Category} request has been submitted.",
            Type = "Request",
            IsRead = false
        });

        await context.SaveChangesAsync();
        return RedirectToAction(nameof(Index));
    }

    [Authorize(Roles = "Staff,Admin")]
    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> UpdateStatus(int id, RequestStatus status, string? notes)
    {
        var request = await context.Requests.FirstOrDefaultAsync(r => r.Id == id);
        if (request == null) return NotFound();

        request.Status = status;
        request.AdminNotes = notes;
        request.UpdatedAt = DateTime.UtcNow;

        context.Notifications.Add(new Notification
        {
            UserId = request.RequestedByUserId,
            Title = "Request Status Updated",
            Message = $"Your request '{request.Subject}' is now marked as {status}.",
            Type = "Request",
            IsRead = false
        });

        await context.SaveChangesAsync();
        return RedirectToAction(nameof(Index));
    }
}
