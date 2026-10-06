using NardzSAO.Models;

namespace NardzSAO.ViewModels;

public class DashboardViewModel
{
    public string UserName { get; set; } = string.Empty;
    public int TotalStudents { get; set; }
    public int ActiveOrganizations { get; set; }
    public int UpcomingEvents { get; set; }
    public int PendingRequests { get; set; }
    public int PendingAppointments { get; set; }
    public int UnresolvedConcerns { get; set; }
    public int PublishedAnnouncements { get; set; }
    public int EventAttendance { get; set; }

    public List<Announcement> RecentAnnouncements { get; set; } = new();
    public List<Event> UpcomingEventList { get; set; } = new();
    public List<RequestItem> PendingRequestsList { get; set; } = new();
    public List<Notification> Notifications { get; set; } = new();
    public List<OrganizationMembership> JoinedOrganizations { get; set; } = new();
    public List<Appointment> Appointments { get; set; } = new();
    public List<object> StudentActivity { get; set; } = new();
}
