using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.EntityFrameworkCore;
using XYZUniversityCourseManagementPortal.Data;
using XYZUniversityCourseManagementPortal.Models;

namespace XYZUniversityCourseManagementPortal.Controllers
{
    [Authorize(Roles = "Admin")]
    public class AdminController : Controller
    {
        private readonly ApplicationDbContext _context;
        private readonly UserManager<IdentityUser> _userManager;
       // private readonly UserManager<IdentityUser> _roleManager;
       // initialization in program.cs



        public AdminController( ApplicationDbContext context,UserManager<IdentityUser> userManager)
        {
            _context = context;
            _userManager = userManager;
            
        }

        public IActionResult Index()
        {
            return View();
        }

        public IActionResult Users()
        {
            var users = _userManager.Users.ToList();
            return View(users);
        }


        //Admin can decide who is Instructor or Student (only these two roles)
        [HttpPost]
        public async Task<IActionResult> AssignRole(string userId, string role)
        {
            var user = await _userManager.FindByIdAsync(userId);
            if (user == null || string.IsNullOrEmpty(role))
            {
                TempData["ErrorMessage"] = "Invalid user or role.";
                return RedirectToAction("Users");
            }

            // Only allow Student or Instructor roles to be assigned
            if (role != "Student" && role != "Instructor")
            {
                TempData["ErrorMessage"] = "You can only assign Student or Instructor roles.";
                return RedirectToAction("Users");
            }

            // Check if user is currently an Admin - prevent changing admin role
            var currentRoles = await _userManager.GetRolesAsync(user);
            if (currentRoles.Contains("Admin"))
            {
                TempData["ErrorMessage"] = "Cannot change Admin role. There can only be one Admin.";
                return RedirectToAction("Users");
            }

            // Remove existing roles (except Admin which shouldn't be here, but just in case)
            var rolesToRemove = currentRoles.Where(r => r != "Admin").ToList();
            if (rolesToRemove.Any())
            {
                await _userManager.RemoveFromRolesAsync(user, rolesToRemove);
            }

            // Assign the new role
            await _userManager.AddToRoleAsync(user, role);

            // Automatically create Instructor or Student profile when role is assigned
            if (role == "Instructor")
            {
                // Check if instructor profile already exists
                var existingInstructor = await _context.Instructors
                    .FirstOrDefaultAsync(i => i.IdentityUserId == userId);
                
                if (existingInstructor == null)
                {
                    // Get first department or create a default one
                    var firstDepartment = await _context.Departments.FirstOrDefaultAsync();
                    if (firstDepartment == null)
                    {
                        // Create a default department if none exists
                        firstDepartment = new Department { Name = "General" };
                        _context.Departments.Add(firstDepartment);
                        await _context.SaveChangesAsync();
                    }

                    // Create instructor profile automatically
                    var instructor = new Instructor
                    {
                        IdentityUserId = userId,
                        FirstName = user.Email?.Split('@')[0] ?? "Instructor",
                        LastName = "",
                        DepartmentId = firstDepartment.Id
                    };
                    _context.Instructors.Add(instructor);
                    await _context.SaveChangesAsync();
                }
            }
            else if (role == "Student")
            {
                // Check if student profile already exists
                var existingStudent = await _context.Students
                    .FirstOrDefaultAsync(s => s.IdentityUserId == userId);
                
                if (existingStudent == null)
                {
                    // Create student profile automatically
                    var student = new Student
                    {
                        IdentityUserId = userId,
                        FirstName = user.Email?.Split('@')[0] ?? "Student",
                        LastName = "",
                        DOB = DateTime.Now.AddYears(-20) // Default age
                    };
                    _context.Students.Add(student);
                    await _context.SaveChangesAsync();
                }
            }

            TempData["SuccessMessage"] = $"Role '{role}' assigned successfully.";
            return RedirectToAction("Users");
        }

