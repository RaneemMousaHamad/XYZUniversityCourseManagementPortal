using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace XYZUniversityCourseManagementPortal.Models
{
    public class Enrollment
    {
     
        public int CourseId { get; set; }
        public int StudentId{ get; set; }
        public int? GradeId {  get; set; }
        [ForeignKey("GradeId")]
        public Grade? Grade { get; set; }
        public Student? Enrollment_student { get; set; }
        public Course? Enrollment_course { get; set; }
   
    }
}
