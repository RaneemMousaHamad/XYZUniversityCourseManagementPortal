using System.ComponentModel.DataAnnotations;

namespace WebAPI.Models
{
    public class Department
    {
        [Key]
        public int Id { get; set; }
        public string? Name { get; set; }
        //string Description { get; set; }
        public ICollection<Course>? Courses { get; set; }
        public ICollection<Instructor>? Instructors { get; set; }


    }
}
