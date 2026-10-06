using System.Security.Claims;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using NardzSAO.Data;
using NardzSAO.Models;

namespace NardzSAO.Controllers;

public class AppointmentsController(ApplicationDbContext context) : Controller
{
    [Authorize]
    public async Task<IActionResult> Index()
    {
        var userId = User.FindFirstValue(ClaimTypes.NameIdentifier);
        var isStaff = User.IsInRole("Staff") || User.IsInRole("Admin");

        var appointments = context.Appointments
            .AsQueryable();

        if (!isStaff)
        {
            appointments = appointments.Where(a => a.UserId == userId);
        }

        var list = await appointments.OrderByDescending(a => a.CreatedAt).ToListAsync();
        return View(list);
    }

    [Authorize(Roles = "Student")]
    public IActionResult Create()
    {
        return View();
    }

    [HttpPost]
    [Authorize(Roles = "Student")]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Create(Appointment model)
    {
        var userId = User.FindFirstValue(ClaimTypes.NameIdentifier);
        if (string.IsNullOrEmpty(userId)) return Challenge();

        model.UserId = userId;
        model.Status = AppointmentStatus.Pending;
        model.CreatedAt = DateTime.UtcNow;

        context.Appointments.Add(model);
        await context.SaveChangesAsync();

        context.Notifications.Add(new Notification
        {
            UserId = userId,
            Title = "Appointment Requested",
            Message = $"Your {model.AppointmentType} appointment request has been submitted.",
            Type = "Appointment",
            IsRead = false
        });

        await context.SaveChangesAsync();
        return RedirectToAction(nameof(Index));
    }

    [Authorize(Roles = "Staff,Admin")]
    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> UpdateStatus(int id, AppointmentStatus status, string? notes)
    {
        var appointment = await context.Appointments.FirstOrDefaultAsync(a => a.Id == id);
        if (appointment == null) return NotFound();

        appointment.Status = status;
        appointment.Notes = notes;

        context.Notifications.Add(new Notification
        {
            UserId = appointment.UserId,
            Title = "Appointment Updated",
            Message = $"Your appointment status changed to {status}.",
            Type = "Appointment",
            IsRead = false
        });

        await context.SaveChangesAsync();
        return RedirectToAction(nameof(Index));
    }
}
