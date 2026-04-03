using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using WebAPI.Data;


namespace WebAPI.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class CoursesController : ControllerBase
    {
        private readonly ApiDbContext _db;

        public CoursesController(ApiDbContext db)
        {
            _db = db;
        }

        // ✅ PUBLIC endpoint
        // GET: api/courses
        [HttpGet]
        public async Task<IActionResult> Get()
        {
            var courses = await _db.Courses
                .AsNoTracking()
                .Select(c => new        
                {
                   c.Id,
                   c.Name
                })
                .ToListAsync();

            return Ok(courses);
        }
    }
}
