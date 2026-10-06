using System.Security.Claims;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using NardzSAO.Data;
using NardzSAO.Models;

namespace NardzSAO.Controllers;

public class OrganizationsController(ApplicationDbContext context) : Controller
{
    [AllowAnonymous]
    public async Task<IActionResult> Index()
    {
        var organizations = await context.Organizations
            .Where(o => o.Status == OrganizationStatus.Approved)
            .OrderBy(o => o.Name)
            .ToListAsync();

        return View(organizations);
    }

    [AllowAnonymous]
    public async Task<IActionResult> Details(int id)
    {
        var organization = await context.Organizations
            .Include(o => o.Memberships)
            .FirstOrDefaultAsync(o => o.Id == id);

        if (organization == null)
        {
            return NotFound();
        }

        return View(organization);
    }

    [Authorize(Roles = "Student")]
    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Join(int id)
    {
        var userId = User.FindFirstValue(ClaimTypes.NameIdentifier);
        if (string.IsNullOrEmpty(userId)) return Challenge();

        var exists = await context.OrganizationMemberships.AnyAsync(m => m.OrganizationId == id && m.UserId == userId);
        if (exists)
        {
            TempData["Info"] = "Membership request already sent.";
            return RedirectToAction(nameof(Details), new { id });
        }

        context.OrganizationMemberships.Add(new OrganizationMembership
        {
            OrganizationId = id,
            UserId = userId,
            Status = MembershipStatus.Requested,
            JoinedAt = DateTime.UtcNow
        });

        await context.SaveChangesAsync();
        TempData["Success"] = "Membership request submitted.";
        return RedirectToAction(nameof(Details), new { id });
    }
}
