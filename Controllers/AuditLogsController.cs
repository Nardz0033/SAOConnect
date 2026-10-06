using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using NardzSAO.Data;

namespace NardzSAO.Controllers;

[Authorize(Roles = "Admin,Staff")]
public class AuditLogsController(ApplicationDbContext context) : Controller
{
    public async Task<IActionResult> Index()
    {
        var logs = await context.AuditLogs
            .OrderByDescending(l => l.CreatedAt)
            .Take(100)
            .ToListAsync();

        return View(logs);
    }
}
