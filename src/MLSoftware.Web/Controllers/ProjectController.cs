using Microsoft.AspNetCore.Mvc;

namespace MLSoftware.Web.Controllers
{
    [Route("[controller]")]
    public class ProjectController : Controller
    {
        public IActionResult Index()
        {
            return View();
        }
    }
}
