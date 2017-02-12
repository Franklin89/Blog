using System.Collections.Generic;
using MLSoftware.Web.Model;
using System.Linq;
using Microsoft.EntityFrameworkCore;
using System;

namespace MLSoftware.Web
{
    public class PostRepository : IPostRepository
    {
        private readonly BlogContext _dbContext;

        public PostRepository(BlogContext dbContext)
        {
            _dbContext = dbContext;
        }

        public Post Get(int id)
        {
            return _dbContext.Post
                .Include(x => x.Content)
                .Include(x => x.PostTags)
                    .ThenInclude(pt => pt.Tag)
                .SingleOrDefault(x => x.Id == id);
        }

        public IEnumerable<Post> GetDrafts()
        {
            var posts = _dbContext.Post
                .Include(x => x.PostTags)
                    .ThenInclude(pt => pt.Tag)
                .Where(x => x.Published == null);
            return posts;
        }

        public IEnumerable<Post> GetPostMetadata()
        {
            var posts = _dbContext.Post
                .Include(x => x.PostTags)
                    .ThenInclude(pt => pt.Tag)
                .Where(x => x.Published != null)
                .OrderByDescending(x => x.Published);
            return posts;
        }

        public IEnumerable<Post> GetPostMetadata(int count)
        {
            var posts = _dbContext.Post
                .Include(x => x.PostTags)
                    .ThenInclude(pt => pt.Tag)
                .Where(x => x.Published != null)
                .OrderByDescending(x => x.Published)
                .Take(count);
            return posts;
        }

        public void Publish(int id)
        {
            var post = _dbContext.Post.Single(x => x.Id == id);
            post.Published = DateTime.UtcNow;
            _dbContext.SaveChanges();
        }

        public void Unpublish(int id)
        {
            var post = _dbContext.Post.Single(x => x.Id == id);
            post.Published = null;
            _dbContext.SaveChanges();
        }

        public void Update(Post post)
        {
            if(post == null)
            {
                throw new ArgumentNullException(nameof(post));
            }
            _dbContext.SaveChanges();
        }

        public void Delete(int id)
        {
            throw new NotImplementedException();
        }

        public void Add(Post post)
        {
            if (post == null)
            {
                throw new ArgumentNullException(nameof(post));
            }
            _dbContext.Add(post);
            _dbContext.SaveChanges();
        }
    }
}
