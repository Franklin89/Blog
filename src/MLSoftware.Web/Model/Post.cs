using System.Collections.Generic;

namespace MLSoftware.Web.Model
{
    public class Post : PostFrontMatter
    {
        public Post(string filename, IEnumerable<string> lines) : base(filename, lines)
        {
        }

        public string Content { get; set; }
    }
}
