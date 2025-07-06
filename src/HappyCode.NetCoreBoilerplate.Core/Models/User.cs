using System;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace HappyCode.NetCoreBoilerplate.Core.Models
{
    [Table("users", Schema = "dbo")]
    public class User
    {
        [Key]
        public int Id { get; set; }

        [Required]
        [StringLength(50)]
        public required string Username { get; set; }

        [Required]
        [EmailAddress]
        [StringLength(100)]
        public required string Email { get; set; }

        [Required]
        [StringLength(100)]
        public required string PasswordHash { get; set; }

        [Required]
        [StringLength(20)]
        public required string Role { get; set; }

        [StringLength(50)]
        public required string FirstName { get; set; }

        [StringLength(50)]
        public required string LastName { get; set; }

        [Phone]
        [StringLength(20)]
        public required string PhoneNumber { get; set; }

        public DateTime CreatedAt { get; set; } = DateTime.UtcNow;

        public DateTime? UpdatedAt { get; set; }

        public bool IsActive { get; set; } = true;

        public DateTime? LastLoginAt { get; set; }

        public virtual Student? Student { get; set; }
        public virtual Teacher? Teacher { get; set; }
        public virtual Parent? Parent { get; set; }
        public virtual Admin? Admin { get; set; }
    }
} 