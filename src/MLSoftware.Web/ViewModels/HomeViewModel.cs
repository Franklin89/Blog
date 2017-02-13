using System.Collections.Generic;

namespace MLSoftware.Web.ViewModels
{
    public class HomeViewModel
    {
        public IEnumerable<PostViewModel> Posts { get; set; }
    }
}