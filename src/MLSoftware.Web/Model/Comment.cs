using System;

namespace MLSoftware.Web.Model
{
    public class Comment
    {
        public int Id { get; set; }
        public string Name { get; set; }
        public string Email { get; set; }
        public DateTime Created { get; set; }
        public string Content { get; set; }

        public int PostId { get; set; }
        public Post Post { get; set; }
    }
}
