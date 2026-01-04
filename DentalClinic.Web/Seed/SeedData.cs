using DentalClinic.Web.Data;
using DentalClinic.Web.Models;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;

namespace DentalClinic.Web.Seed;

public static class SeedData
{
    public static async Task InitializeAsync(IServiceProvider services)
    {
        using var scope = services.CreateScope();
        var provider = scope.ServiceProvider;

        var roleManager = provider.GetRequiredService<RoleManager<IdentityRole>>();
        var userManager = provider.GetRequiredService<UserManager<IdentityUser>>();
        var db = provider.GetRequiredService<AppDbContext>();

        await EnsureRoleAsync(roleManager, "Admin");
        await EnsureRoleAsync(roleManager, "Staff");

        // Default admin (login with Email)
        var adminEmail = "admin@clinic.local";

        // caută după email, iar dacă nu există, caută după vechiul username "admin"
        var admin = await userManager.FindByEmailAsync(adminEmail)
                   ?? await userManager.FindByNameAsync("admin");

        if (admin is null)
        {
            admin = new IdentityUser
            {
                UserName = adminEmail,          // IMPORTANT: UserName = Email
                Email = adminEmail,
                EmailConfirmed = true
            };

            var result = await userManager.CreateAsync(admin, "Admin123!");
            if (!result.Succeeded)
                throw new InvalidOperationException("Failed to create default admin: " +
                    string.Join(", ", result.Errors.Select(e => e.Description)));

            await userManager.AddToRoleAsync(admin, "Admin");
        }
        else
        {
            // dacă există vechiul admin (UserName="admin"), îl “migrezi” să poată loga cu email
            if (!string.Equals(admin.UserName, adminEmail, StringComparison.OrdinalIgnoreCase)
                || !string.Equals(admin.Email, adminEmail, StringComparison.OrdinalIgnoreCase))
            {
                admin.UserName = adminEmail;
                admin.Email = adminEmail;
                admin.EmailConfirmed = true;

                var update = await userManager.UpdateAsync(admin);
                if (!update.Succeeded)
                    throw new InvalidOperationException("Failed to update admin: " +
                        string.Join(", ", update.Errors.Select(e => e.Description)));
            }

            // te asiguri că are rolul Admin
            if (!await userManager.IsInRoleAsync(admin, "Admin"))
                await userManager.AddToRoleAsync(admin, "Admin");
        }

        // Seed domain data if empty
        if (!await db.Doctors.AnyAsync())
        {
            db.Doctors.AddRange(
                new Doctor { FullName = "Dr. Alexandra Popescu", Specialty = "Chirurgie dentară", Phone = "0719 890 68", Email = "alexandra@clinic.local" },
                new Doctor { FullName = "Dr. Cristian Ionescu", Specialty = "Ortodonție", Phone = "0790 678 971", Email = "cristian@clinic.local" }
            );
        }

        if (!await db.Patients.AnyAsync())
        {
            db.Patients.AddRange(
                new Patient { FullName = "Titus Marin", BirthDate = new DateTime(2006, 1, 4), Phone = "0722 000 111", Email = "titus@example.com" },
                new Patient { FullName = "Maria Dumitru", BirthDate = new DateTime(1998, 10, 12), Phone = "0733 222 333", Email = "maria@example.com" }
            );
        }

        if (!await db.Treatments.AnyAsync())
        {
            db.Treatments.AddRange(
                new Treatment { Name = "Consultație", Price = 100, DurationMinutes = 30, Description = "Consultație inițială." },
                new Treatment { Name = "Detartraj", Price = 250, DurationMinutes = 45, Description = "Igienizare profesională." },
                new Treatment { Name = "Plombă", Price = 350, DurationMinutes = 60, Description = "Tratament carie." }
            );
        }

        await db.SaveChangesAsync();

        if (!await db.Appointments.AnyAsync())
        {
            var patient = await db.Patients.FirstAsync();
            var doctor = await db.Doctors.FirstAsync();
            var treatment = await db.Treatments.FirstAsync();

            db.Appointments.Add(new Appointment
            {
                PatientId = patient.Id,
                DoctorId = doctor.Id,
                TreatmentId = treatment.Id,
                Date = DateTime.Today.AddDays(2),
                Time = new TimeSpan(10, 0, 0),
                Notes = "Programare demo",
                Completed = false
            });

            await db.SaveChangesAsync();
        }
    }

    private static async Task EnsureRoleAsync(RoleManager<IdentityRole> roleManager, string roleName)
    {
        if (!await roleManager.RoleExistsAsync(roleName))
            await roleManager.CreateAsync(new IdentityRole(roleName));
    }
}
