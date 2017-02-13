using System;
using MLSoftware.Web.Model;

namespace MLSoftware.Web
{
    public class CommentRepository : ICommentRepository
    {
        private readonly BlogContext _dbContext;

        public CommentRepository(BlogContext dbContext)
        {
            _dbContext = dbContext;
        }

        public void Add(Comment comment)
        {
            if (comment == null)
            {
                throw new ArgumentNullException(nameof(comment));
            }
            _dbContext.Add(comment);
            _dbContext.SaveChanges();
        }

        public void Remove(int id)
        {
            throw new NotImplementedException();
        }
    }
}
