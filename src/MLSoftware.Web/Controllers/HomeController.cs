using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Hosting;
using Microsoft.Extensions.Logging;
using MLSoftware.Web.ViewModels;
using System.Linq;

namespace MLSoftware.Web.Controllers
{
    public class HomeController : Controller
    {
        private readonly IHostingEnvironment _env;
        private readonly ILogger _logger;
        private readonly IPostRepository _postRepository;

        public HomeController(IHostingEnvironment env, ILogger<HomeController> logger, IPostRepository postRepository)
        {
            _env = env;
            _logger = logger;
            _postRepository = postRepository;
        }

        [Route("/")]
        public IActionResult Index()
        {
            var postMetadata = _postRepository.GetPostMetadata(5);
            var viewModel = new HomeViewModel {
                Posts = postMetadata.Select(x => new PostViewModel(x)).ToList()
            };
            return View(viewModel);
        }

        [Route("/error")]
        public IActionResult Error()
        {
            return View();
        }
    }
}
