using Microsoft.AspNetCore.Identity;

namespace NardzSAO.Models;

public enum AnnouncementPriority
{
    Normal,
    Important,
    Urgent
}

public enum AnnouncementCategoryType
{
    General,
    StudentAffairs,
    Scholarships,
    Events,
    Organizations,
    StudentServices,
    Emergency,
    ImportantNotice
}

public enum RequestCategory
{
    CertificateRequest,
    OrganizationRequest,
    EventRequest,
    StudentAssistance,
    ClearanceRelatedRequest,
    AppointmentRequest,
    Other
}

public enum RequestStatus
{
    Pending,
    UnderReview,
    Approved,
    Rejected,
    Completed
}

public enum AppointmentStatus
{
    Pending,
    Approved,
    Rejected,
    Rescheduled,
    Completed
}

public enum ConcernCategory
{
    Academic,
    Personal,
    Organization,
    CampusLife,
    Facilities,
    StudentServices,
    Other
}

public enum ConcernStatus
{
    Submitted,
    InReview,
    Resolved,
    Closed
}

public enum OrganizationStatus
{
    Pending,
    Approved,
    Suspended,
    Archived
}

public enum MembershipStatus
{
    Requested,
    Approved,
    Rejected,
    Active
}

public class ApplicationUser : IdentityUser
{
    public string FullName { get; set; } = string.Empty;
    public string? StudentNumber { get; set; }
    public string? Department { get; set; }
    public bool IsActive { get; set; } = true;
    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;

    public ICollection<Announcement> Announcements { get; set; } = new List<Announcement>();
    public ICollection<EventRegistration> EventRegistrations { get; set; } = new List<EventRegistration>();
    public ICollection<RequestItem> Requests { get; set; } = new List<RequestItem>();
    public ICollection<Appointment> Appointments { get; set; } = new List<Appointment>();
    public ICollection<Concern> Concerns { get; set; } = new List<Concern>();
    public ICollection<OrganizationMembership> OrganizationMemberships { get; set; } = new List<OrganizationMembership>();
    public ICollection<Notification> Notifications { get; set; } = new List<Notification>();
    public ICollection<Document> Documents { get; set; } = new List<Document>();
    public ICollection<AuditLog> AuditLogs { get; set; } = new List<AuditLog>();
}

public class AnnouncementCategory
{
    public int Id { get; set; }
    public string Name { get; set; } = string.Empty;
    public string? Description { get; set; }

    public ICollection<Announcement> Announcements { get; set; } = new List<Announcement>();
}

public class Announcement
{
    public int Id { get; set; }
    public string Title { get; set; } = string.Empty;
    public string Content { get; set; } = string.Empty;
    public AnnouncementCategoryType Category { get; set; }
    public AnnouncementPriority Priority { get; set; } = AnnouncementPriority.Normal;
    public DateTime PublishDate { get; set; } = DateTime.UtcNow;
    public DateTime? ExpirationDate { get; set; }
    public string TargetAudience { get; set; } = "All Students";
    public string? ImageUrl { get; set; }
    public string? DocumentUrl { get; set; }
    public bool IsFeatured { get; set; }
    public int ViewCount { get; set; }
    public string CreatedByUserId { get; set; } = string.Empty;
    public ApplicationUser? CreatedByUser { get; set; }
    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
    public int AnnouncementCategoryId { get; set; }
    public AnnouncementCategory? AnnouncementCategory { get; set; }
}

public class Event
{
    public int Id { get; set; }
    public string Name { get; set; } = string.Empty;
    public string Description { get; set; } = string.Empty;
    public string Location { get; set; } = string.Empty;
    public DateTime StartDateTime { get; set; }
    public DateTime EndDateTime { get; set; }
    public int MaximumParticipants { get; set; }
    public string Organizer { get; set; } = string.Empty;
    public string Category { get; set; } = string.Empty;
    public string? PosterUrl { get; set; }
    public DateTime RegistrationDeadline { get; set; }
    public bool IsArchived { get; set; }
    public string CreatedByUserId { get; set; } = string.Empty;
    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;

    public ICollection<EventRegistration> Registrations { get; set; } = new List<EventRegistration>();
    public ICollection<Attendance> Attendances { get; set; } = new List<Attendance>();
}

public class EventRegistration
{
    public int Id { get; set; }
    public int EventId { get; set; }
    public string UserId { get; set; } = string.Empty;
    public DateTime RegisteredAt { get; set; } = DateTime.UtcNow;
    public string Status { get; set; } = "Registered";
    public Event? Event { get; set; }
    public ApplicationUser? User { get; set; }
}

