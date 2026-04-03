using Microsoft.AspNetCore.Identity;

namespace XYZUniversityCourseManagementPortal.Models
{
    public class ApplicationUser : IdentityUser
    {
        public string FirstName { get; set; }
        public string LastName { get; set; }

    }
}
