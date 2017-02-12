using Microsoft.EntityFrameworkCore;
using MLSoftware.Web.Model;

namespace MLSoftware.Web
{
    public class BlogContext : DbContext
    {
        public DbSet<Post> Post { get; set; }
        public DbSet<PostTag> PostTag { get; set; }
        public DbSet<Tag> Tag { get; set; }

        public DbSet<PostContent> PostContent { get; set; }

        public BlogContext(DbContextOptions options) : base(options)
        {
            this.Database.EnsureCreated();
        }
    }
}
