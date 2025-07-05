using System.ComponentModel.DataAnnotations;

namespace HappyCode.NetCoreBoilerplate.Core.Dtos
{
    public class AnnouncementDto
    {
        public int Id { get; set; }
        public string Title { get; set; }
        public string Content { get; set; }
        public int CreatedById { get; set; }
        public string CreatedByName { get; set; }
        public string TargetRole { get; set; }
        public string Priority { get; set; }
        public DateTime? PublishDate { get; set; }
        public DateTime? ExpiryDate { get; set; }
        public string Status { get; set; }
        public DateTime CreatedAt { get; set; }
    }

    public class AnnouncementCreateDto
    {
        [Required]
        [StringLength(200)]
        public string Title { get; set; }

        [Required]
        [StringLength(1000)]
        public string Content { get; set; }

        [StringLength(50)]
        public string TargetRole { get; set; }

        [StringLength(50)]
        public string Priority { get; set; } = "Normal";

        public DateTime? PublishDate { get; set; }

        public DateTime? ExpiryDate { get; set; }

        [StringLength(50)]
        public string Status { get; set; } = "Draft";
    }

    public class AnnouncementUpdateDto
    {
        [StringLength(200)]
        public string Title { get; set; }

        [StringLength(1000)]
        public string Content { get; set; }

        [StringLength(50)]
        public string TargetRole { get; set; }

        [StringLength(50)]
        public string Priority { get; set; }

        public DateTime? PublishDate { get; set; }

        public DateTime? ExpiryDate { get; set; }

        [StringLength(50)]
        public string Status { get; set; }
    }

    public class NotificationDto
    {
        public int Id { get; set; }
        public int UserId { get; set; }
        public int? AnnouncementId { get; set; }
        public string Title { get; set; }
        public string Message { get; set; }
        public string Type { get; set; }
        public string Status { get; set; }
        public DateTime? ReadAt { get; set; }
        public DateTime CreatedAt { get; set; }
    }

    public class NotificationCreateDto
    {
        [Required]
        public int UserId { get; set; }

        public int? AnnouncementId { get; set; }

        [Required]
        [StringLength(200)]
        public string Title { get; set; }

        [Required]
        [StringLength(500)]
        public string Message { get; set; }

        [StringLength(50)]
        public string Type { get; set; } = "Info";
    }

    public class NotificationUpdateDto
    {
        [StringLength(50)]
        public string Status { get; set; }

        public DateTime? ReadAt { get; set; }
    }
} 