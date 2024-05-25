using System.Reflection;
using Microsoft.EntityFrameworkCore;
using SlothfulCrud.Api.Domain;

namespace SlothfulCrud.Api.EF
{
    public class SlothfulDbContext : DbContext
    {
        public DbSet<Sloth> Sloths { get; set; }
        
        public SlothfulDbContext(DbContextOptions<SlothfulDbContext> options) : base(options)
        {
        }

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            modelBuilder.ApplyConfigurationsFromAssembly(Assembly.GetExecutingAssembly());
        }
    }
}