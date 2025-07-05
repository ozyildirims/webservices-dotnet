using System.ComponentModel.DataAnnotations;

namespace HappyCode.NetCoreBoilerplate.Core.Dtos
{
    public class ExamDto
    {
        public int Id { get; set; }
        public string Title { get; set; }
        public string Description { get; set; }
        public int TeacherId { get; set; }
        public string TeacherName { get; set; }
        public DateTime ExamDate { get; set; }
        public int DurationMinutes { get; set; }
        public int TotalPoints { get; set; }
        public string Subject { get; set; }
        public string ExamType { get; set; }
        public string Status { get; set; }
        public DateTime CreatedAt { get; set; }
    }

    public class ExamCreateDto
    {
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
        [Range(1, 480)]
        public int DurationMinutes { get; set; }

        [Required]
        [Range(1, 1000)]
        public int TotalPoints { get; set; }

        [StringLength(100)]
        public string Subject { get; set; }

        [StringLength(50)]
        public string ExamType { get; set; }
    }

    public class ExamUpdateDto
    {
        [StringLength(200)]
        public string Title { get; set; }

        [StringLength(500)]
        public string Description { get; set; }

        public DateTime? ExamDate { get; set; }

        [Range(1, 480)]
        public int? DurationMinutes { get; set; }

        [Range(1, 1000)]
        public int? TotalPoints { get; set; }

        [StringLength(100)]
        public string Subject { get; set; }

        [StringLength(50)]
        public string ExamType { get; set; }

        [StringLength(50)]
        public string Status { get; set; }
    }

    public class ExamResultDto
    {
        public int Id { get; set; }
        public int ExamId { get; set; }
        public string ExamTitle { get; set; }
        public int StudentId { get; set; }
        public string StudentName { get; set; }
        public decimal Score { get; set; }
        public int TotalPoints { get; set; }
        public decimal? Percentage { get; set; }
        public string Grade { get; set; }
        public string Comments { get; set; }
        public DateTime? CompletedAt { get; set; }
        public DateTime CreatedAt { get; set; }
    }

    public class ExamResultCreateDto
    {
        [Required]
        public int ExamId { get; set; }

        [Required]
        public int StudentId { get; set; }

        [Required]
        [Range(0, 1000)]
        public decimal Score { get; set; }

        [Required]
        [Range(1, 1000)]
        public int TotalPoints { get; set; }

        [StringLength(10)]
        public string Grade { get; set; }

        [StringLength(500)]
        public string Comments { get; set; }

        public DateTime? CompletedAt { get; set; }
    }

    public class ExamResultUpdateDto
    {
        [Range(0, 1000)]
        public decimal? Score { get; set; }

        [Range(1, 1000)]
        public int? TotalPoints { get; set; }

        [StringLength(10)]
        public string Grade { get; set; }

        [StringLength(500)]
        public string Comments { get; set; }

        public DateTime? CompletedAt { get; set; }
    }
} 