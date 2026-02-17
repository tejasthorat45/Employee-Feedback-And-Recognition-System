using Microsoft.EntityFrameworkCore;
using FeedbackSystem.API.Entities;

namespace FeedbackSystem.API.Data;

public class AppDbContext : DbContext
{
    public AppDbContext(DbContextOptions<AppDbContext> options) : base(options)
    {
    }

    public DbSet<Role> Roles => Set<Role>();
    public DbSet<User> Users => Set<User>();
    public DbSet<Category> Categories => Set<Category>();
    public DbSet<Feedback> Feedbacks => Set<Feedback>();
    public DbSet<FeedbackReview> FeedbackReviews => Set<FeedbackReview>();
    public DbSet<Recognition> Recognitions => Set<Recognition>();
    public DbSet<Badge> Badges => Set<Badge>();
    public DbSet<AppSetting> AppSettings => Set<AppSetting>();
    public DbSet<Notification> Notifications => Set<Notification>();
    public DbSet<ActivityLog> ActivityLogs => Set<ActivityLog>();

    // NEW: Departments
    public DbSet<Department> Departments => Set<Department>();

    protected override void OnModelCreating(ModelBuilder model)
    {
        // ---------- ROLES ----------
        model.Entity<Role>(e =>
        {
            e.ToTable("Roles");
            e.HasKey(x => x.RoleId);
            e.Property(x => x.RoleName).IsRequired().HasMaxLength(50);
            e.Property(x => x.IsActive).HasDefaultValue(true);
            e.Property(x => x.CreatedAt).HasDefaultValueSql("GETDATE()");
            e.HasIndex(x => x.RoleName).IsUnique();
        });

        // ---------- DEPARTMENTS (NEW) ----------
        model.Entity<Department>(e =>
        {
            e.ToTable("Departments");
            e.HasKey(x => x.DepartmentId);

            e.Property(x => x.DepartmentId).IsRequired().HasMaxLength(20);
            e.Property(x => x.DepartmentName).IsRequired().HasMaxLength(100);
            e.Property(x => x.Description).HasMaxLength(225);
            e.Property(x => x.IsActive).HasDefaultValue(true);
            e.Property(x => x.CreatedAt).HasDefaultValueSql("GETDATE()");

            e.HasIndex(x => x.DepartmentName).IsUnique();
        });

        // ---------- USERS ----------
        model.Entity<User>(e =>
        {
            e.ToTable("Users");
            e.HasKey(x => x.UserId);
            e.Property(x => x.UserId).IsRequired().HasMaxLength(20);
            e.Property(x => x.FullName).IsRequired().HasMaxLength(50);
            e.Property(x => x.Email).IsRequired().HasMaxLength(100);
            e.Property(x => x.PasswordHash).IsRequired().HasMaxLength(225);
            e.Property(x => x.DepartmentId).IsRequired().HasMaxLength(20);
            e.Property(x => x.IsActive).HasDefaultValue(true);
            e.Property(x => x.CreatedAt).HasDefaultValueSql("GETDATE()");
            e.HasIndex(x => x.Email).IsUnique();

            e.HasOne(x => x.Role)
             .WithMany(r => r.Users)
             .HasForeignKey(x => x.RoleId)
             .OnDelete(DeleteBehavior.Restrict)
             .HasConstraintName("FK_Users_Roles");

            // ✅ NEW: User → Department (many-to-one)
            e.HasOne(x => x.Department)
             .WithMany(d => d.Users)
             .HasForeignKey(x => x.DepartmentId)
             .OnDelete(DeleteBehavior.Restrict)
             .HasConstraintName("FK_Users_Departments");

            // (Optional but useful) index for lookups by department
            e.HasIndex(x => x.DepartmentId).HasDatabaseName("IX_Users_DepartmentId");
        });

        // ---------- CATEGORIES ----------
        model.Entity<Category>(e =>
        {
            e.ToTable("Categories");
            e.HasKey(x => x.CategoryId);
            e.Property(x => x.CategoryId).IsRequired().HasMaxLength(20);
            e.Property(x => x.CategoryName).IsRequired().HasMaxLength(100);
            e.Property(x => x.Description).HasMaxLength(225);
            e.Property(x => x.IsActive).HasDefaultValue(true);
            e.Property(x => x.CreatedAt).HasDefaultValueSql("GETDATE()");
            e.HasIndex(x => x.CategoryName).IsUnique();
        });

        // ---------- BADGES ----------
        model.Entity<Badge>(e =>
        {
            e.ToTable("Badges");
            e.HasKey(x => x.BadgeId);
            e.Property(x => x.BadgeId).IsRequired().HasMaxLength(20);
            e.Property(x => x.BadgeName).IsRequired().HasMaxLength(100);
            e.Property(x => x.Description).HasMaxLength(225);
            e.Property(x => x.IconClass).HasMaxLength(50);
            e.Property(x => x.IsActive).HasDefaultValue(true);
            e.Property(x => x.CreatedAt).HasDefaultValueSql("GETDATE()");
            e.HasIndex(x => x.BadgeName).IsUnique();
        });

        // ---------- FEEDBACK ----------
        model.Entity<Feedback>(e =>
        {
            e.ToTable("Feedback");
            e.HasKey(x => x.FeedbackId);
            e.Property(x => x.Comments).IsRequired();
            e.Property(x => x.IsAnonymous).HasDefaultValue(false);
            e.Property(x => x.CreatedAt).HasDefaultValueSql("GETDATE()");

            e.HasOne(x => x.FromUser)
             .WithMany(u => u.FeedbacksFrom)
             .HasForeignKey(x => x.FromUserId)
             .OnDelete(DeleteBehavior.Restrict)
             .HasConstraintName("FK_Feedback_FromUser");

            e.HasOne(x => x.ToUser)
             .WithMany(u => u.FeedbacksTo)
             .HasForeignKey(x => x.ToUserId)
             .OnDelete(DeleteBehavior.Restrict)
             .HasConstraintName("FK_Feedback_ToUser");

            e.HasOne(x => x.Category)
             .WithMany(c => c.Feedbacks)
             .HasForeignKey(x => x.CategoryId)
             .OnDelete(DeleteBehavior.Restrict)
             .HasConstraintName("FK_Feedback_Category");

            e.HasIndex(x => x.CategoryId).HasDatabaseName("IX_Feedback_CategoryId");
            e.HasIndex(x => x.ToUserId).HasDatabaseName("IX_Feedback_ToUserId");
            e.HasIndex(x => x.FromUserId).HasDatabaseName("IX_Feedback_FromUserId");
        });

        // ---------- FEEDBACK REVIEW ----------
        model.Entity<FeedbackReview>(e =>
        {
            e.ToTable("FeedbackReview");
            e.HasKey(x => x.ReviewId);
            e.Property(x => x.ReviewedBy).HasMaxLength(20);
            e.Property(x => x.Status).IsRequired().HasMaxLength(20);
            e.Property(x => x.Remarks).HasMaxLength(225);
            e.Property(x => x.ReviewedAt).HasDefaultValueSql("GETDATE()");
        });

        // ---------- RECOGNITION ----------
        model.Entity<Recognition>(e =>
        {
            e.ToTable("Recognition");
            e.HasKey(x => x.RecognitionId);

            e.Property(x => x.Message).IsRequired().HasMaxLength(500);
            e.Property(x => x.CreatedAt).HasDefaultValueSql("GETDATE()");

            // ✅ Points with check constraint (1–10)
            e.Property(x => x.Points).IsRequired();

            // CHECK constraint at table-level (SQL Server)
            e.ToTable(t =>
            {
                t.HasCheckConstraint("CK_Recognition_Points_Range", "[Points] BETWEEN 1 AND 10");
            });

            e.HasOne(x => x.FromUser)
             .WithMany(u => u.RecognitionsFrom)
             .HasForeignKey(x => x.FromUserId)
             .OnDelete(DeleteBehavior.Restrict)
             .HasConstraintName("FK_Recognition_FromUser");

            e.HasOne(x => x.ToUser)
             .WithMany(u => u.RecognitionsTo)
             .HasForeignKey(x => x.ToUserId)
             .OnDelete(DeleteBehavior.Restrict)
             .HasConstraintName("FK_Recognition_ToUser");

            // ✅ Badge relationship (replaces Category)
            e.HasOne(x => x.Badge)
             .WithMany(b => b.Recognitions)
             .HasForeignKey(x => x.BadgeId)
             .OnDelete(DeleteBehavior.Restrict)
             .HasConstraintName("FK_Recognition_Badge");

            e.HasIndex(x => x.ToUserId).HasDatabaseName("IX_Recognition_ToUserId");
            e.HasIndex(x => x.BadgeId).HasDatabaseName("IX_Recognition_BadgeId");
        });
        // ---------- APP SETTINGS ----------
        model.Entity<AppSetting>(e =>
        {
            e.ToTable("AppSettings");
            e.HasKey(x => x.SettingKey);
            e.Property(x => x.SettingKey).HasMaxLength(100);
            e.Property(x => x.SettingValue).IsRequired().HasMaxLength(255);
            e.Property(x => x.UpdatedAt).HasDefaultValueSql("GETDATE()");
        });

        // ---------- NOTIFICATIONS ----------
        model.Entity<Notification>(e =>
        {
            e.ToTable("Notifications");
            e.HasKey(x => x.NotificationId);
            e.Property(x => x.Title).HasMaxLength(100);
            e.Property(x => x.Message).HasMaxLength(500);
            e.Property(x => x.IsRead).HasDefaultValue(false);
            e.Property(x => x.CreatedAt).HasDefaultValueSql("GETDATE()");

            e.HasOne(x => x.User)
             .WithMany(u => u.Notifications)
             .HasForeignKey(x => x.UserId)
             .OnDelete(DeleteBehavior.Restrict)
             .HasConstraintName("FK_Notification_User");
        });

        // ---------- ACTIVITY LOG ----------
        model.Entity<ActivityLog>(e =>
        {
            e.ToTable("ActivityLog");
            e.HasKey(x => x.ActivityId);
            e.Property(x => x.ActionType).IsRequired().HasMaxLength(50);
            e.Property(x => x.EntityType).HasMaxLength(50);
            e.Property(x => x.CreatedAt).HasDefaultValueSql("GETDATE()");

            e.HasOne(x => x.User)
             .WithMany(u => u.ActivityLogs)
             .HasForeignKey(x => x.UserId)
             .OnDelete(DeleteBehavior.Restrict)
             .HasConstraintName("FK_ActivityLog_User");

            e.HasIndex(x => x.CreatedAt).HasDatabaseName("IX_ActivityLog_CreatedAt");
        });
    }
}