using Microsoft.EntityFrameworkCore;
using Sisora.API.Models.Entities;

namespace Sisora.API.Data;

public static class DatabaseSeeder
{
    public static async Task SeedAsync(AppDbContext context)
    {
        // only seed if no admin exists
        if (await context.Admins.AnyAsync())
            return;

        var admin = new Admin
        {
            FullName = "Sisora Admin",
            Email = "admin@sisora.lk",
            PasswordHash = BCrypt.Net.BCrypt.HashPassword("Admin@1234")
        };

        await context.Admins.AddAsync(admin);
        await context.SaveChangesAsync();

        Console.WriteLine("✓ Admin seeded successfully.");
    }
}