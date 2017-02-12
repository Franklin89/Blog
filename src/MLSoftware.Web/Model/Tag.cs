using System.Collections.Generic;

namespace MLSoftware.Web.Model
{
    public class Tag
    {
        public int Id { get; set; }
        public string Description { get; set; }

        public List<PostTag> PostTags { get; set; }
    }
}
