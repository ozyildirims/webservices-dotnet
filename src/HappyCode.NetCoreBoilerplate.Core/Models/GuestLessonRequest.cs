using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace HappyCode.NetCoreBoilerplate.Core.Models
{
    [Table("guest_lesson_requests", Schema = "dbo")]
    public class GuestLessonRequest
    {
        [Key]
        public int Id { get; set; }

        [Required]
        [StringLength(100)]
        public string GuestName { get; set; }

        [Required]
        [StringLength(255)]
        public string Email { get; set; }

        [StringLength(20)]
        public string PhoneNumber { get; set; }

        [Required]
        [StringLength(200)]
        public string Subject { get; set; }

        [StringLength(500)]
        public string Description { get; set; }

        public DateTime? PreferredDate { get; set; }

        [StringLength(100)]
        public string PreferredTime { get; set; }

        [StringLength(50)]
        public string Status { get; set; } = "Pending";

        [StringLength(500)]
        public string AdminNotes { get; set; }

        public DateTime CreatedAt { get; set; } = DateTime.UtcNow;

        public DateTime? UpdatedAt { get; set; }

        public DateTime? ProcessedAt { get; set; }
    }
} 