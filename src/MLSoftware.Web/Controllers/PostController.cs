using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;
using MLSoftware.Web.Model;
using MLSoftware.Web.ViewModels;
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;

namespace MLSoftware.Web.Controllers
{
    [Authorize("Admin")]
    [Route("[controller]")]
    public class PostController : Controller
    {
        private readonly IHostingEnvironment _env;
        private readonly ILogger _logger;
        private readonly IPostRepository _postRepository;
        private readonly ITagRepository _tagRepository;
        private readonly ICommentRepository _commentRepository;

        public PostController(
            IHostingEnvironment env,
            ILogger<PostController> logger,
            IPostRepository postRepository,
            ITagRepository tagRepository,
            ICommentRepository commentRepository)
        {
            _env = env;
            _logger = logger;
            _postRepository = postRepository;
            _tagRepository = tagRepository;
            _commentRepository = commentRepository;
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
                Content = draft.Content.Content,
                IsDraft = draft.IsDraft,
                Tags = String.Join(", ", draft.PostTags.Select(x => x.Tag.Description))
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

                post.PostTags.Clear();

                var tags = input.Tags.Split(',');
                foreach (var tag in tags)
                {
                    var dbTag = _tagRepository.Get(tag.Trim());
                    if (dbTag == null)
                    {
                        dbTag = new Tag
                        {
                            Description = tag.Trim()
                        };
                    }

                    post.PostTags.Add(new PostTag
                    {
                        Tag = dbTag
                    });
                }

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

            post.PostTags = new List<PostTag>();

            var tags = input.Tags.Split(',');
            foreach (var tag in tags)
            {
                var dbTag = _tagRepository.Get(tag.Trim());
                if (dbTag == null)
                {
                    dbTag = new Tag
                    {
                        Description = tag.Trim()
                    };
                }

                post.PostTags.Add(new PostTag
                {
                    Tag = dbTag
                });
            }

            _postRepository.Add(post);

            return RedirectToAction(nameof(Details), new
            {
                id = post.Id
            });
        }

        [HttpPost]
        [Route("[action]")]
        public IActionResult Comment(CommentViewModel input)
        {
            if (!input.RiddleResult)
            {
                ModelState.AddModelError("RiddleResultValue", "Please solve the math problem. Let's fight spam together!");

                // For more information see: https://weblog.west-wind.com/posts/2012/apr/20/aspnet-mvc-postbacks-and-htmlhelper-controls-ignoring-model-changes
                ModelState.Remove("RiddleValue1");
                ModelState.Remove("RiddleValue2");
            }

            if (!ModelState.IsValid)
            {
                return View(nameof(Details), _postRepository.Get(input.PostId));
            }

            var comment = new Comment
            {
                PostId = input.PostId,
                Created = DateTime.UtcNow,
                Name = input.Name,
                Email = input.Email,
                Content = input.Content
            };

            _commentRepository.Add(comment);

            return RedirectToAction(nameof(Details), new { id = input.PostId });
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

            public string Tags { get; set; }
        }
    }
}
