using Microsoft.EntityFrameworkCore;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace DB.Data.AccountDB
{
    [Table("Employee")]
    public class Employee : IModelCreateEntity
    {
        [Key]
        [Required]
        [DatabaseGenerated(DatabaseGeneratedOption.Identity)]
        public int employeeId { get; set; }

        [Required]
        [StringLength(64)]
        public string name { get; set; }

        [Required]
        [StringLength(128)]
        public string email { get; set; }

        [Required]
        [StringLength(16)]
        public string tel { get; set; }

        public DateTime joined { get; set; }
        public DateTime createdAt { get; set; }
        public DateTime? updatedAt { get; set; }

        public void CreateModel(ModelBuilder modelBuilder)
        {
            modelBuilder.Entity<Employee>()
                .HasIndex(e => e.name)
                .HasDatabaseName("IX_Employees_Name");
        }
    }
}
