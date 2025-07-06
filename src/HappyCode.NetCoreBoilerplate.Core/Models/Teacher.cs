using System;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace HappyCode.NetCoreBoilerplate.Core.Models
{
    [Table("teachers", Schema = "dbo")]
    public class Teacher
    {
        [Key]
        public int Id { get; set; }

        [Required]
        public int UserId { get; set; }

        [Required]
        [StringLength(20)]
        public required string TeacherNumber { get; set; }

        [Required]
        [StringLength(100)]
        public required string Subject { get; set; }

        [StringLength(500)]
        public required string Bio { get; set; }

        public DateTime HireDate { get; set; }
        public bool IsActive { get; set; }
        public DateTime CreatedAt { get; set; } = DateTime.UtcNow;

        public DateTime? UpdatedAt { get; set; }

        [ForeignKey("UserId")]
        public required User User { get; set; }

        public virtual ICollection<StudySession> StudySessions { get; set; } = new HashSet<StudySession>();
        public virtual ICollection<Exam> Exams { get; set; } = new HashSet<Exam>();
        public virtual ICollection<Lesson> Lessons { get; set; } = new HashSet<Lesson>();
    }
} 