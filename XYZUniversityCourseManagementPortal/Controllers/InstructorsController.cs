using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.EntityFrameworkCore;
using XYZUniversityCourseManagementPortal.Data;
using XYZUniversityCourseManagementPortal.Models;

namespace XYZUniversityCourseManagementPortal.Controllers
{
    [Authorize(Roles = "Instructor")]
    

    public class InstructorsController : Controller
    {
        private readonly ApplicationDbContext _context;
        private readonly UserManager<IdentityUser> _userManager;

        public InstructorsController(ApplicationDbContext context, UserManager<IdentityUser> userManager)
        {
            _context = context;
            _userManager = userManager;
        }

        // Helper method to get or create instructor profile automatically
        private async Task<Instructor> GetOrCreateInstructorAsync(IdentityUser user)
        {
            var instructor = await _context.Instructors
                .FirstOrDefaultAsync(i => i.IdentityUserId == user.Id);
            
            if (instructor == null)
            {
                // Get first department or create a default one
                var firstDepartment = await _context.Departments.FirstOrDefaultAsync();
                if (firstDepartment == null)
                {
                    firstDepartment = new Department { Name = "General" };
                    _context.Departments.Add(firstDepartment);
                    await _context.SaveChangesAsync();
                }

                instructor = new Instructor
                {
                    IdentityUserId = user.Id,
                    FirstName = user.Email?.Split('@')[0] ?? "Instructor",
                    LastName = "",
                    DepartmentId = firstDepartment.Id
                };
                _context.Instructors.Add(instructor);
                await _context.SaveChangesAsync();
            }
            
            return instructor;
        }
        // 1. GET: Instructors/MyCourses
        public async Task<IActionResult> MyCourses()
        {
            var user = await _userManager.GetUserAsync(User);
            if (user == null)
            {
                TempData["ErrorMessage"] = "User not found. Please log in again.";
                return RedirectToAction("Index", "Home");
            }

            var instructor = await GetOrCreateInstructorAsync(user);

            ViewData["Title"] = "My Courses";
            ViewBag.InstructorName = instructor.FullName;

            try
            {
                var courses = await _context.InstructorCourses
                    .Where(ic => ic.InstructorId == instructor.Id)
                    .Include(ic => ic.InstructorCourse_course)
                        .ThenInclude(c => c.Department)
                    .ToListAsync();
                
                if (!courses.Any())
                {
                    TempData["InfoMessage"] = "No courses assigned to you yet. Please contact the administrator to assign courses.";
                }
                
                return View(courses);
            }
            catch (Exception ex)
            {
                TempData["ErrorMessage"] = $"An error occurred while loading courses: {ex.Message}";
                return View(new List<InstructorCourse>());
            }
        }

        // 2. GET: Instructors/CourseStudents/5
        public async Task<IActionResult> CourseStudents(int courseId)
        {
            if (courseId <= 0)
            {
                TempData["ErrorMessage"] = "Invalid course ID.";
                return RedirectToAction("MyCourses");
            }

            var user = await _userManager.GetUserAsync(User);
            if (user == null)
            {
                TempData["ErrorMessage"] = "User not found. Please log in again.";
                return RedirectToAction("MyCourses");
            }

            var instructor = await GetOrCreateInstructorAsync(user);

            // Verify instructor is assigned to this course
            var instructorCourse = await _context.InstructorCourses
                .FirstOrDefaultAsync(ic => ic.InstructorId == instructor.Id && ic.CourseId == courseId);
            
            if (instructorCourse == null)
            {
                TempData["ErrorMessage"] = "You are not assigned to this course. Please contact the administrator.";
                return RedirectToAction("MyCourses");
            }

            var course = await _context.Courses.FindAsync(courseId);
            if (course == null)
            {
                TempData["ErrorMessage"] = "Course not found.";
                return RedirectToAction("MyCourses");
            }

            ViewData["Title"] = "Course Students";
            ViewBag.CourseId = courseId;
            ViewBag.CourseName = course.Name;

            var enrollments = await _context.Enrollments
                .Where(e => e.CourseId == courseId)
                .Include(e => e.Enrollment_student)
                .Include(e => e.Grade)
                .ToListAsync();

            return View(enrollments);
        }

        // GET: Instructors/AddStudent/5
        public async Task<IActionResult> AddStudent(int courseId)
        {
            var user = await _userManager.GetUserAsync(User);
            if (user == null) return NotFound("User not found.");

            var instructor = await GetOrCreateInstructorAsync(user);

            // Verify instructor is assigned to this course
            var instructorCourse = await _context.InstructorCourses
                .FirstOrDefaultAsync(ic => ic.InstructorId == instructor.Id && ic.CourseId == courseId);
            if (instructorCourse == null) return Forbid("You are not assigned to this course.");

            var course = await _context.Courses.FindAsync(courseId);
            ViewData["Title"] = "Add Student to Course";
            ViewBag.CourseId = courseId;
            ViewBag.CourseName = course?.Name ?? "Unknown Course";

            // Get students not already enrolled
            var enrolledStudentIds = await _context.Enrollments
                .Where(e => e.CourseId == courseId)
                .Select(e => e.StudentId)
                .ToListAsync();

            var availableStudents = await _context.Students
                .Where(s => !enrolledStudentIds.Contains(s.Id))
                .ToListAsync();

            ViewBag.StudentId = new SelectList(availableStudents, "Id", "FullName");
            return View();
        }

