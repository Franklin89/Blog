using MLSoftware.Web.Model;

namespace MLSoftware.Web
{
    public interface ICommentRepository
    {
        void Add(Comment comment);
        void Remove(int id);
    }
}
