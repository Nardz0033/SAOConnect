using NardzSAO.Models;

namespace NardzSAO.ViewModels;

public class HomeIndexViewModel
{
    public int StudentCount { get; set; }
    public int OrgCount { get; set; }
    public int EventCount { get; set; }
    public int RequestCount { get; set; }

    public List<Event> Events { get; set; } = new();
    public List<Announcement> Announcements { get; set; } = new();
    public List<Organization> Organizations { get; set; } = new();
}