        // POST: Instructors/AddStudent
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> AddStudent(int courseId, int studentId)
        {
            // Verify instructor is assigned to this course
            var user = await _userManager.GetUserAsync(User);
            if (user == null) return NotFound("User not found.");

            var instructor = await GetOrCreateInstructorAsync(user);
            var instructorCourse = await _context.InstructorCourses
                .FirstOrDefaultAsync(ic => ic.InstructorId == instructor.Id && ic.CourseId == courseId);
            if (instructorCourse == null) return Forbid("You are not assigned to this course.");

            // Validate student exists
            var student = await _context.Students.FindAsync(studentId);
            if (student == null)
            {
                TempData["ErrorMessage"] = "Student not found.";
                return RedirectToAction("AddStudent", new { courseId });
            }

            // Check if enrollment already exists
            var existingEnrollment = await _context.Enrollments
                .FirstOrDefaultAsync(e => e.CourseId == courseId && e.StudentId == studentId);
            
            if (existingEnrollment != null)
            {
                TempData["ErrorMessage"] = $"Student {student.FullName} is already enrolled in this course.";
                return RedirectToAction("CourseStudents", new { courseId });
            }

            var enrollment = new Enrollment
            {
                CourseId = courseId,
                StudentId = studentId,
                GradeId = null // No grade initially
            };

            _context.Enrollments.Add(enrollment);
            await _context.SaveChangesAsync();

            TempData["SuccessMessage"] = $"Student {student.FullName} has been successfully enrolled in the course.";
            return RedirectToAction("CourseStudents", new { courseId });
        }

        // GET: Instructors/AddGrade/5?enrollmentCourseId=X&enrollmentStudentId=Y
        public async Task<IActionResult> AddGrade(int courseId, int studentId)
        {
            var user = await _userManager.GetUserAsync(User);
            if (user == null) return NotFound("User not found.");

            var instructor = await GetOrCreateInstructorAsync(user);

            // Verify instructor is assigned to this course
            var instructorCourse = await _context.InstructorCourses
                .FirstOrDefaultAsync(ic => ic.InstructorId == instructor.Id && ic.CourseId == courseId);
            if (instructorCourse == null) return Forbid("You are not assigned to this course.");

            var enrollment = await _context.Enrollments
                .Include(e => e.Enrollment_student)
                .Include(e => e.Enrollment_course)
                .FirstOrDefaultAsync(e => e.CourseId == courseId && e.StudentId == studentId);

            if (enrollment == null) return NotFound("Enrollment not found.");

            ViewData["Title"] = "Add/Edit Grade";
            ViewBag.CourseId = courseId;
            ViewBag.StudentId = studentId;
            ViewBag.StudentName = enrollment.Enrollment_student?.FullName ?? "Unknown";
            ViewBag.CourseName = enrollment.Enrollment_course?.Name ?? "Unknown";

            var grade = enrollment.Grade;
            if (grade == null)
            {
                grade = new Grade { Mark = 0 };
            }

            return View(grade);
        }

        // POST: Instructors/AddGrade
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> AddGrade(int courseId, int studentId, Grade grade)
        {
            if (ModelState.IsValid)
            {
                var enrollment = await _context.Enrollments
                    .Include(e => e.Grade)
                    .FirstOrDefaultAsync(e => e.CourseId == courseId && e.StudentId == studentId);

                if (enrollment == null) return NotFound("Enrollment not found.");

                if (enrollment.Grade == null)
                {
                    // Create new grade
                    _context.Grades.Add(grade);
                    await _context.SaveChangesAsync();
                    enrollment.GradeId = grade.Id;
                    _context.Enrollments.Update(enrollment);
                }
                else
                {
                    // Update existing grade
                    enrollment.Grade.Mark = grade.Mark;
                    _context.Grades.Update(enrollment.Grade);
                }

                await _context.SaveChangesAsync();
                TempData["SuccessMessage"] = $"Grade updated successfully for {enrollment.Enrollment_student?.FullName}.";
                return RedirectToAction("CourseStudents", new { courseId });
            }

            ViewBag.CourseId = courseId;
            ViewBag.StudentId = studentId;
            var enrollmentForView = await _context.Enrollments
                .Include(e => e.Enrollment_student)
                .Include(e => e.Enrollment_course)
                .FirstOrDefaultAsync(e => e.CourseId == courseId && e.StudentId == studentId);
            
            ViewBag.StudentName = enrollmentForView?.Enrollment_student?.FullName ?? "Unknown";
            ViewBag.CourseName = enrollmentForView?.Enrollment_course?.Name ?? "Unknown";
            return View(grade);
        }

        public IActionResult Index()
        {
            return View();
        }
    }
}
