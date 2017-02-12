using System.Collections.Generic;
using MLSoftware.Web.Model;

namespace MLSoftware.Web.ViewModels
{
    public class HomeViewModel
    {
        public IEnumerable<Post> Posts { get; set; }
    }
}