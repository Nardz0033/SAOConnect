using System.Security.Claims;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using NardzSAO.Data;
using NardzSAO.Models;

namespace NardzSAO.Controllers;

public class AnnouncementsController(ApplicationDbContext context) : Controller
{
    [AllowAnonymous]
    public async Task<IActionResult> Index(string? search, AnnouncementCategoryType? category, AnnouncementPriority? priority, bool featuredOnly = false)
    {
        var query = context.Announcements
            .Include(a => a.CreatedByUser)
            .AsQueryable();

        if (!string.IsNullOrWhiteSpace(search))
        {
            query = query.Where(a => a.Title.Contains(search) || a.Content.Contains(search));
        }

        if (category.HasValue)
        {
            query = query.Where(a => a.Category == category.Value);
        }

        if (priority.HasValue)
        {
            query = query.Where(a => a.Priority == priority.Value);
        }

        if (featuredOnly)
        {
            query = query.Where(a => a.IsFeatured);
        }

        query = query.Where(a => !a.ExpirationDate.HasValue || a.ExpirationDate > DateTime.UtcNow);

        var announcements = await query.OrderByDescending(a => a.PublishDate).ToListAsync();
        ViewBag.Categories = Enum.GetValues<AnnouncementCategoryType>();
        ViewBag.Priorities = Enum.GetValues<AnnouncementPriority>();
        return View(announcements);
    }

    [AllowAnonymous]
    public async Task<IActionResult> Details(int id)
    {
        var announcement = await context.Announcements
            .Include(a => a.CreatedByUser)
            .FirstOrDefaultAsync(a => a.Id == id);

        if (announcement == null)
        {
            return NotFound();
        }

        announcement.ViewCount += 1;
        await context.SaveChangesAsync();

        return View(announcement);
    }

    [Authorize(Roles = "Staff,Admin")]
    public IActionResult Create()
    {
        ViewBag.Categories = Enum.GetValues<AnnouncementCategoryType>();
        ViewBag.Priorities = Enum.GetValues<AnnouncementPriority>();
        return View();
    }

    [HttpPost]
    [Authorize(Roles = "Staff,Admin")]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Create(Announcement model, IFormFile? imageFile, IFormFile? documentFile)
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

        model.CreatedByUserId = userId;
        model.CreatedAt = DateTime.UtcNow;
        model.PublishDate = model.PublishDate == default ? DateTime.UtcNow : model.PublishDate;

        if (imageFile != null && imageFile.Length > 0)
        {
            var fileName = Guid.NewGuid() + Path.GetExtension(imageFile.FileName);
            var path = Path.Combine(Directory.GetCurrentDirectory(), "wwwroot", "uploads", fileName);
            await using var stream = new FileStream(path, FileMode.Create);
            await imageFile.CopyToAsync(stream);
            model.ImageUrl = "/uploads/" + fileName;
        }

        if (documentFile != null && documentFile.Length > 0)
        {
            var fileName = Guid.NewGuid() + Path.GetExtension(documentFile.FileName);
            var path = Path.Combine(Directory.GetCurrentDirectory(), "wwwroot", "uploads", fileName);
            await using var stream = new FileStream(path, FileMode.Create);
            await documentFile.CopyToAsync(stream);
            model.DocumentUrl = "/uploads/" + fileName;
        }

        context.Announcements.Add(model);
        await context.SaveChangesAsync();

        return RedirectToAction(nameof(Index));
    }
}
