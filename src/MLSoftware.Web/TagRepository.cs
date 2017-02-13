using System;
using MLSoftware.Web.Model;
using System.Linq;

namespace MLSoftware.Web
{
    public class TagRepository : ITagRepository
    {
        private readonly BlogContext _dbContext;

        public TagRepository(BlogContext dbContext)
        {
            _dbContext = dbContext;
        }

        public void Add(Tag tag)
        {
            if (tag == null)
            {
                throw new ArgumentNullException(nameof(tag));
            }
            _dbContext.Add(tag);
            _dbContext.SaveChanges();
        }

        public Tag Get(string description)
        {
            return _dbContext.Tag.SingleOrDefault(x => x.Description.Equals(description, StringComparison.OrdinalIgnoreCase));
        }
    }
}
