using Microsoft.AspNetCore.Identity.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore;
using NardzSAO.Models;

namespace NardzSAO.Data;

public class ApplicationDbContext(DbContextOptions<ApplicationDbContext> options) : IdentityDbContext<ApplicationUser>(options)
{
    public DbSet<AnnouncementCategory> AnnouncementCategories => Set<AnnouncementCategory>();
    public DbSet<Announcement> Announcements => Set<Announcement>();
    public DbSet<Event> Events => Set<Event>();
    public DbSet<EventRegistration> EventRegistrations => Set<EventRegistration>();
    public DbSet<Attendance> Attendances => Set<Attendance>();
    public DbSet<RequestItem> Requests => Set<RequestItem>();
    public DbSet<RequestAttachment> RequestAttachments => Set<RequestAttachment>();
    public DbSet<Appointment> Appointments => Set<Appointment>();
    public DbSet<Concern> Concerns => Set<Concern>();
    public DbSet<Organization> Organizations => Set<Organization>();
    public DbSet<OrganizationMembership> OrganizationMemberships => Set<OrganizationMembership>();
    public DbSet<Notification> Notifications => Set<Notification>();
    public DbSet<Document> Documents => Set<Document>();
    public DbSet<AuditLog> AuditLogs => Set<AuditLog>();

    protected override void OnModelCreating(ModelBuilder builder)
    {
        base.OnModelCreating(builder);

        builder.Entity<ApplicationUser>()
            .HasIndex(u => u.StudentNumber)
            .IsUnique();

        builder.Entity<Announcement>()
            .HasOne(a => a.CreatedByUser)
            .WithMany(u => u.Announcements)
            .HasForeignKey(a => a.CreatedByUserId)
            .OnDelete(DeleteBehavior.Restrict);

        builder.Entity<Announcement>()
            .HasOne(a => a.AnnouncementCategory)
            .WithMany(c => c.Announcements)
            .HasForeignKey(a => a.AnnouncementCategoryId)
            .OnDelete(DeleteBehavior.Cascade);

        builder.Entity<EventRegistration>()
            .HasOne(r => r.Event)
            .WithMany(e => e.Registrations)
            .HasForeignKey(r => r.EventId)
            .OnDelete(DeleteBehavior.Cascade);

        builder.Entity<EventRegistration>()
            .HasOne(r => r.User)
            .WithMany(u => u.EventRegistrations)
            .HasForeignKey(r => r.UserId)
            .OnDelete(DeleteBehavior.Cascade);

        builder.Entity<Attendance>()
            .HasOne(a => a.Event)
            .WithMany(e => e.Attendances)
            .HasForeignKey(a => a.EventId)
            .OnDelete(DeleteBehavior.Cascade);

        builder.Entity<Attendance>()
            .HasOne(a => a.User)
            .WithMany()
            .HasForeignKey(a => a.UserId)
            .OnDelete(DeleteBehavior.Cascade);

        builder.Entity<RequestItem>()
            .HasOne(r => r.RequestedByUser)
            .WithMany(u => u.Requests)
            .HasForeignKey(r => r.RequestedByUserId)
            .OnDelete(DeleteBehavior.Cascade);

        builder.Entity<RequestAttachment>()
            .HasOne(a => a.Request)
            .WithMany(r => r.RequestAttachments)
            .HasForeignKey(a => a.RequestId)
            .OnDelete(DeleteBehavior.Cascade);

        builder.Entity<Appointment>()
            .HasOne(a => a.User)
            .WithMany(u => u.Appointments)
            .HasForeignKey(a => a.UserId)
            .OnDelete(DeleteBehavior.Cascade);

        builder.Entity<Concern>()
            .HasOne(c => c.User)
            .WithMany(u => u.Concerns)
            .HasForeignKey(c => c.UserId)
            .OnDelete(DeleteBehavior.Cascade);

        builder.Entity<OrganizationMembership>()
            .HasOne(m => m.Organization)
            .WithMany(o => o.Memberships)
            .HasForeignKey(m => m.OrganizationId)
            .OnDelete(DeleteBehavior.Cascade);

        builder.Entity<OrganizationMembership>()
            .HasOne(m => m.User)
            .WithMany(u => u.OrganizationMemberships)
            .HasForeignKey(m => m.UserId)
            .OnDelete(DeleteBehavior.Cascade);

        builder.Entity<Notification>()
            .HasOne(n => n.User)
            .WithMany(u => u.Notifications)
            .HasForeignKey(n => n.UserId)
            .OnDelete(DeleteBehavior.Cascade);

        builder.Entity<Document>()
            .HasOne(d => d.User)
            .WithMany(u => u.Documents)
            .HasForeignKey(d => d.UserId)
            .OnDelete(DeleteBehavior.Cascade);

        builder.Entity<AuditLog>()
            .HasOne(a => a.User)
            .WithMany(u => u.AuditLogs)
            .HasForeignKey(a => a.UserId)
            .OnDelete(DeleteBehavior.SetNull);
    }
}

