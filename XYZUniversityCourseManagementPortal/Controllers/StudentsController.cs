
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using XYZUniversityCourseManagementPortal.Data;
using XYZUniversityCourseManagementPortal.Models;

namespace XYZUniversityCourseManagementPortal.Controllers
{
    [Authorize(Roles = "Student")]
    public class StudentsController : Controller
    {
        private readonly UserManager<IdentityUser> _userManager;
        private readonly ApplicationDbContext _context;

        public StudentsController(UserManager<IdentityUser> userManager, ApplicationDbContext context)
        {
            _userManager = userManager;
            _context = context;
        }

        // Helper method to get or create student profile automatically
        private async Task<Student> GetOrCreateStudentAsync(IdentityUser user)
        {
            var student = await _context.Students
                .FirstOrDefaultAsync(s => s.IdentityUserId == user.Id);
            
            if (student == null)
            {
                student = new Student
                {
                    IdentityUserId = user.Id,
                    FirstName = user.Email?.Split('@')[0] ?? "Student",
                    LastName = "",
                    DOB = DateTime.Now.AddYears(-20) // Default age
                };
                _context.Students.Add(student);
                await _context.SaveChangesAsync();
            }
            
            return student;
        }

        public IActionResult Index()
        {
            return View();
        }

        public async Task<IActionResult> MyCourses()
        {
            ViewData["Title"] = "My Courses";
            ViewBag.Message = "View all courses you are enrolled in";

            var user = await _userManager.GetUserAsync(User);
            if (user == null)
                return NotFound("User not found.");

            var student = await GetOrCreateStudentAsync(user);

            var enrollments = await _context.Enrollments
                .Where(e => e.StudentId == student.Id)
                .Include(e => e.Enrollment_course)
                    .ThenInclude(c => c.Department)
                .Include(e => e.Grade)
                .ToListAsync();

            ViewBag.StudentName = student.FullName;
            return View(enrollments);
        }

        public async Task<IActionResult> GPA()
        {
            ViewData["Title"] = "My GPA";
            ViewBag.Message = "View your calculated Grade Point Average";

            var user = await _userManager.GetUserAsync(User);
            if (user == null)
                return NotFound("User not found.");

            var student = await GetOrCreateStudentAsync(user);

            var enrollments = await _context.Enrollments
                .Where(e => e.StudentId == student.Id)
                .Include(e => e.Enrollment_course)
                .Include(e => e.Grade)
                .Where(e => e.Grade != null)
                .ToListAsync();

            if (!enrollments.Any())
            {
                ViewBag.GPA = 0.0;
                ViewBag.TotalCredits = 0;
                ViewBag.CoursesWithGrades = 0;
            }
            else
            {
                // Calculate GPA as average of all grades (0-100 scale converted to 0-4 scale)
                double totalGpaPoints = 0;
                int courseCount = 0;

                foreach (var enrollment in enrollments)
                {
                    if (enrollment.Grade != null)
                    {
                        int mark = enrollment.Grade.Mark;
                        // Convert mark to GPA points (0-100 scale to 0-4 scale)
                        double gpaPoints = (mark / 100.0) * 4.0;
                        totalGpaPoints += gpaPoints;
                        courseCount++;
                    }
                }

                double gpa = courseCount > 0 ? totalGpaPoints / courseCount : 0.0;

                ViewBag.GPA = Math.Round(gpa, 2);
                ViewBag.TotalCredits = courseCount; // Using course count instead of credits
                ViewBag.CoursesWithGrades = courseCount;
            }
            ViewBag.StudentName = student.FullName;

            return View();
        }
    }
}