        // Delete user
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> DeleteUser(string userId)
        {
            var user = await _userManager.FindByIdAsync(userId);
            if (user == null)
            {
                TempData["ErrorMessage"] = "User not found.";
                return RedirectToAction("Users");
            }

            // Prevent deleting users with Admin role
            var userRoles = await _userManager.GetRolesAsync(user);
            if (userRoles.Contains("Admin"))
            {
                TempData["ErrorMessage"] = "Cannot delete Admin user. There can only be one Admin.";
                return RedirectToAction("Users");
            }

            // Prevent deleting the current admin user (extra safety check)
            var currentUser = await _userManager.GetUserAsync(User);
            if (user.Id == currentUser?.Id)
            {
                TempData["ErrorMessage"] = "You cannot delete your own account.";
                return RedirectToAction("Users");
            }

            // Delete associated instructor or student profile and related data
            var instructor = await _context.Instructors
                .FirstOrDefaultAsync(i => i.IdentityUserId == userId);
            if (instructor != null)
            {
                // Remove instructor-course relationships first to satisfy FK constraints
                var instructorCourses = await _context.InstructorCourses
                    .Where(ic => ic.InstructorId == instructor.Id)
                    .ToListAsync();

                if (instructorCourses.Any())
                {
                    _context.InstructorCourses.RemoveRange(instructorCourses);
                }

                _context.Instructors.Remove(instructor);
            }

            var student = await _context.Students
                .Include(s => s.Enrollments)
                .FirstOrDefaultAsync(s => s.IdentityUserId == userId);
            if (student != null)
            {
                // Remove enrollments before deleting student (Enrollment -> Student uses Restrict)
                if (student.Enrollments != null && student.Enrollments.Any())
                {
                    _context.Enrollments.RemoveRange(student.Enrollments);
                }

                _context.Students.Remove(student);
            }

            await _context.SaveChangesAsync();

            // Delete the user
            var result = await _userManager.DeleteAsync(user);
            if (result.Succeeded)
            {
                TempData["SuccessMessage"] = "User deleted successfully.";
            }
            else
            {
                TempData["ErrorMessage"] = "Error deleting user.";
            }
            
            return RedirectToAction("Users");
        }

        // Instructor Management for Admin
        public async Task<IActionResult> Instructors()
        {
            try
            {
                var instructors = await _context.Instructors
                    .Include(i => i.Department)
                    .ToListAsync();
                return View(instructors);//database 
            }
            catch (Exception ex)
            {
                // Database schema not updated - return empty list with message
              
                return View(new List<Instructor>());
            }
        }

        public IActionResult CreateInstructor()
        {
            ViewData["DepartmentId"] = new SelectList(_context.Departments, "Id", "Name");
            ViewData["Users"] = new SelectList(_userManager.Users, "Id", "Email");
            return View();
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> CreateInstructor(Instructor instructor)
        {
            if (ModelState.IsValid)
            {
                _context.Instructors.Add(instructor);
                await _context.SaveChangesAsync();
                return RedirectToAction("Instructors");
            }
            ViewData["DepartmentId"] = new SelectList(_context.Departments, "Id", "Name", instructor.DepartmentId);
            ViewData["Users"] = new SelectList(_userManager.Users, "Id", "Email", instructor.IdentityUserId);
            return View(instructor);
        }

        public async Task<IActionResult> EditInstructor(int id)
        {
            var instructor = await _context.Instructors.FindAsync(id);
            if (instructor == null)
                return NotFound();

            ViewData["DepartmentId"] = new SelectList(_context.Departments, "Id", "Name", instructor.DepartmentId);
            ViewData["Users"] = new SelectList(_userManager.Users, "Id", "Email", instructor.IdentityUserId);
            return View(instructor);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> EditInstructor(int id, Instructor instructor)
        {
            if (id != instructor.Id)
                return NotFound();

            if (ModelState.IsValid)
            {
                _context.Update(instructor);
                await _context.SaveChangesAsync();
                return RedirectToAction("Instructors");
            }
            ViewData["DepartmentId"] = new SelectList(_context.Departments, "Id", "Name", instructor.DepartmentId);
            ViewData["Users"] = new SelectList(_userManager.Users, "Id", "Email", instructor.IdentityUserId);
            return View(instructor);
        }

        public async Task<IActionResult> DeleteInstructor(int id)
        {
            var instructor = await _context.Instructors
                .Include(i => i.Department)
                .FirstOrDefaultAsync(i => i.Id == id);
            if (instructor == null)
                return NotFound();

            return View(instructor);
        }

        [HttpPost, ActionName("DeleteInstructor")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> DeleteInstructorConfirmed(int id)
        {
            var instructor = await _context.Instructors
                .Include(i => i.InstructorCourses)
                .FirstOrDefaultAsync(i => i.Id == id);

            if (instructor != null)
            {
                // Remove instructor-course relationships first to satisfy FK constraints
                if (instructor.InstructorCourses != null && instructor.InstructorCourses.Any())
                {
                    _context.InstructorCourses.RemoveRange(instructor.InstructorCourses);
                }

                _context.Instructors.Remove(instructor);
                await _context.SaveChangesAsync();
            }
            return RedirectToAction("Instructors");
        }

        public async Task<IActionResult> InstructorDetails(int id)
        {
            var instructor = await _context.Instructors
                .Include(i => i.Department)
                .Include(i => i.InstructorCourses)
                    .ThenInclude(ic => ic.InstructorCourse_course)
                .FirstOrDefaultAsync(i => i.Id == id);
            if (instructor == null)
                return NotFound();

            return View(instructor);
        }
    }
}
