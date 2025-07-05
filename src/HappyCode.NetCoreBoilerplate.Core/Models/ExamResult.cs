using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace HappyCode.NetCoreBoilerplate.Core.Models
{
    [Table("exam_results", Schema = "dbo")]
    public class ExamResult
    {
        [Key]
        public int Id { get; set; }

        [Required]
        public int ExamId { get; set; }

        [Required]
        public int StudentId { get; set; }

        [Required]
        public decimal Score { get; set; }

        [Required]
        public int TotalPoints { get; set; }

        public decimal? Percentage { get; set; }

        [StringLength(10)]
        public string Grade { get; set; }

        [StringLength(500)]
        public string Comments { get; set; }

        public DateTime? CompletedAt { get; set; }

        public DateTime CreatedAt { get; set; } = DateTime.UtcNow;

        public DateTime? UpdatedAt { get; set; }

        [ForeignKey("ExamId")]
        public virtual Exam Exam { get; set; }

        [ForeignKey("StudentId")]
        public virtual Student Student { get; set; }
    }
} 