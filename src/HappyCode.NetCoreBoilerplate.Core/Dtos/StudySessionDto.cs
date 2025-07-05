using System.ComponentModel.DataAnnotations;

namespace HappyCode.NetCoreBoilerplate.Core.Dtos
{
    public class StudySessionDto
    {
        public int Id { get; set; }
        public string Title { get; set; }
        public string Description { get; set; }
        public int TeacherId { get; set; }
        public string TeacherName { get; set; }
        public DateTime StartTime { get; set; }
        public DateTime EndTime { get; set; }
        public int MaxCapacity { get; set; }
        public int CurrentCapacity { get; set; }
        public string Location { get; set; }
        public string Status { get; set; }
        public DateTime CreatedAt { get; set; }
        public bool IsReserved { get; set; }
    }

    public class StudySessionCreateDto
    {
        [Required]
        [StringLength(200)]
        public string Title { get; set; }

        [StringLength(500)]
        public string Description { get; set; }

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
        public string Location { get; set; }
    }

    public class StudySessionUpdateDto
    {
        [StringLength(200)]
        public string Title { get; set; }

        [StringLength(500)]
        public string Description { get; set; }

        public DateTime? StartTime { get; set; }

        public DateTime? EndTime { get; set; }

        [Range(1, 100)]
        public int? MaxCapacity { get; set; }

        [StringLength(100)]
        public string Location { get; set; }

        [StringLength(50)]
        public string Status { get; set; }
    }

    public class StudySessionReservationDto
    {
        public int Id { get; set; }
        public int StudySessionId { get; set; }
        public int StudentId { get; set; }
        public string StudentName { get; set; }
        public string Status { get; set; }
        public DateTime ReservedAt { get; set; }
        public DateTime? CancelledAt { get; set; }
        public string CancellationReason { get; set; }
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
        public string CancellationReason { get; set; }
    }
} 