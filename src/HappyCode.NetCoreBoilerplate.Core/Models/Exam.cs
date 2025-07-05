using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace HappyCode.NetCoreBoilerplate.Core.Models
{
    [Table("exams", Schema = "dbo")]
    public class Exam
    {
        [Key]
        public int Id { get; set; }

        [Required]
        [StringLength(200)]
        public string Title { get; set; }

        [StringLength(500)]
        public string Description { get; set; }

        [Required]
        public int TeacherId { get; set; }

        [Required]
        public DateTime ExamDate { get; set; }

        [Required]
        public int DurationMinutes { get; set; }

        [Required]
        public int TotalPoints { get; set; }

        [StringLength(100)]
        public string Subject { get; set; }

        [StringLength(50)]
        public string ExamType { get; set; }

        [StringLength(50)]
        public string Status { get; set; } = "Scheduled";

        public DateTime CreatedAt { get; set; } = DateTime.UtcNow;

        public DateTime? UpdatedAt { get; set; }

        [ForeignKey("TeacherId")]
        public virtual Teacher Teacher { get; set; }

        public virtual ICollection<ExamResult> Results { get; set; } = new HashSet<ExamResult>();
    }
} 