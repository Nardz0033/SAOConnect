using System.Diagnostics;
using System.Security.Claims;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using NardzSAO.Data;
using NardzSAO.Models;
using NardzSAO.Services;
using NardzSAO.ViewModels;

namespace NardzSAO.Controllers;

public class HomeController : Controller
{
    private readonly ApplicationDbContext _context;
    private readonly DashboardService _dashboardService;

    public HomeController(ApplicationDbContext context, DashboardService dashboardService)
    {
        _context = context;
        _dashboardService = dashboardService;
    }

    public async Task<IActionResult> Index()
    {
        var model = new HomeIndexViewModel
        {
            StudentCount = await _context.Users.CountAsync(),
            OrgCount = await _context.Organizations.CountAsync(),
            EventCount = await _context.Events.CountAsync(),
            RequestCount = await _context.Requests.CountAsync(),
            Events = await _context.Events
                .OrderByDescending(e => e.StartDateTime)
                .Take(3)
                .ToListAsync(),
            Announcements = await _context.Announcements
                .OrderByDescending(a => a.PublishDate)
                .Take(3)
                .ToListAsync(),
            Organizations = await _context.Organizations
                .OrderByDescending(o => o.CreatedAt)
                .Take(3)
                .ToListAsync()
        };

        return View(model);
    }

    [Authorize]
    public async Task<IActionResult> Dashboard()
    {
        var userId = User.FindFirstValue(ClaimTypes.NameIdentifier);
        if (string.IsNullOrEmpty(userId))
        {
            return Challenge();
        }

        var role = User.IsInRole("Admin") ? "Admin" : User.IsInRole("Staff") ? "Staff" : "Student";
        var model = await _dashboardService.GetDashboardAsync(userId, User.Identity?.Name ?? "User", role);

        var viewName = role == "Student" ? "StudentDashboard" : "StaffDashboard";
        return View(viewName, model);
    }

    public IActionResult Privacy()
    {
        return View();
    }

    [ResponseCache(Duration = 0, Location = ResponseCacheLocation.None, NoStore = true)]
    public IActionResult Error()
    {
        return View(new ErrorViewModel { RequestId = Activity.Current?.Id ?? HttpContext.TraceIdentifier });
    }
}
