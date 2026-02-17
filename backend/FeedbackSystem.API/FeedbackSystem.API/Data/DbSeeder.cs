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
<<<<<<< HEAD
                new Department { DepartmentName = "Engineer", Description = "Engineering department", IsActive = true },
                new Department { DepartmentName = "Engineering", Description = "Product engineering", IsActive = true },
                new Department { DepartmentName = "HR", Description = "Human resources", IsActive = true },
                new Department { DepartmentName = "Sales", Description = "Revenue team", IsActive = true },
                new Department { DepartmentName = "General", Description = "Default department", IsActive = true }
=======
                new Department { DepartmentId = "dept001", DepartmentName = "Engineering", Description = "Product engineering", IsActive = true },
                new Department { DepartmentId = "dept002", DepartmentName = "HR", Description = "Human resources", IsActive = true },
                new Department { DepartmentId = "dept003", DepartmentName = "Sales", Description = "Revenue team", IsActive = true },
                new Department { DepartmentId = "dept004", DepartmentName = "IT", Description = "Information Technology", IsActive = true },
                new Department { DepartmentId = "dept005", DepartmentName = "General", Description = "Default department", IsActive = true }
>>>>>>> 56232111525bc7137d027d06ad7f98c6289f9025
            );
        }

        // ---------- BADGES (NEW) ----------
        if (!await db.Badges.AnyAsync())
        {
            db.Badges.AddRange(
                new Badge { BadgeId = "badge001", BadgeName = "Team Player", Description = "Collaboration excellence", IconClass = "🤝", IsActive = true },
                new Badge { BadgeId = "badge002", BadgeName = "Problem Solver", Description = "Creative solutions", IconClass = "💡", IsActive = true },
                new Badge { BadgeId = "badge003", BadgeName = "Leader", Description = "Leadership excellence", IconClass = "🚀", IsActive = true },
                new Badge { BadgeId = "badge004", BadgeName = "Innovator", Description = "Innovation and creativity", IconClass = "✨", IsActive = true }
            );
        }

        // Save inserts for Roles, AppSettings, Departments, Badges (if any)
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

        if (string.IsNullOrEmpty(defaultDeptId))
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
<<<<<<< HEAD
                FullName = "admin001",
=======
                UserId = "admin001",
                FullName = "System Administrator",
>>>>>>> 56232111525bc7137d027d06ad7f98c6289f9025
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
            if (string.IsNullOrEmpty(admin.DepartmentId))
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

        // ---------- SAMPLE EMPLOYEES FOR TESTING ----------
        var employeeRole = await db.Roles
            .Where(r => r.RoleName == "Employee")
            .FirstOrDefaultAsync();

        var managerRole = await db.Roles
            .Where(r => r.RoleName == "Manager")
            .FirstOrDefaultAsync();

        if (employeeRole != null && !await db.Users.AnyAsync(u => u.UserId == "emp001"))
        {
            var salesDept = await db.Departments.Where(d => d.DepartmentName == "Sales").Select(d => d.DepartmentId).FirstOrDefaultAsync() ?? defaultDeptId;
            var hrDept = await db.Departments.Where(d => d.DepartmentName == "HR").Select(d => d.DepartmentId).FirstOrDefaultAsync() ?? defaultDeptId;
            var itDept = await db.Departments.Where(d => d.DepartmentName == "IT").Select(d => d.DepartmentId).FirstOrDefaultAsync() ?? defaultDeptId;

            db.Users.AddRange(
                new User
                {
                    UserId = "emp001",
                    FullName = "John Doe",
                    Email = "john.doe@company.com",
                    PasswordHash = PasswordHasher.Hash("Pass@123"),
                    RoleId = employeeRole.RoleId,
                    DepartmentId = defaultDeptId,
                    IsActive = true,
                    CreatedAt = DateTime.UtcNow
                },
                new User
                {
                    UserId = "emp002",
                    FullName = "Jane Smith",
                    Email = "jane.smith@company.com",
                    PasswordHash = PasswordHasher.Hash("Pass@123"),
                    RoleId = employeeRole.RoleId,
                    DepartmentId = salesDept,
                    IsActive = true,
                    CreatedAt = DateTime.UtcNow
                },
                new User
                {
                    UserId = "emp003",
                    FullName = "Robert Johnson",
                    Email = "robert.johnson@company.com",
                    PasswordHash = PasswordHasher.Hash("Pass@123"),
                    RoleId = employeeRole.RoleId,
                    DepartmentId = hrDept,
                    IsActive = true,
                    CreatedAt = DateTime.UtcNow
                },
                new User
                {
                    UserId = "mgr001",
                    FullName = "Sarah Williams",
                    Email = "sarah.williams@company.com",
                    PasswordHash = PasswordHasher.Hash("Pass@123"),
                    RoleId = managerRole?.RoleId ?? employeeRole.RoleId,
                    DepartmentId = itDept,
                    IsActive = true,
                    CreatedAt = DateTime.UtcNow
                },
                new User
                {
                    UserId = "emp004",
                    FullName = "Michael Brown",
                    Email = "michael.brown@company.com",
                    PasswordHash = PasswordHasher.Hash("Pass@123"),
                    RoleId = employeeRole.RoleId,
                    DepartmentId = defaultDeptId,
                    IsActive = true,
                    CreatedAt = DateTime.UtcNow
                }
            );
            await db.SaveChangesAsync();
        }
    }
}
