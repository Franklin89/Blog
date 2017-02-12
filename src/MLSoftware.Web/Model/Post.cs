using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations.Schema;

namespace MLSoftware.Web.Model
{
    public class Post
    {
        public int Id { get; set; }
        public string Title { get; set; }
        public string Author { get; set; }
        public string Description { get; set; }
        public DateTime? Published { get; set; }

        public List<PostTag> PostTags { get; set; }

        public int PostContentId { get; set; }
        public PostContent Content { get; set; }

        [NotMapped]
        public bool IsDraft => Published == null;
    }
}
