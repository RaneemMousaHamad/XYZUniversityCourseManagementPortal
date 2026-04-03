using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.EntityFrameworkCore;
using XYZUniversityCourseManagementPortal.Data;
using XYZUniversityCourseManagementPortal.Models;
using Amazon.S3;
using Amazon.S3.Model;

public class AwsTestService
{
    private readonly IAmazonS3 _s3Client;

    public AwsTestService(IAmazonS3 s3Client)
    {
        _s3Client = s3Client;
    }

    public async Task<bool> TestConnectionAsync()
    {
        var response = await _s3Client.ListBucketsAsync();
        return response.Buckets.Any();
    }
}



namespace XYZUniversityCourseManagementPortal.Controllers
{
    [Authorize(Roles = "Admin")]
    public class CoursesController : Controller
    {

        private readonly ApplicationDbContext _context;

        public CoursesController(ApplicationDbContext context)
        {
            _context = context;
        }
        // GET: HomeController1
        public async Task<IActionResult> Index()
        {
            try
            {
                var courses = await _context.Courses
                    .Include(c => c.Department)
                    .Include(c => c.InstructorCourses)
                        .ThenInclude(ic => ic.InstructorCourse_instructor)
                    .ToListAsync();

                return View(courses);
            }
            catch (Exception)
            {
                // If database schema is not updated, load courses without instructor details
                var courses = await _context.Courses
                    .Include(c => c.Department)
                    .ToListAsync();

                return View(courses);
            }
        }


        // GET: HomeController1/Details/5
        public async Task<IActionResult> Details(int id)
        {  
            var course = await _context.Courses
                .Include(c => c.Department)
                .Include(c => c.InstructorCourses)
                    .ThenInclude(ic => ic.InstructorCourse_instructor)
                .FirstOrDefaultAsync(c => c.Id == id);
            
            if (course == null)
                return NotFound();

            return View(course);
        }

        // GET: HomeController1/Create
        public async Task<IActionResult> Create()
        {
            try
            {
                var departments = await _context.Departments.ToListAsync();
                var instructors = await _context.Instructors
                    .Include(i => i.Department)
                    .ToListAsync();
                
                if (!departments.Any())
                {
                    ViewBag.ErrorMessage = "Please create at least one Department before creating a Course.";
                }
                
                ViewData["DepartmentId"] = new SelectList(departments, "Id", "Name");
                ViewBag.Instructors = instructors;
            }
            catch (Exception)
            {
                // Database schema not updated - load only departments
                var departments = await _context.Departments.ToListAsync();
                ViewData["DepartmentId"] = new SelectList(departments, "Id", "Name");
                ViewBag.Instructors = new List<Instructor>();
                ViewBag.DatabaseError = "Database schema needs to be updated. Please run the SQL script FixDatabase.sql first.";
            }
            
            return View();
        }

        // POST: HomeController1/Create
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create(Course course, int[] selectedInstructorIds)
        {
            if (ModelState.IsValid)
            {
                _context.Courses.Add(course);
                await _context.SaveChangesAsync();

                // Assign multiple instructors to course if provided
                if (selectedInstructorIds != null && selectedInstructorIds.Length > 0)
                {
                    foreach (var instructorId in selectedInstructorIds)
                    {
                        var instructor = await _context.Instructors.FindAsync(instructorId);
                        if (instructor != null)
                        {
                            var instructorCourse = new InstructorCourse
                            {
                                CourseId = course.Id,
                                InstructorId = instructorId
                            };
                            _context.InstructorCourses.Add(instructorCourse);
                        }
                    }
                    await _context.SaveChangesAsync();
                }

                TempData["SuccessMessage"] = "Course created successfully.";
                return RedirectToAction(nameof(Index));
            }

            ViewData["DepartmentId"] = new SelectList(_context.Departments, "Id", "Name", course.DepartmentId);
            var instructors = await _context.Instructors.ToListAsync();
            ViewData["Instructors"] = instructors;
            return View(course);
        }

        // GET: HomeController1/Edit/5
        public async Task<IActionResult> Edit(int id)
        {
            var course = await _context.Courses.FindAsync(id);
            if (course == null)
                return NotFound();

            ViewData["DepartmentId"] = new SelectList(await _context.Departments.ToListAsync(), "Id", "Name", course.DepartmentId);
            return View(course);
        }
        

