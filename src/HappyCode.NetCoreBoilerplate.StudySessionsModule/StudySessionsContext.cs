using HappyCode.NetCoreBoilerplate.StudySessionsModule.Models;
using Microsoft.EntityFrameworkCore;

namespace HappyCode.NetCoreBoilerplate.StudySessionsModule;

public class StudySessionsContext : DbContext
{
    public StudySessionsContext(DbContextOptions<StudySessionsContext> options)
        : base(options)
    {
    }

    public DbSet<StudySession> StudySessions { get; set; } = default!;
    public DbSet<StudySessionReservation> StudySessionReservations { get; set; } = default!;
    public DbSet<StudySessionWaitlist> StudySessionWaitlist { get; set; } = default!;
    public DbSet<StudySessionRecurrenceRule> StudySessionRecurrenceRules { get; set; } = default!;

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        base.OnModelCreating(modelBuilder);

        modelBuilder.Entity<StudySession>(entity =>
        {
            entity.ToTable("StudySessions");
            entity.HasKey(e => e.Id);
            entity.Property(e => e.Title).HasMaxLength(200).IsRequired();
            entity.Property(e => e.Location).HasMaxLength(200);
            entity.Property(e => e.IsActive).HasDefaultValue(true);
            entity.Property(e => e.IsCancelled).HasDefaultValue(false);
            entity.Property(e => e.CurrentCapacity).HasDefaultValue(0);
            entity.Property(e => e.CreatedAt).HasDefaultValueSql("GETUTCDATE()");

            entity.HasOne(e => e.Teacher)
                .WithMany()
                .HasForeignKey(e => e.TeacherId)
                .OnDelete(DeleteBehavior.Restrict);

            entity.HasOne(e => e.RecurrenceRule)
                .WithOne(e => e.Session)
                .HasForeignKey<StudySessionRecurrenceRule>(e => e.SessionId)
                .OnDelete(DeleteBehavior.Cascade);

            entity.HasMany(e => e.Reservations)
                .WithOne(e => e.Session)
                .HasForeignKey(e => e.SessionId)
                .OnDelete(DeleteBehavior.Cascade);

            entity.HasMany(e => e.WaitlistEntries)
                .WithOne(e => e.Session)
                .HasForeignKey(e => e.SessionId)
                .OnDelete(DeleteBehavior.Cascade);
        });

        modelBuilder.Entity<StudySessionReservation>(entity =>
        {
            entity.ToTable("StudySessionReservations");
            entity.HasKey(e => e.Id);
            entity.Property(e => e.Status).HasMaxLength(50).IsRequired();
            entity.Property(e => e.CreatedAt).HasDefaultValueSql("GETUTCDATE()");

            entity.HasOne(e => e.Student)
                .WithMany()
                .HasForeignKey(e => e.StudentId)
                .OnDelete(DeleteBehavior.Restrict);

            entity.HasIndex(e => new { e.SessionId, e.StudentId }).IsUnique();
        });

        modelBuilder.Entity<StudySessionWaitlist>(entity =>
        {
            entity.ToTable("StudySessionWaitlist");
            entity.HasKey(e => e.Id);
            entity.Property(e => e.Status).HasMaxLength(50).IsRequired();
            entity.Property(e => e.CreatedAt).HasDefaultValueSql("GETUTCDATE()");

            entity.HasOne(e => e.Student)
                .WithMany()
                .HasForeignKey(e => e.StudentId)
                .OnDelete(DeleteBehavior.Restrict);

            entity.HasIndex(e => new { e.SessionId, e.StudentId }).IsUnique();
            entity.HasIndex(e => new { e.SessionId, e.Position }).IsUnique();
        });

        modelBuilder.Entity<StudySessionRecurrenceRule>(entity =>
        {
            entity.ToTable("StudySessionRecurrenceRules");
            entity.HasKey(e => e.Id);
            entity.Property(e => e.Pattern).HasMaxLength(50).IsRequired();
            entity.Property(e => e.DaysOfWeek).HasMaxLength(100);
            entity.Property(e => e.CreatedAt).HasDefaultValueSql("GETUTCDATE()");
        });
    }
} 