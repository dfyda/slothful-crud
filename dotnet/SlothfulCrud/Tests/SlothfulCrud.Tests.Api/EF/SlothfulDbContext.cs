using System.Reflection;
using Microsoft.EntityFrameworkCore;
using SlothfulCrud.Tests.Api.Domain;

namespace SlothfulCrud.Tests.Api.EF
{
    public class SlothfulDbContext : DbContext
    {
        public DbSet<Sloth> Sloths { get; set; }
        public DbSet<WildKoala> Koalas { get; set; }
        
        public SlothfulDbContext(DbContextOptions<SlothfulDbContext> options) : base(options)
        {
            Configure();
        }

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            modelBuilder.ApplyConfigurationsFromAssembly(Assembly.GetExecutingAssembly());
        }

        public void Configure()
        {
            InitializeDatabase(this);
        }

        private static void InitializeDatabase(SlothfulDbContext dbContext)
        {
            if (!dbContext.Set<Sloth>().Any() && !dbContext.Set<WildKoala>().Any())
            {
                dbContext.Set<Sloth>()
                    .AddRange(
                        new Sloth(Guid.NewGuid(), "DapperSloth", 10),
                        new Sloth(Guid.NewGuid(), "EnergySloth", 1));
                dbContext.Set<WildKoala>()
                    .AddRange(
                        new WildKoala(Guid.NewGuid(), "SpeedyKoala", 11),
                        new WildKoala(Guid.NewGuid(), "DevKoala", 5));

                dbContext.SaveChanges();
            }
        }
    }
}