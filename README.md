# SAOConnect

SAOConnect is a student affairs office management and student engagement system built with ASP.NET Core MVC and ASP.NET Identity.

## Features

- Student announcements and updates
- Event listing and registration workflow
- Student organization directory
- Student support request tracking
- Appointment scheduling and management
- Concern and case reporting
- Document management
- Role-based dashboards for Student, Staff, and Admin users
- Notification center for user updates
- Audit logging for administrative activity

## Demo Accounts

Student
- Email: student@sao.local
- Password: Student123!

Staff
- Email: staff@sao.local
- Password: Staff123!

Admin
- Email: admin@sao.local
- Password: Admin123!

## Run the app

1. Open a terminal in the project folder.
2. Run:
   ```bash
   dotnet restore
   dotnet build
   dotnet run --urls http://localhost:5050
   ```
3. Visit:
   - http://localhost:5050
   - http://localhost:5050/Identity/Account/Login

## Database

The project uses SQLite and app.db is generated automatically on first run.

## Notes

- The app is configured with seed data for demo content and accounts.
- The login page includes the required jQuery validation script ordering for ASP.NET Identity.
- The dashboard route is available for authenticated users.
