using Microsoft.EntityFrameworkCore;
using NardzSAO.Data;
using NardzSAO.Models;
using NardzSAO.ViewModels;

namespace NardzSAO.Services;

public class DashboardService(ApplicationDbContext context)
{
    public async Task<DashboardViewModel> GetDashboardAsync(string userId, string userName, string role)
    {
        var today = DateTime.UtcNow;
        var studentRole = string.Equals(role, "Student", StringComparison.OrdinalIgnoreCase);

        var totalStudents = await context.Users.CountAsync(u => u.Email != null);
        var activeOrganizations = await context.Organizations.CountAsync(o => o.Status == OrganizationStatus.Approved);
        var upcomingEvents = await context.Events.CountAsync(e => e.StartDateTime >= today && !e.IsArchived);
        var pendingRequests = await context.Requests.CountAsync(r => r.Status == RequestStatus.Pending || r.Status == RequestStatus.UnderReview);
        var pendingAppointments = await context.Appointments.CountAsync(a => a.Status == AppointmentStatus.Pending || a.Status == AppointmentStatus.Rescheduled);
        var unresolvedConcerns = await context.Concerns.CountAsync(c => c.Status != ConcernStatus.Resolved && c.Status != ConcernStatus.Closed);
        var publishedAnnouncements = await context.Announcements.CountAsync(a => a.PublishDate <= today && (!a.ExpirationDate.HasValue || a.ExpirationDate > today));
        var eventAttendance = await context.Attendances.CountAsync();

        var model = new DashboardViewModel
        {
            UserName = userName,
            TotalStudents = totalStudents,
            ActiveOrganizations = activeOrganizations,
            UpcomingEvents = upcomingEvents,
            PendingRequests = pendingRequests,
            PendingAppointments = pendingAppointments,
            UnresolvedConcerns = unresolvedConcerns,
            PublishedAnnouncements = publishedAnnouncements,
            EventAttendance = eventAttendance,
            RecentAnnouncements = await context.Announcements
                .Include(a => a.CreatedByUser)
                .OrderByDescending(a => a.PublishDate)
                .Take(4)
                .ToListAsync(),
            UpcomingEventList = await context.Events
                .Where(e => e.StartDateTime >= today && !e.IsArchived)
                .OrderBy(e => e.StartDateTime)
                .Take(5)
                .ToListAsync(),
            PendingRequestsList = await context.Requests
                .Include(r => r.RequestedByUser)
                .Where(r => r.Status == RequestStatus.Pending || r.Status == RequestStatus.UnderReview)
                .OrderByDescending(r => r.DateSubmitted)
                .Take(5)
                .ToListAsync(),
            Notifications = await context.Notifications
                .Where(n => n.UserId == userId)
                .OrderByDescending(n => n.CreatedAt)
                .Take(5)
                .ToListAsync(),
            JoinedOrganizations = await context.OrganizationMemberships
                .Include(m => m.Organization)
                .Where(m => m.UserId == userId)
                .OrderByDescending(m => m.JoinedAt)
                .Take(5)
                .ToListAsync(),
            Appointments = await context.Appointments
                .Where(a => a.UserId == userId)
                .OrderByDescending(a => a.CreatedAt)
                .Take(5)
                .ToListAsync(),
            StudentActivity = studentRole
                ? (await context.Requests
                    .Where(r => r.RequestedByUserId == userId)
                    .OrderByDescending(r => r.DateSubmitted)
                    .Take(5)
                    .ToListAsync())
                    .Cast<object>()
                    .ToList()
                : (await context.AuditLogs
                    .OrderByDescending(a => a.CreatedAt)
                    .Take(5)
                    .ToListAsync())
                    .Cast<object>()
                    .ToList()
        };

        return model;
    }
}
