namespace WebAPI.Models
{
    public class InstructorCourse
    {


        public int CourseId { get; set; }
        public int InstructorId { get; set; }

        public Instructor? InstructorCourse_instructor { get; set; }
        public Course? InstructorCourse_course { get; set; }
    }
}