public class Attendance
{
    public int Id { get; set; }
    public int EventId { get; set; }
    public string UserId { get; set; } = string.Empty;
    public DateTime DateRecorded { get; set; } = DateTime.UtcNow;
    public string AttendanceStatus { get; set; } = "Present";
    public string? QrCodeValue { get; set; }
    public string? Remarks { get; set; }

    public Event? Event { get; set; }
    public ApplicationUser? User { get; set; }
}

public class RequestItem
{
    public int Id { get; set; }
    public RequestCategory Category { get; set; }
    public string Subject { get; set; } = string.Empty;
    public string Description { get; set; } = string.Empty;
    public string? AttachmentFileName { get; set; }
    public string? AttachmentPath { get; set; }
    public DateTime DateSubmitted { get; set; } = DateTime.UtcNow;
    public RequestStatus Status { get; set; } = RequestStatus.Pending;
    public string RequestedByUserId { get; set; } = string.Empty;
    public ApplicationUser? RequestedByUser { get; set; }
    public string? AdminNotes { get; set; }
    public DateTime? UpdatedAt { get; set; }

    public ICollection<RequestAttachment> RequestAttachments { get; set; } = new List<RequestAttachment>();
}

public class RequestAttachment
{
    public int Id { get; set; }
    public int RequestId { get; set; }
    public string FileName { get; set; } = string.Empty;
    public string StoredFileName { get; set; } = string.Empty;
    public string ContentType { get; set; } = string.Empty;
    public long FileSize { get; set; }
    public DateTime UploadedAt { get; set; } = DateTime.UtcNow;

    public RequestItem? Request { get; set; }
}

public class Appointment
{
    public int Id { get; set; }
    public string UserId { get; set; } = string.Empty;
    public string AppointmentType { get; set; } = string.Empty;
    public DateTime PreferredDate { get; set; }
    public string PreferredTime { get; set; } = string.Empty;
    public string Reason { get; set; } = string.Empty;
    public AppointmentStatus Status { get; set; } = AppointmentStatus.Pending;
    public string? Notes { get; set; }
    public string? ApprovedByUserId { get; set; }
    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;

    public ApplicationUser? User { get; set; }
}

public class Concern
{
    public int Id { get; set; }
    public string UserId { get; set; } = string.Empty;
    public ConcernCategory Category { get; set; }
    public string Description { get; set; } = string.Empty;
    public string? AttachmentUrl { get; set; }
    public string PreferredContactMethod { get; set; } = "Email";
    public ConcernStatus Status { get; set; } = ConcernStatus.Submitted;
    public string? Response { get; set; }
    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;

    public ApplicationUser? User { get; set; }
}

public class Organization
{
    public int Id { get; set; }
    public string Name { get; set; } = string.Empty;
    public string Description { get; set; } = string.Empty;
    public string Category { get; set; } = string.Empty;
    public string President { get; set; } = string.Empty;
    public string Adviser { get; set; } = string.Empty;
    public string? LogoUrl { get; set; }
    public string ContactInformation { get; set; } = string.Empty;
    public OrganizationStatus Status { get; set; } = OrganizationStatus.Pending;
    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
    public string CreatedByUserId { get; set; } = string.Empty;

    public ICollection<OrganizationMembership> Memberships { get; set; } = new List<OrganizationMembership>();
}

public class OrganizationMembership
{
    public int Id { get; set; }
    public int OrganizationId { get; set; }
    public string UserId { get; set; } = string.Empty;
    public MembershipStatus Status { get; set; } = MembershipStatus.Requested;
    public DateTime JoinedAt { get; set; } = DateTime.UtcNow;

    public Organization? Organization { get; set; }
    public ApplicationUser? User { get; set; }
}

public class Notification
{
    public int Id { get; set; }
    public string UserId { get; set; } = string.Empty;
    public string Title { get; set; } = string.Empty;
    public string Message { get; set; } = string.Empty;
    public string Type { get; set; } = "General";
    public bool IsRead { get; set; }
    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
    public string? RelatedEntityType { get; set; }
    public int? RelatedEntityId { get; set; }

    public ApplicationUser? User { get; set; }
}

public class Document
{
    public int Id { get; set; }
    public string UserId { get; set; } = string.Empty;
    public string FileName { get; set; } = string.Empty;
    public string StoredFileName { get; set; } = string.Empty;
    public string ContentType { get; set; } = string.Empty;
    public long FileSize { get; set; }
    public string Category { get; set; } = "General";
    public bool IsPublic { get; set; }
    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;

    public ApplicationUser? User { get; set; }
}

public class AuditLog
{
    public int Id { get; set; }
    public string? UserId { get; set; }
    public string? UserName { get; set; }
    public string Action { get; set; } = string.Empty;
    public string Module { get; set; } = string.Empty;
    public string Description { get; set; } = string.Empty;
    public string? IpAddress { get; set; }
    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;

    public ApplicationUser? User { get; set; }
}
