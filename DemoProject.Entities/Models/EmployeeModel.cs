
using System.ComponentModel.DataAnnotations;


namespace DemoProject.Entities.Models
{
    public class EmployeeModel
    {
        [Key]
        public long emp_id { get; set; }

        public string? firstname { get; set; }

        public string? lastname { get; set; }

        public string email { get; set; } = null!;

        public string password { get; set; } = null!;

        public DateTime? created_at { get; set; }

        public DateTime? updated_at { get; set; }

        public DateTime? deleted_at { get; set; }
    }
}
