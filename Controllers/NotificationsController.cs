using System.Security.Claims;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using NardzSAO.Data;
using NardzSAO.Models;

namespace NardzSAO.Controllers;

public class NotificationsController(ApplicationDbContext context) : Controller
{
    [Authorize]
    public async Task<IActionResult> Index()
    {
        var userId = User.FindFirstValue(ClaimTypes.NameIdentifier);
        var notifications = await context.Notifications
            .Where(n => n.UserId == userId)
            .OrderByDescending(n => n.CreatedAt)
            .ToListAsync();

        return View(notifications);
    }

    [Authorize]
    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> MarkRead(int id)
    {
        var notification = await context.Notifications.FirstOrDefaultAsync(n => n.Id == id && n.UserId == User.FindFirstValue(ClaimTypes.NameIdentifier));
        if (notification == null) return NotFound();

        notification.IsRead = true;
        await context.SaveChangesAsync();
        return RedirectToAction(nameof(Index));
    }

    [Authorize]
    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> MarkAllRead()
    {
        var userId = User.FindFirstValue(ClaimTypes.NameIdentifier);
        var notifications = await context.Notifications.Where(n => n.UserId == userId && !n.IsRead).ToListAsync();
        foreach (var notification in notifications)
        {
            notification.IsRead = true;
        }
        await context.SaveChangesAsync();
        return RedirectToAction(nameof(Index));
    }
}
