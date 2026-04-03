using System.Diagnostics;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using XYZUniversityCourseManagementPortal.Data;
using XYZUniversityCourseManagementPortal.Models;

namespace XYZUniversityCourseManagementPortal.Controllers
{
    public class HomeController : Controller
    {
        private readonly ApplicationDbContext _context;

        public HomeController(ApplicationDbContext context)
        {
            _context = context;
        }

        public async Task<IActionResult> Index()
        {
            ViewData["Title"] = "XYZ University - Course Catalog";
            ViewBag.WelcomeMessage = "Welcome to XYZ University Course Management Portal";
            ViewBag.CourseCount = await _context.Courses.CountAsync();

            // Public course list - accessible to everyone
            var courses = await _context.Courses
                .Include(c => c.Department)
                .ToListAsync();

            return View(courses);
        }

        public IActionResult Privacy()
        {
            return View();
        }

        public IActionResult ApiCatalog()
        {
            return View();
        }

        [ResponseCache(Duration = 0, Location = ResponseCacheLocation.None, NoStore = true)]
        public IActionResult Error()
        {
            return View(new ErrorViewModel { RequestId = Activity.Current?.Id ?? HttpContext.TraceIdentifier });
        }
    }
}
