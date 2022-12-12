using Microsoft.EntityFrameworkCore;

namespace AlprChecker.Models
{
    public class AlprDbContext : DbContext
    {
        public DbSet<Camera> Cameras { get; set; }
        public DbSet<Car> Cars { get; set; }
        public DbSet<Person> Persons { get; set; }
        public DbSet<Event> Events { get; set; }

        public AlprDbContext(DbContextOptions<AlprDbContext> options) : base(options)
        {
            AppContext.SetSwitch("Npgsql.EnableLegacyTimestampBehavior", true);

            //Database.EnsureCreated();
        }
    }
}
