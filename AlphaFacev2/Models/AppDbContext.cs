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

        public DbSet<Profile> Profile { get; set; }
        public DbSet<History> History { get; set; }
        public DbSet<Face> Face { get; set; }
        public DbSet<Feedback> Feedback { get; set; }
        public DbSet<ImageStore> ImageStore { get; set; }
    }
}
