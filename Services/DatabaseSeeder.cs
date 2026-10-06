using Microsoft.AspNetCore.Identity;
using NardzSAO.Data;
using NardzSAO.Models;

namespace NardzSAO.Services;

public static class DatabaseSeeder
{
    public static async Task SeedAsync(IServiceProvider services)
    {
        using var scope = services.CreateScope();
        var scopedServices = scope.ServiceProvider;
        var context = scopedServices.GetRequiredService<ApplicationDbContext>();
        var roleManager = scopedServices.GetRequiredService<RoleManager<IdentityRole>>();
        var userManager = scopedServices.GetRequiredService<UserManager<ApplicationUser>>();

        foreach (var role in new[] { "Student", "Staff", "Admin" })
        {
            if (!await roleManager.RoleExistsAsync(role))
            {
                await roleManager.CreateAsync(new IdentityRole(role));
            }
        }

        if (!context.AnnouncementCategories.Any())
        {
            context.AnnouncementCategories.AddRange(
                new AnnouncementCategory { Name = "General", Description = "General updates" },
                new AnnouncementCategory { Name = "Student Affairs", Description = "Student support" },
                new AnnouncementCategory { Name = "Scholarships", Description = "Scholarship alerts" },
                new AnnouncementCategory { Name = "Events", Description = "Campus activities" },
                new AnnouncementCategory { Name = "Organizations", Description = "Student org updates" },
                new AnnouncementCategory { Name = "Student Services", Description = "Campus services" },
                new AnnouncementCategory { Name = "Emergency", Description = "Emergency notices" },
                new AnnouncementCategory { Name = "Important Notice", Description = "Must read" }
            );
        }

        var admin = await userManager.FindByEmailAsync("admin@sao.local");
        if (admin is null)
        {
            admin = new ApplicationUser
            {
                UserName = "admin@sao.local",
                Email = "admin@sao.local",
                FullName = "System Administrator",
                StudentNumber = "ADMIN01",
                Department = "Administration",
                EmailConfirmed = true,
                IsActive = true
            };
            var result = await userManager.CreateAsync(admin, "Admin123!");
            if (result.Succeeded)
            {
                await userManager.AddToRoleAsync(admin, "Admin");
            }
        }

        var staff = await userManager.FindByEmailAsync("staff@sao.local");
        if (staff is null)
        {
            staff = new ApplicationUser
            {
                UserName = "staff@sao.local",
                Email = "staff@sao.local",
                FullName = "SAO Staff Member",
                StudentNumber = "SAO1001",
                Department = "Student Affairs",
                EmailConfirmed = true,
                IsActive = true
            };
            var result = await userManager.CreateAsync(staff, "Staff123!");
            if (result.Succeeded)
            {
                await userManager.AddToRoleAsync(staff, "Staff");
            }
        }

        var student = await userManager.FindByEmailAsync("student@sao.local");
        if (student is null)
        {
            student = new ApplicationUser
            {
                UserName = "student@sao.local",
                Email = "student@sao.local",
                FullName = "Nardz Student",
                StudentNumber = "2024001",
                Department = "Information Technology",
                EmailConfirmed = true,
                IsActive = true
            };
            var result = await userManager.CreateAsync(student, "Student123!");
            if (result.Succeeded)
            {
                await userManager.AddToRoleAsync(student, "Student");
            }
        }

        if (!context.Announcements.Any())
        {
            var generalCategory = context.AnnouncementCategories.First(c => c.Name == "General");
            var eventCategory = context.AnnouncementCategories.First(c => c.Name == "Events");
            context.Announcements.AddRange(
                new Announcement
                {
                    Title = "Orientation Week Starts Next Monday",
                    Content = "All new and returning students are expected to attend the orientation week activities beginning next Monday.",
                    Category = AnnouncementCategoryType.General,
                    Priority = AnnouncementPriority.Important,
                    PublishDate = DateTime.UtcNow.AddDays(-2),
                    ExpirationDate = DateTime.UtcNow.AddDays(15),
                    TargetAudience = "All Students",
                    IsFeatured = true,
                    CreatedByUserId = admin.Id,
                    AnnouncementCategoryId = generalCategory.Id
                },
                new Announcement
                {
                    Title = "Campus Leadership Summit Registration Open",
                    Content = "Students may now register for the annual leadership summit and networking event with student leaders and campus partners.",
                    Category = AnnouncementCategoryType.Events,
                    Priority = AnnouncementPriority.Normal,
                    PublishDate = DateTime.UtcNow.AddDays(-1),
                    ExpirationDate = DateTime.UtcNow.AddDays(20),
                    TargetAudience = "All Students",
                    IsFeatured = true,
                    CreatedByUserId = staff.Id,
                    AnnouncementCategoryId = eventCategory.Id
                }
            );
        }

        if (!context.Organizations.Any())
        {
            context.Organizations.AddRange(
                new Organization
                {
                    Name = "IT Society",
                    Description = "A student organization for computer science and IT students to explore technology, leadership, and innovation.",
                    Category = "Academic",
                    President = "Rhea Santos",
                    Adviser = "Prof. C. Martinez",
                    ContactInformation = "itsociety@campus.edu",
                    Status = OrganizationStatus.Approved,
                    CreatedByUserId = staff.Id
                },
                new Organization
                {
                    Name = "Student Volunteers Alliance",
                    Description = "Volunteering and outreach programs for community-building and social responsibility.",
                    Category = "Community",
                    President = "Leo Delos Reyes",
                    Adviser = "Prof. M. Dela Cruz",
                    ContactInformation = "volunteers@campus.edu",
                    Status = OrganizationStatus.Approved,
                    CreatedByUserId = staff.Id
                }
            );
        }

        if (!context.Events.Any())
        {
            context.Events.AddRange(
                new Event
                {
                    Name = "Mental Wellness Week",
                    Description = "A week of activities focused on well-being, resilience, and support services for students.",
                    Location = "Student Center",
                    StartDateTime = DateTime.UtcNow.AddDays(5),
                    EndDateTime = DateTime.UtcNow.AddDays(5).AddHours(3),
                    MaximumParticipants = 150,
                    Organizer = "SAO Office",
                    Category = "Wellness",
                    RegistrationDeadline = DateTime.UtcNow.AddDays(3),
                    CreatedByUserId = staff.Id,
                    PosterUrl = "/images/event-default.svg"
                },
                new Event
                {
                    Name = "Career and Internship Fair",
                    Description = "Connect with employers, internship partners, and alumni about career opportunities.",
                    Location = "Main Gymnasium",
                    StartDateTime = DateTime.UtcNow.AddDays(14),
                    EndDateTime = DateTime.UtcNow.AddDays(14).AddHours(4),
                    MaximumParticipants = 250,
                    Organizer = "Career Center",
                    Category = "Careers",
                    RegistrationDeadline = DateTime.UtcNow.AddDays(9),
                    CreatedByUserId = staff.Id,
                    PosterUrl = "/images/event-default.svg"
                }
            );
        }

        if (!context.Requests.Any())
        {
            context.Requests.AddRange(
                new RequestItem
                {
                    Category = RequestCategory.CertificateRequest,
                    Subject = "Transcript Request",
                    Description = "I would like to request a copy of my official transcript for scholarship application.",
                    DateSubmitted = DateTime.UtcNow.AddDays(-3),
                    Status = RequestStatus.Pending,
                    RequestedByUserId = student.Id
                },
                new RequestItem
                {
                    Category = RequestCategory.StudentAssistance,
                    Subject = "Financial Assistance Consultation",
                    Description = "I would like to consult about available student support and emergency aid options.",
                    DateSubmitted = DateTime.UtcNow.AddDays(-1),
                    Status = RequestStatus.UnderReview,
                    RequestedByUserId = student.Id
                }
            );
        }

        if (!context.Appointments.Any())
        {
            context.Appointments.AddRange(
                new Appointment
                {
                    UserId = student.Id,
                    AppointmentType = "Counseling",
                    PreferredDate = DateTime.UtcNow.AddDays(3),
                    PreferredTime = "09:30 AM",
                    Reason = "Academic guidance and consultation",
                    Status = AppointmentStatus.Pending
                },
                new Appointment
                {
                    UserId = student.Id,
                    AppointmentType = "Document Assistance",
                    PreferredDate = DateTime.UtcNow.AddDays(5),
                    PreferredTime = "01:00 PM",
                    Reason = "Request for guidance on student forms",
                    Status = AppointmentStatus.Approved
                }
            );
        }

        if (!context.Notifications.Any())
        {
            context.Notifications.AddRange(
                new Notification
                {
                    UserId = student.Id,
                    Title = "Welcome to SAOConnect",
                    Message = "Your personalized student portal is ready. Explore announcements, events, and support services.",
                    Type = "Welcome",
                    IsRead = false
                },
                new Notification
                {
                    UserId = student.Id,
                    Title = "Upcoming Event",
                    Message = "Mental Wellness Week is scheduled for next week. Register now to reserve your slot.",
                    Type = "Event",
                    IsRead = false
                }
            );
        }

        await context.SaveChangesAsync();
    }
}
