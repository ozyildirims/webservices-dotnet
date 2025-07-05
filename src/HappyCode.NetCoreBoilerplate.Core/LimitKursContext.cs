using System.Diagnostics.CodeAnalysis;
using HappyCode.NetCoreBoilerplate.Core.Models;
using Microsoft.EntityFrameworkCore;

namespace HappyCode.NetCoreBoilerplate.Core
{
    [ExcludeFromCodeCoverage]
    public partial class LimitKursContext : DbContext
    {
        public LimitKursContext(DbContextOptions<LimitKursContext> options)
            : base(options)
        {
        }

        public virtual DbSet<User> Users { get; set; }
        public virtual DbSet<Student> Students { get; set; }
        public virtual DbSet<Teacher> Teachers { get; set; }
        public virtual DbSet<Parent> Parents { get; set; }
        public virtual DbSet<Admin> Admins { get; set; }
        public virtual DbSet<StudySession> StudySessions { get; set; }
        public virtual DbSet<StudySessionReservation> StudySessionReservations { get; set; }
        public virtual DbSet<Exam> Exams { get; set; }
        public virtual DbSet<ExamResult> ExamResults { get; set; }
        public virtual DbSet<Lesson> Lessons { get; set; }
        public virtual DbSet<Attendance> Attendances { get; set; }
        public virtual DbSet<Announcement> Announcements { get; set; }
        public virtual DbSet<Notification> Notifications { get; set; }
        public virtual DbSet<GuestLessonRequest> GuestLessonRequests { get; set; }

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            modelBuilder.HasAnnotation("ProductVersion", "2.2.6-servicing-10079");

            modelBuilder.Entity<User>(entity =>
            {
                entity.HasIndex(e => e.Username).IsUnique();
                entity.HasIndex(e => e.Email).IsUnique();
                entity.Property(e => e.Username).IsRequired().HasMaxLength(100);
                entity.Property(e => e.Email).IsRequired().HasMaxLength(255);
                entity.Property(e => e.PasswordHash).IsRequired().HasMaxLength(255);
                entity.Property(e => e.Role).IsRequired().HasMaxLength(50);
            });

            modelBuilder.Entity<Student>(entity =>
            {
                entity.HasIndex(e => e.UserId).IsUnique();
                entity.HasIndex(e => e.StudentNumber).IsUnique();
                entity.Property(e => e.StudentNumber).HasMaxLength(50);
                entity.Property(e => e.Address).HasMaxLength(200);

                entity.HasOne(d => d.User)
                    .WithOne(p => p.Student)
                    .HasForeignKey<Student>(d => d.UserId)
                    .OnDelete(DeleteBehavior.Cascade);

                entity.HasOne(d => d.Parent)
                    .WithMany(p => p.Students)
                    .HasForeignKey(d => d.ParentId)
                    .OnDelete(DeleteBehavior.SetNull);
            });

            modelBuilder.Entity<Teacher>(entity =>
            {
                entity.HasIndex(e => e.UserId).IsUnique();
                entity.HasIndex(e => e.TeacherNumber).IsUnique();
                entity.Property(e => e.TeacherNumber).HasMaxLength(50);
                entity.Property(e => e.Subject).HasMaxLength(100);
                entity.Property(e => e.Bio).HasMaxLength(200);

                entity.HasOne(d => d.User)
                    .WithOne(p => p.Teacher)
                    .HasForeignKey<Teacher>(d => d.UserId)
                    .OnDelete(DeleteBehavior.Cascade);
            });

            modelBuilder.Entity<Parent>(entity =>
            {
                entity.HasIndex(e => e.UserId).IsUnique();
                entity.HasIndex(e => e.ParentNumber).IsUnique();
                entity.Property(e => e.ParentNumber).HasMaxLength(50);
                entity.Property(e => e.Address).HasMaxLength(200);

                entity.HasOne(d => d.User)
                    .WithOne(p => p.Parent)
                    .HasForeignKey<Parent>(d => d.UserId)
                    .OnDelete(DeleteBehavior.Cascade);
            });

            modelBuilder.Entity<Admin>(entity =>
            {
                entity.HasIndex(e => e.UserId).IsUnique();
                entity.HasIndex(e => e.AdminNumber).IsUnique();
                entity.Property(e => e.AdminNumber).HasMaxLength(50);
                entity.Property(e => e.Department).HasMaxLength(100);

                entity.HasOne(d => d.User)
                    .WithOne(p => p.Admin)
                    .HasForeignKey<Admin>(d => d.UserId)
                    .OnDelete(DeleteBehavior.Cascade);
            });

            modelBuilder.Entity<StudySession>(entity =>
            {
                entity.Property(e => e.Title).IsRequired().HasMaxLength(200);
                entity.Property(e => e.Description).HasMaxLength(500);
                entity.Property(e => e.Location).HasMaxLength(100);
                entity.Property(e => e.Status).HasMaxLength(50).HasDefaultValue("Scheduled");

                entity.HasOne(d => d.Teacher)
                    .WithMany(p => p.StudySessions)
                    .HasForeignKey(d => d.TeacherId)
                    .OnDelete(DeleteBehavior.Cascade);
            });

