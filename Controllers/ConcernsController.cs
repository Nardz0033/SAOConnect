using System.Security.Claims;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using NardzSAO.Data;
using NardzSAO.Models;

namespace NardzSAO.Controllers;

public class ConcernsController(ApplicationDbContext context) : Controller
{
    [Authorize]
    public async Task<IActionResult> Index()
    {
        var userId = User.FindFirstValue(ClaimTypes.NameIdentifier);
        var isStaff = User.IsInRole("Staff") || User.IsInRole("Admin");

        var concerns = context.Concerns
            .Include(c => c.User)
            .AsQueryable();

        if (!isStaff)
        {
            concerns = concerns.Where(c => c.UserId == userId);
        }

        var list = await concerns.OrderByDescending(c => c.CreatedAt).ToListAsync();
        return View(list);
    }

    [Authorize(Roles = "Student")]
    public IActionResult Create()
    {
        ViewBag.Categories = Enum.GetValues<ConcernCategory>();
        return View();
    }

    [HttpPost]
    [Authorize(Roles = "Student")]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Create(Concern model, IFormFile? attachment)
    {
        var userId = User.FindFirstValue(ClaimTypes.NameIdentifier);
        if (string.IsNullOrEmpty(userId)) return Challenge();

        model.UserId = userId;
        model.CreatedAt = DateTime.UtcNow;
        model.Status = ConcernStatus.Submitted;

        if (attachment != null && attachment.Length > 0)
        {
            var safeName = Guid.NewGuid() + Path.GetExtension(attachment.FileName);
            var path = Path.Combine(Directory.GetCurrentDirectory(), "wwwroot", "uploads", safeName);
            await using var stream = new FileStream(path, FileMode.Create);
            await attachment.CopyToAsync(stream);
            model.AttachmentUrl = "/uploads/" + safeName;
        }

        context.Concerns.Add(model);
        await context.SaveChangesAsync();

        return RedirectToAction(nameof(Index));
    }
}
