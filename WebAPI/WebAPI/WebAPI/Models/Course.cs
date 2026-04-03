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
        




    }
}
