using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using NardzSAO.Data;

namespace NardzSAO.Controllers;

[Authorize(Roles = "Staff,Admin")]
public class ReportsController(ApplicationDbContext context) : Controller
{
    public async Task<IActionResult> Index()
    {
        var report = new
        {
            TotalAttendances = await context.Attendances.CountAsync(),
            TotalRequests = await context.Requests.CountAsync(),
            TotalOrganizations = await context.OrganizationMemberships.CountAsync(),
            TotalAnnouncements = await context.Announcements.CountAsync(),
            TotalAppointments = await context.Appointments.CountAsync(),
            TotalUsers = await context.Users.CountAsync()
        };

        return View(report);
    }
}
