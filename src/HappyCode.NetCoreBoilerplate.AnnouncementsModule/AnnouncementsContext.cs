using HappyCode.NetCoreBoilerplate.AnnouncementsModule.Models;
using Microsoft.EntityFrameworkCore;

namespace HappyCode.NetCoreBoilerplate.AnnouncementsModule;

public class AnnouncementsContext : DbContext
{
    public AnnouncementsContext(DbContextOptions<AnnouncementsContext> options)
        : base(options)
    {
    }

    public DbSet<Announcement> Announcements { get; set; } = default!;
    public DbSet<AnnouncementTarget> AnnouncementTargets { get; set; } = default!;
    public DbSet<UserNotification> UserNotifications { get; set; } = default!;
    public DbSet<NotificationSetting> NotificationSettings { get; set; } = default!;
    public DbSet<UserDevice> UserDevices { get; set; } = default!;

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        base.OnModelCreating(modelBuilder);

        modelBuilder.Entity<Announcement>(entity =>
        {
            entity.ToTable("Announcements");
            entity.HasKey(e => e.Id);
            entity.Property(e => e.Title).HasMaxLength(200).IsRequired();
            entity.Property(e => e.Content).IsRequired();
            entity.Property(e => e.StartDate).IsRequired();
            entity.Property(e => e.IsActive).HasDefaultValue(true);
            entity.Property(e => e.Priority).HasDefaultValue(0);
            entity.Property(e => e.CreatedAt).HasDefaultValueSql("GETUTCDATE()");
        });

        modelBuilder.Entity<AnnouncementTarget>(entity =>
        {
            entity.ToTable("AnnouncementTargets");
            entity.HasKey(e => e.Id);
            entity.Property(e => e.TargetType).HasMaxLength(50).IsRequired();
            entity.Property(e => e.TargetId).IsRequired();

            entity.HasOne(e => e.Announcement)
                .WithMany(e => e.Targets)
                .HasForeignKey(e => e.AnnouncementId)
                .OnDelete(DeleteBehavior.Cascade);
        });

        modelBuilder.Entity<UserNotification>(entity =>
        {
            entity.ToTable("UserNotifications");
            entity.HasKey(e => e.Id);
            entity.Property(e => e.IsRead).HasDefaultValue(false);
            entity.Property(e => e.CreatedAt).HasDefaultValueSql("GETUTCDATE()");

            entity.HasOne(e => e.Announcement)
                .WithMany(e => e.UserNotifications)
                .HasForeignKey(e => e.AnnouncementId)
                .OnDelete(DeleteBehavior.Cascade);

            entity.HasIndex(e => new { e.UserId, e.AnnouncementId }).IsUnique();
        });

        modelBuilder.Entity<NotificationSetting>(entity =>
        {
            entity.ToTable("NotificationSettings");
            entity.HasKey(e => e.Id);
            entity.Property(e => e.NotificationType).HasMaxLength(50).IsRequired();
            entity.Property(e => e.IsEnabled).HasDefaultValue(true);
            entity.Property(e => e.CreatedAt).HasDefaultValueSql("GETUTCDATE()");

            entity.HasIndex(e => new { e.UserId, e.NotificationType }).IsUnique();
        });

        modelBuilder.Entity<UserDevice>(entity =>
        {
            entity.ToTable("UserDevices");
            entity.HasKey(e => e.Id);
            entity.Property(e => e.DeviceToken).HasMaxLength(500).IsRequired();
            entity.Property(e => e.DeviceType).HasMaxLength(50).IsRequired();
            entity.Property(e => e.IsActive).HasDefaultValue(true);
            entity.Property(e => e.LastUsedAt).HasDefaultValueSql("GETUTCDATE()");
            entity.Property(e => e.CreatedAt).HasDefaultValueSql("GETUTCDATE()");

            entity.HasIndex(e => new { e.UserId, e.IsActive });
        });
    }
} 