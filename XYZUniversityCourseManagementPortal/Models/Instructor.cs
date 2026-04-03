using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using static Microsoft.EntityFrameworkCore.DbLoggerCategory.Database;

namespace XYZUniversityCourseManagementPortal.Models
{
    public class Instructor
    {
        [Key]
        public int Id { get; set; }
        public string? IdentityUserId { get; set; }
        public string? FirstName { get; set; }
        public string? LastName { get; set; }
        public int DepartmentId { get; set; }

        [ForeignKey("DepartmentId")]
        public Department? Department { get; set; }
        public ICollection<InstructorCourse> InstructorCourses { get; set; } = new List<InstructorCourse>();

        [NotMapped]
        public string FullName => $"{FirstName} {LastName}";

    }
}
