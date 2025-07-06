using System;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace HappyCode.NetCoreBoilerplate.Core.Models
{
    public partial class Owner
    {
        public int Id { get; set; }

        [Required]
        [StringLength(50)]
        public required string FirstName { get; set; }

        [Required]
        [StringLength(50)]
        public required string LastName { get; set; }

        [Required]
        [StringLength(100)]
        public required string FullName { get; set; }

        public DateTime CreatedAt { get; set; }

        [InverseProperty("Owner")]
        public virtual ICollection<Car> Cars { get; set; } = new HashSet<Car>();
    }
}
