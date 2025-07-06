using System;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace HappyCode.NetCoreBoilerplate.Core.Models
{
    [Table("departments", Schema = "employees")]
    public class Department
    {
        [Key]
        [Column("dept_no", TypeName = "char(4)")]
        [Required]
        [StringLength(20)]
        public string DeptNo { get; set; }

        [Required]
        [Column("dept_name")]
        [StringLength(50)]
        public string DeptName { get; set; }

        [Column("manger_no", TypeName = "int(11)")]
        public int MangerNo { get; set; }

        public DateTime CreatedAt { get; set; }

        [ForeignKey("MangerNo")]
        [InverseProperty("LeadingDepartments")]
        public virtual Employee Manger { get; set; }

        [InverseProperty("ManagedDepartments")]
        public virtual ICollection<Employee> Employees { get; set; } = new HashSet<Employee>();
    }
}
