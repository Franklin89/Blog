namespace MLSoftware.Web.Model
{
    public class PostContent
    {
        public int Id { get; set; }
        public string Content { get; set; }

        public override string ToString()
        {
            return Content;
        }
    }
}