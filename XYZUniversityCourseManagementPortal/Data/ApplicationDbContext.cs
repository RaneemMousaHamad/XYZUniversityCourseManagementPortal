using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Identity.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore;
using XYZUniversityCourseManagementPortal.Models;
using Microsoft.AspNetCore.Mvc;

namespace XYZUniversityCourseManagementPortal.Data
{
    public class ApplicationDbContext(DbContextOptions<ApplicationDbContext> options) : IdentityDbContext(options)
    {
        public DbSet<Course> Courses { get; set; }
        public DbSet<Department> Departments { get; set; }
        public DbSet<Enrollment> Enrollments { get; set; }
        public DbSet<Grade> Grades { get; set; }
        public DbSet<Instructor> Instructors { get; set; }
        public DbSet<InstructorCourse> InstructorCourses { get; set; }
        public DbSet<Student> Students { get; set; }

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            /////Enrollment

            base.OnModelCreating(modelBuilder);
            modelBuilder.Entity<Enrollment>()
                .HasKey(ep => new {
                    ep.CourseId,
                    ep.StudentId
                });
            modelBuilder.Entity<Enrollment>()
                .HasOne(ap => ap.Enrollment_course)
                .WithMany(ap => ap.Enrollments)
                .HasForeignKey(ap => ap.CourseId)
                .OnDelete(DeleteBehavior.Restrict);

            modelBuilder.Entity<Enrollment>()
                .HasOne(ap => ap.Enrollment_student)
                .WithMany(ap => ap.Enrollments)
                .HasForeignKey(ap => ap.StudentId)
                .OnDelete(DeleteBehavior.Restrict);

            modelBuilder.Entity<Enrollment>()
                .HasOne(e => e.Grade)
                .WithOne(g => g.Enrollment)
                .HasForeignKey<Enrollment>(e => e.GradeId)
                .IsRequired(false)
                .OnDelete(DeleteBehavior.SetNull);


            ///////InstructorCourse

            modelBuilder.Entity<InstructorCourse>()
                .HasKey(ep => new {
                    ep.CourseId,
                    ep.InstructorId
                });
            modelBuilder.Entity<InstructorCourse>()
                .HasOne(ap => ap.InstructorCourse_course)
                .WithMany(ap => ap.InstructorCourses)
                .HasForeignKey(ap => ap.CourseId)
                .OnDelete(DeleteBehavior.Restrict);

            modelBuilder.Entity<InstructorCourse>()
                .HasOne(ap => ap.InstructorCourse_instructor)
                .WithMany(ap => ap.InstructorCourses)
                .HasForeignKey(ap => ap.InstructorId)
                .OnDelete(DeleteBehavior.Restrict);

            // Link Identity users to Student/Instructor profiles (optional at DB level)
            modelBuilder.Entity<Student>()
                .HasOne<IdentityUser>()
                .WithMany()
                .HasForeignKey(s => s.IdentityUserId)
                .OnDelete(DeleteBehavior.Cascade);

            modelBuilder.Entity<Student>()
                .HasIndex(s => s.IdentityUserId)
                .IsUnique()
                .HasFilter("[IdentityUserId] IS NOT NULL");

            modelBuilder.Entity<Instructor>()
                .HasOne<IdentityUser>()
                .WithMany()
                .HasForeignKey(i => i.IdentityUserId)
                .OnDelete(DeleteBehavior.Cascade);

            modelBuilder.Entity<Instructor>()
                .HasIndex(i => i.IdentityUserId)
                .IsUnique()
                .HasFilter("[IdentityUserId] IS NOT NULL");

        }



    }
}
