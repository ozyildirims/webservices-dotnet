using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace HappyCode.NetCoreBoilerplate.Core.Models
{
    [Table("announcements", Schema = "dbo")]
    public class Announcement
    {
        [Key]
        public int Id { get; set; }

        [Required]
        [StringLength(200)]
        public string Title { get; set; }

        [Required]
        [StringLength(1000)]
        public string Content { get; set; }

        [Required]
        public int CreatedById { get; set; }

        [StringLength(50)]
        public string TargetRole { get; set; }

        [StringLength(50)]
        public string Priority { get; set; } = "Normal";

        public DateTime? PublishDate { get; set; }

        public DateTime? ExpiryDate { get; set; }

        [StringLength(50)]
        public string Status { get; set; } = "Draft";

        public DateTime CreatedAt { get; set; } = DateTime.UtcNow;

        public DateTime? UpdatedAt { get; set; }

        [ForeignKey("CreatedById")]
        public virtual User CreatedBy { get; set; }

        public virtual ICollection<Notification> Notifications { get; set; } = new HashSet<Notification>();
    }
} 