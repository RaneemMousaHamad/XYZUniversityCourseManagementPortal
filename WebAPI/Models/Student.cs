using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace WebAPI.Models
{
    public class Student
    {
        [Key]
        public int Id { get; set; }
        public string? FirstName { get; set; }
        public string? LastName { get; set; }
        public DateTime DOB { get; set; }
        public string? IdentityUserId { get; set; }

        public ICollection<Enrollment> Enrollments { get; set; } = new List<Enrollment>();

        [NotMapped]
        public string FullName => $"{FirstName} {LastName}";

    }
}
