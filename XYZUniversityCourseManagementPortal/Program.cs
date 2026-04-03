using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using Microsoft.VisualBasic;
//using SchoolRoles.Data;
using XYZUniversityCourseManagementPortal.Data;

var builder = WebApplication.CreateBuilder(args);

builder.Services.AddControllersWithViews();
//builder.Services.AddRazorPages();

var connectionString = builder.Configuration.GetConnectionString("DefaultConnection")
    ?? "Data Source=app.db";
builder.Services.AddDbContext<ApplicationDbContext>(options =>
    options.UseSqlServer(connectionString));

builder.Services.AddDefaultIdentity<IdentityUser>(options =>
{
    options.SignIn.RequireConfirmedAccount = false;
})
    .AddRoles<IdentityRole>()
    .AddEntityFrameworkStores<ApplicationDbContext>();

var app = builder.Build();

// Apply migrations and seed roles
using (var scope = app.Services.CreateScope())
{
    var services = scope.ServiceProvider;
    var db = services.GetRequiredService<ApplicationDbContext>();
    
    // Apply pending migrations
    try
    {
        // Check if database exists and apply migrations
        if (db.Database.CanConnect())
        {
            var pendingMigrations = await db.Database.GetPendingMigrationsAsync();
            if (pendingMigrations.Any())
            {
                await db.Database.MigrateAsync();
            }
        }
        else
        {
            // Database doesn't exist, create it with migrations
            await db.Database.MigrateAsync();
        }
    }
    catch (Exception ex)
    {
        // Log the error but continue - migrations might already be applied
        // If you see database errors, run the FixDatabase.sql script manually
        Console.WriteLine($"Migration warning: {ex.Message}");
    }

    // Seed roles: Admin, IT, Users (from RoleSeeder)
    await RoleSeeder.SeedRolesAsync(services);
    
    // Also seed additional roles: Instructor, Student (for the course management system)
    await SeedAdditionalRolesAndAdminAsync(services);
}

if (!app.Environment.IsDevelopment())
{
    app.UseExceptionHandler("/Home/Error");
    app.UseHsts();
}

app.UseHttpsRedirection();
app.UseStaticFiles();

app.UseRouting();
app.UseAuthentication();
app.UseAuthorization();

app.MapControllers(); // Map API controllers
app.MapControllerRoute(
    name: "default",
    pattern: "{controller=Home}/{action=Index}/{id?}");
app.MapRazorPages();

app.Run();
async Task SeedAdditionalRolesAndAdminAsync(IServiceProvider service)
{
    var roleManger = service.GetRequiredService<RoleManager<IdentityRole>>();
    var userManger = service.GetRequiredService<UserManager<IdentityUser>>();

    // Seed additional roles for the course management system
    string[] additionalRoles = { "Instructor", "Student" };
    foreach (var role in additionalRoles)
    {
        if (!await roleManger.RoleExistsAsync(role))
            await roleManger.CreateAsync(new IdentityRole(role));
    }
    
    // Create default admin user
    string adminEmail = "Admin@htu.edu.jo";
    string adminPassword = "Admin@123";

    var adminUser = await userManger.FindByEmailAsync(adminEmail);
    if (adminUser == null)
    {
        adminUser = new IdentityUser
        {
            UserName = adminEmail,
            Email = adminEmail,
            EmailConfirmed = true
        };
        await userManger.CreateAsync(adminUser, adminPassword);
        await userManger.AddToRoleAsync(adminUser, "Admin");
    }
}



