using MLSoftware.Web.Model;

namespace MLSoftware.Web
{
    public interface ITagRepository
    {
        Tag Get(string description);
        void Add(Tag tag);
    }
}
