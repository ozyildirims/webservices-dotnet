using System;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace HappyCode.NetCoreBoilerplate.Core.Models
{
    [Table("employees", Schema = "employees")]
    public class Employee
    {
        public Employee()
        {
            FirstName = string.Empty;
            LastName = string.Empty;
            Gender = string.Empty;
            DeptNo = string.Empty;
            Department = null!;
        }

        [Key]
        [Column("emp_no", TypeName = "int(11)")]
        public int Id { get; set; }

        [Column("birth_date", TypeName = "date")]
        public DateTime BirthDate { get; set; }

        [Required]
        [Column("first_name")]
        [StringLength(14)]
        public required string FirstName { get; set; }

        [Required]
        [Column("last_name")]
        [StringLength(16)]
        public required string LastName { get; set; }

        [Required]
        [Column("gender", TypeName = "enum('M','F')")]
        public required string Gender { get; set; }

        [Required]
        [Column("dept_no", TypeName = "char(4)")]
        public required string DeptNo { get; set; }

        [Column("hire_date", TypeName = "date")]
        public DateTime HireDate { get; set; }

        [Column("created_at", TypeName = "datetime")]
        public DateTime CreatedAt { get; set; }

        [ForeignKey("DeptNo")]
        [InverseProperty("Employees")]
        public virtual required Department Department { get; set; }

        [InverseProperty("Manger")]
        public virtual ICollection<Department>? ManagedDepartments { get; set; }
    }
}
