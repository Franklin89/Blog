using System;
using System.Collections.Generic;

namespace MLSoftware.Web.Model
{
    public class Post
    {
        public int Id { get; set; }
        public string Title { get; set; }
        public string Author { get; set; }
        public string Description { get; set; }
        public DateTime? Published { get; set; }
        public DateTime? Created { get; set; }

        public List<PostTag> PostTags { get; set; }

        public PostContent Content { get; set; }

        public List<Comment> Comments { get; set; }
    }
}
