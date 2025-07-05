using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace HappyCode.NetCoreBoilerplate.Core.Models
{
    [Table("attendances", Schema = "dbo")]
    public class Attendance
    {
        [Key]
        public int Id { get; set; }

        [Required]
        public int LessonId { get; set; }

        [Required]
        public int StudentId { get; set; }

        [Required]
        [StringLength(20)]
        public string Status { get; set; }

        public DateTime? CheckInTime { get; set; }

        public DateTime? CheckOutTime { get; set; }

        [StringLength(200)]
        public string Notes { get; set; }

        public DateTime CreatedAt { get; set; } = DateTime.UtcNow;

        public DateTime? UpdatedAt { get; set; }

        [ForeignKey("LessonId")]
        public virtual Lesson Lesson { get; set; }

        [ForeignKey("StudentId")]
        public virtual Student Student { get; set; }
    }
} 