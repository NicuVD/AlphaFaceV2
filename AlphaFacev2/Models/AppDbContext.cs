using Microsoft.EntityFrameworkCore;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace AlphaFacev2.Models
{
    public class AppDbContext: DbContext
    {
        public AppDbContext(DbContextOptions<AppDbContext> options) : base(options)
        {

        }

        public virtual DbSet<Profile> Profile { get; set; }
        public virtual DbSet<History> History { get; set; }
        public virtual DbSet<Face> Face { get; set; }
        public DbSet<Feedback> Feedbacks { get; set; }
    }
}
