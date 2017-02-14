using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using System.Linq;

namespace MLSoftware.Web.Controllers
{
    [Authorize("Admin")]
    [Route("[controller]")]
    public class DraftController : Controller
    {
        private readonly IPostRepository _postRepository;

        public DraftController(IPostRepository postRepository)
        {
            _postRepository = postRepository;
        }
        
        public IActionResult Index()
        {
            var draftPosts = _postRepository.GetDrafts();
            return View(draftPosts.ToList());
        }
    }
}
