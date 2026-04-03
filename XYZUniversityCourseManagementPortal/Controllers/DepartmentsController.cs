using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using XYZUniversityCourseManagementPortal.Data;
using XYZUniversityCourseManagementPortal.Models;

namespace XYZUniversityCourseManagementPortal.Controllers
{
    [Authorize(Roles = "Admin")]
    public class DepartmentsController : Controller
    {
        private readonly ApplicationDbContext _context;

        public DepartmentsController(ApplicationDbContext context)
        {
            _context = context;
        }
        // GET:DepartmentsController
        // Read All Departmwnts
        public async Task<IActionResult> Index()
        {
            var departments = await _context.Departments.ToListAsync();
            return View(departments);
        }

        // GET: DepartmentsController/Details/5
        public async Task<IActionResult> Details(int id)
        {
            var department = await _context.Departments
                .FirstOrDefaultAsync(d => d.Id == id);

            if (department == null)
                return NotFound();

            return View(department);
        }

        // GET: DepartmentsController/Create
        public ActionResult Create()
        {
            return View();
        }

        // POST: DepartmentsController/Create
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create(Department department)
        {
            if (ModelState.IsValid)
            {
                _context.Departments.Add(department);
                await _context.SaveChangesAsync();
                return RedirectToAction(nameof(Index));
            }

            return View(department);
        }

        // GET: DepartmentsController/Edit/5
        public async Task<IActionResult> Edit(int id)
        {
            var department = await _context.Departments.FindAsync(id);

            if (department == null)
                return NotFound();

            return View(department);
        }

        // POST: DepartmentsController/Edit/5
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Edit(int id, Department department)
        {
            if (id != department.Id)
                return NotFound();

            if (ModelState.IsValid)
            {
                _context.Departments.Update(department);
                await _context.SaveChangesAsync();
                return RedirectToAction(nameof(Index));
            }

            return View(department);
        }

        // GET: DepartmentsController/Delete/5
        public async Task<IActionResult> Delete(int id)
        {
            var department = await _context.Departments.FindAsync(id);
            if (department == null)
                return NotFound();
            return View(department);
        }


        // POST: DepartmentsController/Delete/5
        [HttpPost, ActionName("Delete")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> DeleteConfirmed(int id)
        {
            // Load department with related Courses and Instructors
            var department = await _context.Departments
                .Include(d => d.Courses)
                .Include(d => d.Instructors)
                .FirstOrDefaultAsync(d => d.Id == id);

            if (department == null)
                return NotFound();

            // Prevent delete if there are related courses or instructors,
            // because they are linked via other tables (InstructorCourses, Enrollments)
            var hasCourses = department.Courses != null && department.Courses.Any();
            var hasInstructors = department.Instructors != null && department.Instructors.Any();

            if (hasCourses || hasInstructors)
            {
                TempData["ErrorMessage"] =
                    "Cannot delete this department because it still has related courses or instructors. " +
                    "Please remove or reassign them before deleting the department.";

                return RedirectToAction(nameof(Index));
            }

            _context.Departments.Remove(department);
            await _context.SaveChangesAsync();

            TempData["SuccessMessage"] = "Department deleted successfully.";
            return RedirectToAction(nameof(Index));
        }
    }
}
