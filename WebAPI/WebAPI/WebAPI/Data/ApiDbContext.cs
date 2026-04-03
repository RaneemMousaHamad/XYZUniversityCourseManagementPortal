  using Microsoft.EntityFrameworkCore;
    using System.Collections.Generic;
    using System.Reflection.Emit;
    using WebAPI.Models;

    namespace WebAPI.Data
{
        public class ApiDbContext : DbContext
        {
            public ApiDbContext(DbContextOptions<ApiDbContext> options) : base(options) { }
            public DbSet<Course> Courses { get; set; } = null!;


            protected override void OnModelCreating(ModelBuilder modelBuilder)
            {
                base.OnModelCreating(modelBuilder);

            }
        }

   
}
