using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;
using MLSoftware.Web.Model;
using MLSoftware.Web.ViewModels;
using System;
using System.ComponentModel.DataAnnotations;

namespace MLSoftware.Web.Controllers
{
    [Authorize("Admin")]
    [Route("[controller]")]
    public class PostController : Controller
    {
        private readonly IHostingEnvironment _env;
        private readonly ILogger _logger;
        private readonly IPostRepository _postRepository;

        public PostController(IHostingEnvironment env, ILogger<PostController> logger, IPostRepository postRepository)
        {
            _env = env;
            _logger = logger;
            _postRepository = postRepository;
        }

        [HttpGet]
        [AllowAnonymous]
        public IActionResult Index()
        {
            var postMetadata = _postRepository.GetPostMetadata();
            var viewModel = new HomeViewModel
            {
                Posts = postMetadata
            };
            return View(viewModel);
        }

        [HttpGet]
        [AllowAnonymous]
        [Route("[action]/{id}")]
        public IActionResult Details(int id)
        {
            var post = _postRepository.Get(id);

            if (post == null || (post.Published == null && !User.Identity.IsAuthenticated))
            {
                return NotFound();
            }

            ViewData["Title"] = post.Title;

            post.Content = new PostContent
            {
                Id = post.Content.Id,
                Content = post.Content.Parse()
            };

            return View(post);
        }

        [HttpGet]
        [Route("[action]/{id}")]
        public IActionResult Publish(int id)
        {
            _postRepository.Publish(id);
            return RedirectToAction("Index", "Home");
        }

        [HttpGet]
        [Route("[action]/{id}")]
        public IActionResult Unpublish(int id)
        {
            _postRepository.Unpublish(id);
            return RedirectToAction("Index", "Home");
        }

        [HttpGet]
        [Route("[action]/{id}")]
        public IActionResult Edit(int id)
        {
            var draft = _postRepository.Get(id);
            var viewModel = new PostViewModel
            {
                Id = draft.Id,
                Title = draft.Title,
                Description = draft.Description,
                Content = draft.Content.Content
            };
            return View(viewModel);
        }

        [HttpPost]
        [Route("[action]/{id}")]
        public IActionResult Edit(int id, PostInputModel input)
        {
            if (!ModelState.IsValid)
            {
                // Model validation error, just return and let the error render
                var viewModel = new PostViewModel();
                return View(nameof(Edit), viewModel);
            }

            var post = _postRepository.Get(input.Id);

            if (post != null)
            {
                post.Title = input.Title;
                post.Description = input.Description;


                post.Content.Content = input.Content;

                _postRepository.Update(post);
            }

            return RedirectToAction(nameof(Details), new { id = post.Id });
        }


        [HttpGet]
        [Route("[action]")]
        public IActionResult Create()
        {
            return View(new PostViewModel());
        }

        [HttpPost]
        [Route("[action]")]
        public IActionResult Create(PostInputModel input)
        {
            if (!ModelState.IsValid)
            {
                // Model validation error, just return and let the error render
                var viewModel = new PostViewModel();
                return View(nameof(Edit), viewModel);
            }

            var post = new Post()
            {
                Content = new PostContent
                {
                    Content = input.Content
                },

                Title = input.Title,
                Description = input.Description,
                Created = DateTime.UtcNow,
                Author = "Matteo"
            };

            _postRepository.Add(post);

            return RedirectToAction(nameof(Details), new
            {
                id = post.Id
            });
        }

        [ModelMetadataType(typeof(PostViewModel))]
        public class PostInputModel
        {
            [Required]
            public int Id { get; set; }

            [Required]
            public string Title { get; set; }

            public string Description { get; set; }

            public string Content { get; set; }
        }
    }
}
