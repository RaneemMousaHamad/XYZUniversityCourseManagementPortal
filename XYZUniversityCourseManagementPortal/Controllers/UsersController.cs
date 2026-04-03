using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace XYZUniversityCourseManagementPortal.Controllers
{
    [Authorize(Roles = "Users")]
    public class UsersController : Controller
    {
        public IActionResult Index()
        {
            return View();
        }
    }
}


