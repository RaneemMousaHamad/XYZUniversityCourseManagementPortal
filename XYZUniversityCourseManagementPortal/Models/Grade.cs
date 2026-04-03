using System.ComponentModel.DataAnnotations;

namespace XYZUniversityCourseManagementPortal.Models
{
    public class Grade
    {
        [Key]
        public int Id { get; set; }
        public int Mark { get; set; }
        public Enrollment? Enrollment { get; set; }
        
    }
}
