using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Hosting;
using Microsoft.Extensions.Logging;
using MLSoftware.Web.ViewModels;
using System.Linq;
using System.Threading.Tasks;
using MLSoftware.Web.Services;
using System;

namespace MLSoftware.Web.Controllers
{
    public class HomeController : Controller
    {
        private readonly IHostingEnvironment _env;
        private readonly ILogger _logger;
        private readonly IPostRepository _postRepository;
        private readonly IFeedService _feedService;

        public HomeController(IHostingEnvironment env, ILogger<HomeController> logger, IPostRepository postRepository, IFeedService feedService)
        {
            _env = env;
            _logger = logger;
            _postRepository = postRepository;
            _feedService = feedService;
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

        [HttpGet]
        [Route("/feed.atom")]
        public IActionResult GetFeed()
        {
            var request = Url.ActionContext.HttpContext.Request;
            var absoluteRoot = new Uri(request.Scheme + "://" + request.Host.Value).ToString();
            return Ok(_feedService.GetFeed(absoluteRoot));
        }
    }
}
