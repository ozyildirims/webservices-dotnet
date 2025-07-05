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

        [StringLength(50)]
        public string TeacherNumber { get; set; }

        [StringLength(100)]
        public string Subject { get; set; }

        [StringLength(200)]
        public string Bio { get; set; }

        public DateTime CreatedAt { get; set; } = DateTime.UtcNow;

        public DateTime? UpdatedAt { get; set; }

        [ForeignKey("UserId")]
        public virtual User User { get; set; }

        public virtual ICollection<StudySession> StudySessions { get; set; } = new HashSet<StudySession>();
        public virtual ICollection<Exam> Exams { get; set; } = new HashSet<Exam>();
        public virtual ICollection<Lesson> Lessons { get; set; } = new HashSet<Lesson>();
    }
} 