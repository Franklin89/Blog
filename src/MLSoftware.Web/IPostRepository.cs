using MLSoftware.Web.Model;
using System.Collections.Generic;

namespace MLSoftware.Web
{
    public interface IPostRepository
    {
        IEnumerable<Post> GetDrafts();
        IEnumerable<Post> GetPostMetadata();
        IEnumerable<Post> GetPostMetadata(int count);

        Post Get(int id);

        void Delete(int id);

        void Publish(int id);
        void Unpublish(int id);

        void Update(Post post);
        void Add(Post post);
    }
}