        // POST: HomeController1/Edit/5
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Edit(int id, Course course)    
        {
            if (id != course.Id)
                return NotFound();

            if (ModelState.IsValid)
            {
                try
                {
                    _context.Update(course);
                    await _context.SaveChangesAsync();
                    TempData["SuccessMessage"] = "Course updated successfully.";
                    return RedirectToAction(nameof(Index));
                }
                catch (DbUpdateConcurrencyException)
                {
                    if (!await _context.Courses.AnyAsync(e => e.Id == id))
                    {
                        return NotFound();
                    }
                    else
                    {
                        throw;
                    }
                }
            }

            ViewData["DepartmentId"] = new SelectList(await _context.Departments.ToListAsync(), "Id", "Name", course.DepartmentId);
            return View(course);
        }

        // GET: CoursesController/Delete/5
        public async Task<IActionResult> Delete(int id)
        {
            var course = await _context.Courses
                .Include(c => c.Department)
                .FirstOrDefaultAsync(c => c.Id == id);
            
            if (course == null)
                return NotFound();
            
            return View(course);
        }

        // POST: CoursesController/Delete/5
        [HttpPost, ActionName("Delete")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> DeleteConfirmed(int id)
        {
            // Load course with related InstructorCourses and Enrollments
            var course = await _context.Courses
                .Include(c => c.InstructorCourses)
                .Include(c => c.Enrollments)
                .FirstOrDefaultAsync(c => c.Id == id);

            if (course != null)
            {
                // Remove related instructor-course relationships
                if (course.InstructorCourses != null && course.InstructorCourses.Any())
                {
                    _context.InstructorCourses.RemoveRange(course.InstructorCourses);
                }

                // Remove related enrollments (grades will be handled by FK with SetNull)
                if (course.Enrollments != null && course.Enrollments.Any())
                {
                    _context.Enrollments.RemoveRange(course.Enrollments);
                }

                // Now remove the course itself
                _context.Courses.Remove(course);
                await _context.SaveChangesAsync();
            }

            return RedirectToAction(nameof(Index));
        }

        // GET: Courses/AssignInstructors/5
        public async Task<IActionResult> AssignInstructors(int id)
        {
            var course = await _context.Courses
                .Include(c => c.InstructorCourses)
                    .ThenInclude(ic => ic.InstructorCourse_instructor)
                .FirstOrDefaultAsync(c => c.Id == id);

            if (course == null)
                return NotFound();

            // Get all instructors
            var allInstructors = await _context.Instructors
                .Include(i => i.Department)
                .ToListAsync();

            // Get currently assigned instructor IDs
            var assignedInstructorIds = course.InstructorCourses
                .Select(ic => ic.InstructorId)
                .ToList();

            ViewBag.Course = course;
            ViewBag.AllInstructors = allInstructors;
            ViewBag.AssignedInstructorIds = assignedInstructorIds;

            return View();
        }

        // POST: Courses/AssignInstructors/5
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> AssignInstructors(int id, int[] selectedInstructorIds)
        {
            var course = await _context.Courses
                .Include(c => c.InstructorCourses)
                .FirstOrDefaultAsync(c => c.Id == id);

            if (course == null)
                return NotFound();

            // Remove all existing instructor-course relationships
            var existingRelations = course.InstructorCourses.ToList();
            foreach (var relation in existingRelations)
            {
                _context.InstructorCourses.Remove(relation);
            }

            // Add new relationships for selected instructors
            if (selectedInstructorIds != null && selectedInstructorIds.Length > 0)
            {
                foreach (var instructorId in selectedInstructorIds)
                {
                    // Verify instructor exists
                    var instructor = await _context.Instructors.FindAsync(instructorId);
                    if (instructor != null)
                    {
                        var instructorCourse = new InstructorCourse
                        {
                            CourseId = course.Id,
                            InstructorId = instructorId
                        };
                        _context.InstructorCourses.Add(instructorCourse);
                    }
                }
            }

            await _context.SaveChangesAsync();
            TempData["SuccessMessage"] = "Instructors assigned successfully.";
            return RedirectToAction(nameof(Details), new { id });
        }
    }
}
