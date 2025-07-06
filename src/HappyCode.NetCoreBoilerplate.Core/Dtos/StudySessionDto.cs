using System.ComponentModel.DataAnnotations;

namespace HappyCode.NetCoreBoilerplate.Core.Dtos
{
    public class StudySessionDto
    {
        public int Id { get; set; }
        public required string Title { get; set; }
        public required string Description { get; set; }
        public int TeacherId { get; set; }
        public string TeacherName { get; set; }
        public DateTime StartTime { get; set; }
        public DateTime EndTime { get; set; }
        public int MaxCapacity { get; set; }
        public int CurrentCapacity { get; set; }
        public required string Location { get; set; }
        public required string Status { get; set; }
        public DateTime CreatedAt { get; set; }
        public bool IsReserved { get; set; }
    }

    public class StudySessionCreateDto
    {
        [Required]
        [StringLength(200)]
        public required string Title { get; set; }

        [StringLength(500)]
        public required string Description { get; set; }

        [Required]
        public int TeacherId { get; set; }

        [Required]
        public DateTime StartTime { get; set; }

        [Required]
        public DateTime EndTime { get; set; }

        [Required]
        [Range(1, 100)]
        public int MaxCapacity { get; set; }

        [StringLength(100)]
        public required string Location { get; set; }
    }

    public class StudySessionUpdateDto
    {
        [StringLength(200)]
        public required string Title { get; set; }

        [StringLength(500)]
        public required string Description { get; set; }

        public DateTime? StartTime { get; set; }

        public DateTime? EndTime { get; set; }

        [Range(1, 100)]
        public int? MaxCapacity { get; set; }

        [StringLength(100)]
        public required string Location { get; set; }

        [StringLength(50)]
        public required string Status { get; set; }
    }

    public class StudySessionReservationDto
    {
        public int Id { get; set; }
        public int StudySessionId { get; set; }
        public int StudentId { get; set; }
        public string StudentName { get; set; }
        public required string Status { get; set; }
        public DateTime ReservedAt { get; set; }
        public DateTime? CancelledAt { get; set; }
        public required string CancellationReason { get; set; }
    }

    public class StudySessionReservationCreateDto
    {
        [Required]
        public int StudySessionId { get; set; }

        [Required]
        public int StudentId { get; set; }
    }

    public class StudySessionReservationCancelDto
    {
        [StringLength(200)]
        public required string CancellationReason { get; set; }
    }
} 