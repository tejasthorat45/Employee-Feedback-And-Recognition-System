using FeedbackSystem.API.Entities;
using FeedbackSystem.API.Security;
using Microsoft.EntityFrameworkCore;

namespace FeedbackSystem.API.Data;

public static class DbSeeder
{
    public static async Task SeedAsync(AppDbContext db)
    {
        // Ensure DB and migrations are applied
        await db.Database.MigrateAsync();

        // ---------- ROLES ----------
        if (!await db.Roles.AnyAsync())
        {
            db.Roles.AddRange(
                new Role { RoleName = "Admin", IsActive = true },
                new Role { RoleName = "Manager", IsActive = true },
                new Role { RoleName = "Employee", IsActive = true }
            );
        }

        // ---------- APP SETTINGS ----------
        if (!await db.AppSettings.AnyAsync())
        {
            db.AppSettings.AddRange(
                new AppSetting { SettingKey = "feedbackEnabled", SettingValue = "true" },
                new AppSetting { SettingKey = "anonymous", SettingValue = "false" },
                new AppSetting { SettingKey = "recognitionEnabled", SettingValue = "true" },
                new AppSetting { SettingKey = "MaxFeedPerMonth", SettingValue = "5" }
            );
        }

        // ---------- DEPARTMENTS (NEW) ----------
        if (!await db.Departments.AnyAsync())
        {
            db.Departments.AddRange(
                new Department { DepartmentName = "Engineer", Description = "Engineering department", IsActive = true },
                new Department { DepartmentName = "Engineering", Description = "Product engineering", IsActive = true },
                new Department { DepartmentName = "HR", Description = "Human resources", IsActive = true },
                new Department { DepartmentName = "Sales", Description = "Revenue team", IsActive = true },
                new Department { DepartmentName = "General", Description = "Default department", IsActive = true }
            );
        }

        // Save inserts for Roles, AppSettings, Departments (if any)
        await db.SaveChangesAsync();

        // Get IDs we need for relationships
        var adminRoleId = await db.Roles
            .Where(r => r.RoleName == "Admin")
            .Select(r => r.RoleId)
            .FirstAsync();

        // Prefer Engineer; fall back to Engineering, then General
        var defaultDeptId = await db.Departments
            .Where(d => d.DepartmentName == "Engineer")
            .Select(d => d.DepartmentId)
            .FirstOrDefaultAsync();

        if (defaultDeptId == 0)
        {
            defaultDeptId = await db.Departments
                .Where(d => d.DepartmentName == "Engineering")
                .Select(d => d.DepartmentId)
                .FirstOrDefaultAsync();
        }

        if (defaultDeptId == 0)
        {
            defaultDeptId = await db.Departments
                .Where(d => d.DepartmentName == "General")
                .Select(d => d.DepartmentId)
                .FirstAsync();
        }

        // ---------- ADMIN USER ----------
        var admin = await db.Users.FirstOrDefaultAsync(u => u.Email == "admin@local");
        if (admin is null)
        {
            db.Users.Add(new User
            {
                FullName = "admin001",
                Email = "admin@local",
                PasswordHash = PasswordHasher.Hash("Admin@123"),
                RoleId = adminRoleId,
                DepartmentId = defaultDeptId, // ✅ ensure required FK is set
                IsActive = true,
                CreatedAt = DateTime.UtcNow
            });
            await db.SaveChangesAsync();
        }
        else
        {
            // If DepartmentId was missing (e.g., older DB), backfill it safely
            if (admin.DepartmentId == 0)
            {
                admin.DepartmentId = defaultDeptId;
            }

            if (admin.FullName != "admin001")
            {
                admin.FullName = "admin001";
            }

            if (admin.Email != "admin@local")
            {
                admin.Email = "admin@local";
            }

            if (admin.DepartmentId == 0 || admin.FullName != "admin001" || admin.Email != "admin@local")
            {
                await db.SaveChangesAsync();
            }
        }
    }
}
