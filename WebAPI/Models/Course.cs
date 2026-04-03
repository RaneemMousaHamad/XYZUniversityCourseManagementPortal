using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace WebAPI.Models
{
    public class Course
    {
        [Key]
        public int Id { get; set; }
        public string? Name { get; set; }
        // 1-->M
        public int DepartmentId { get; set; }
        [ForeignKey("DepartmentId")]
        public Department? Department { get; set; }

        public ICollection<InstructorCourse> InstructorCourses { get; set; } = new List<InstructorCourse>();
        public ICollection<Enrollment> Enrollments { get; set; } = new List<Enrollment>();




    }
}