            modelBuilder.Entity<StudySessionReservation>(entity =>
            {
                entity.Property(e => e.Status).HasMaxLength(50).HasDefaultValue("Reserved");
                entity.Property(e => e.CancellationReason).HasMaxLength(200);

                entity.HasOne(d => d.StudySession)
                    .WithMany(p => p.Reservations)
                    .HasForeignKey(d => d.StudySessionId)
                    .OnDelete(DeleteBehavior.Cascade);

                entity.HasOne(d => d.Student)
                    .WithMany(p => p.StudySessionReservations)
                    .HasForeignKey(d => d.StudentId)
                    .OnDelete(DeleteBehavior.Cascade);
            });

            modelBuilder.Entity<Exam>(entity =>
            {
                entity.Property(e => e.Title).IsRequired().HasMaxLength(200);
                entity.Property(e => e.Description).HasMaxLength(500);
                entity.Property(e => e.Subject).HasMaxLength(100);
                entity.Property(e => e.ExamType).HasMaxLength(50);
                entity.Property(e => e.Status).HasMaxLength(50).HasDefaultValue("Scheduled");

                entity.HasOne(d => d.Teacher)
                    .WithMany(p => p.Exams)
                    .HasForeignKey(d => d.TeacherId)
                    .OnDelete(DeleteBehavior.Cascade);
            });

            modelBuilder.Entity<ExamResult>(entity =>
            {
                entity.Property(e => e.Grade).HasMaxLength(10);
                entity.Property(e => e.Comments).HasMaxLength(500);

                entity.HasOne(d => d.Exam)
                    .WithMany(p => p.Results)
                    .HasForeignKey(d => d.ExamId)
                    .OnDelete(DeleteBehavior.Cascade);

                entity.HasOne(d => d.Student)
                    .WithMany(p => p.ExamResults)
                    .HasForeignKey(d => d.StudentId)
                    .OnDelete(DeleteBehavior.Cascade);
            });

            modelBuilder.Entity<Lesson>(entity =>
            {
                entity.Property(e => e.Title).IsRequired().HasMaxLength(200);
                entity.Property(e => e.Description).HasMaxLength(500);
                entity.Property(e => e.Subject).HasMaxLength(100);
                entity.Property(e => e.Location).HasMaxLength(100);
                entity.Property(e => e.Status).HasMaxLength(50).HasDefaultValue("Scheduled");

                entity.HasOne(d => d.Teacher)
                    .WithMany(p => p.Lessons)
                    .HasForeignKey(d => d.TeacherId)
                    .OnDelete(DeleteBehavior.Cascade);
            });

            modelBuilder.Entity<Attendance>(entity =>
            {
                entity.Property(e => e.Status).IsRequired().HasMaxLength(20);
                entity.Property(e => e.Notes).HasMaxLength(200);

                entity.HasOne(d => d.Lesson)
                    .WithMany(p => p.Attendances)
                    .HasForeignKey(d => d.LessonId)
                    .OnDelete(DeleteBehavior.Cascade);

                entity.HasOne(d => d.Student)
                    .WithMany(p => p.Attendances)
                    .HasForeignKey(d => d.StudentId)
                    .OnDelete(DeleteBehavior.Cascade);
            });

            modelBuilder.Entity<Announcement>(entity =>
            {
                entity.Property(e => e.Title).IsRequired().HasMaxLength(200);
                entity.Property(e => e.Content).IsRequired().HasMaxLength(1000);
                entity.Property(e => e.TargetRole).HasMaxLength(50);
                entity.Property(e => e.Priority).HasMaxLength(50).HasDefaultValue("Normal");
                entity.Property(e => e.Status).HasMaxLength(50).HasDefaultValue("Draft");

                entity.HasOne(d => d.CreatedBy)
                    .WithMany()
                    .HasForeignKey(d => d.CreatedById)
                    .OnDelete(DeleteBehavior.Cascade);
            });

            modelBuilder.Entity<Notification>(entity =>
            {
                entity.Property(e => e.Title).IsRequired().HasMaxLength(200);
                entity.Property(e => e.Message).IsRequired().HasMaxLength(500);
                entity.Property(e => e.Type).HasMaxLength(50).HasDefaultValue("Info");
                entity.Property(e => e.Status).HasMaxLength(50).HasDefaultValue("Unread");

                entity.HasOne(d => d.User)
                    .WithMany()
                    .HasForeignKey(d => d.UserId)
                    .OnDelete(DeleteBehavior.Cascade);

                entity.HasOne(d => d.Announcement)
                    .WithMany(p => p.Notifications)
                    .HasForeignKey(d => d.AnnouncementId)
                    .OnDelete(DeleteBehavior.SetNull);
            });

            modelBuilder.Entity<GuestLessonRequest>(entity =>
            {
                entity.Property(e => e.GuestName).IsRequired().HasMaxLength(100);
                entity.Property(e => e.Email).IsRequired().HasMaxLength(255);
                entity.Property(e => e.PhoneNumber).HasMaxLength(20);
                entity.Property(e => e.Subject).IsRequired().HasMaxLength(200);
                entity.Property(e => e.Description).HasMaxLength(500);
                entity.Property(e => e.PreferredTime).HasMaxLength(100);
                entity.Property(e => e.Status).HasMaxLength(50).HasDefaultValue("Pending");
                entity.Property(e => e.AdminNotes).HasMaxLength(500);
            });
        }
    }
} 