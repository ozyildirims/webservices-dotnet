using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace HappyCode.NetCoreBoilerplate.Core.Models
{
    [Table("study_session_reservations", Schema = "dbo")]
    public class StudySessionReservation
    {
        [Key]
        public int Id { get; set; }

        [Required]
        public int StudySessionId { get; set; }

        [Required]
        public int StudentId { get; set; }

        [StringLength(50)]
        public string Status { get; set; } = "Reserved";

        public DateTime ReservedAt { get; set; } = DateTime.UtcNow;

        public DateTime? CancelledAt { get; set; }

        [StringLength(200)]
        public string CancellationReason { get; set; }

        [ForeignKey("StudySessionId")]
        public virtual StudySession StudySession { get; set; }

        [ForeignKey("StudentId")]
        public virtual Student Student { get; set; }
    }
} 