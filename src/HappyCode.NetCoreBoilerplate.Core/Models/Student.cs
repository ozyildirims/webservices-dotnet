using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace HappyCode.NetCoreBoilerplate.Core.Models
{
    [Table("students", Schema = "dbo")]
    public class Student
    {
        [Key]
        public int Id { get; set; }

        [Required]
        public int UserId { get; set; }

        [StringLength(50)]
        public string StudentNumber { get; set; }

        public DateTime? DateOfBirth { get; set; }

        [StringLength(200)]
        public string Address { get; set; }

        public int? ParentId { get; set; }

        public DateTime CreatedAt { get; set; } = DateTime.UtcNow;

        public DateTime? UpdatedAt { get; set; }

        [ForeignKey("UserId")]
        public virtual User User { get; set; }

        [ForeignKey("ParentId")]
        public virtual Parent Parent { get; set; }

        public virtual ICollection<StudySessionReservation> StudySessionReservations { get; set; } = new HashSet<StudySessionReservation>();
        public virtual ICollection<ExamResult> ExamResults { get; set; } = new HashSet<ExamResult>();
        public virtual ICollection<Attendance> Attendances { get; set; } = new HashSet<Attendance>();
    }
} 