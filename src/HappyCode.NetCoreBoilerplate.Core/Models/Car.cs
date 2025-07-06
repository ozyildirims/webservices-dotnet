using System;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace HappyCode.NetCoreBoilerplate.Core.Models
{
    public partial class Car
    {
        public int Id { get; set; }

        [Required]
        [StringLength(20)]
        public required string Plate { get; set; }

        [Required]
        [StringLength(50)]
        public required string Model { get; set; }

        public int Year { get; set; }
        public DateTime CreatedAt { get; set; }

        public int? OwnerId { get; set; }

        [ForeignKey("OwnerId")]
        [InverseProperty("Cars")]
        public virtual Owner Owner { get; set; }
    }
}
