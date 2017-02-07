using Microsoft.AspNetCore.Mvc;

namespace MLSoftware.Web.Controllers
{
    public class ProjectController : Controller
    {
        public IActionResult Index()
        {
            return View();
        }
    }
}
