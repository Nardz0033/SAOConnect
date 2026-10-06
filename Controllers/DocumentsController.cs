using System.Security.Claims;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using NardzSAO.Data;
using NardzSAO.Models;

namespace NardzSAO.Controllers;

public class DocumentsController(ApplicationDbContext context) : Controller
{
    [Authorize]
    public async Task<IActionResult> Index()
    {
        var userId = User.FindFirstValue(ClaimTypes.NameIdentifier);
        var isStaff = User.IsInRole("Staff") || User.IsInRole("Admin");

        var documents = context.Documents.AsQueryable();

        if (!isStaff)
        {
            documents = documents.Where(d => d.UserId == userId);
        }

        var list = await documents.OrderByDescending(d => d.CreatedAt).ToListAsync();
        return View(list);
    }

    [Authorize]
    public IActionResult Upload()
    {
        return View();
    }

    [HttpPost]
    [Authorize]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Upload(IFormFile file, string category)
    {
        if (file == null || file.Length == 0)
        {
            ModelState.AddModelError(string.Empty, "Please choose a document to upload.");
            return View();
        }

        var allowedExtensions = new[] { ".pdf", ".jpg", ".jpeg", ".png", ".docx" };
        var extension = Path.GetExtension(file.FileName).ToLowerInvariant();
        if (!allowedExtensions.Contains(extension))
        {
            ModelState.AddModelError(string.Empty, "Unsupported file type.");
            return View();
        }

        if (file.Length > 5 * 1024 * 1024)
        {
            ModelState.AddModelError(string.Empty, "File size must be under 5 MB.");
            return View();
        }

        var userId = User.FindFirstValue(ClaimTypes.NameIdentifier);
        if (string.IsNullOrEmpty(userId)) return Challenge();

        var safeName = Guid.NewGuid() + extension;
        var path = Path.Combine(Directory.GetCurrentDirectory(), "wwwroot", "uploads", safeName);
        await using var stream = new FileStream(path, FileMode.Create);
        await file.CopyToAsync(stream);

        context.Documents.Add(new Document
        {
            UserId = userId,
            FileName = file.FileName,
            StoredFileName = safeName,
            ContentType = file.ContentType,
            FileSize = file.Length,
            Category = category,
            CreatedAt = DateTime.UtcNow
        });

        await context.SaveChangesAsync();
        return RedirectToAction(nameof(Index));
    }
}
