using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using XYZUniversityCourseManagementPortal.Data;
using XYZUniversityCourseManagementPortal.Models;

namespace XYZUniversityCourseManagementPortal.Controllers.Api
{
    [ApiController]
    [Route("api/[controller]")]
    public class CoursesApiController : ControllerBase
    {
        private readonly ApplicationDbContext _context;

        public CoursesApiController(ApplicationDbContext context)
        {
            _context = context;
        }

        // GET: api/Courses
        [HttpGet]
        public async Task<ActionResult<IEnumerable<object>>> GetCourses()
        {
            var courses = await _context.Courses
                .Include(c => c.Department)
                .Select(c => new
                {
                    id = c.Id,
                    name = c.Name,
                    departmentName = c.Department != null ? c.Department.Name : null
                })
                .ToListAsync();

            return Ok(courses);
        }
    }
}

