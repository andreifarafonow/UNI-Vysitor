using Microsoft.EntityFrameworkCore;

namespace UniALPRMain.Models
{
    public class VysitorDbContext : DbContext
    {
        public DbSet<Camera> Cameras { get; set; }
        public DbSet<Car> Cars { get; set; }
        public DbSet<Person> Persons { get; set; }
        public DbSet<Event> Events { get; set; }
        public DbSet<Cookie> Cookies { get; set; }
        public DbSet<UnauthorizedRequest> UnauthorizedRequests { get; set; }

        public VysitorDbContext(DbContextOptions<VysitorDbContext> options) : base(options)
        {
            AppContext.SetSwitch("Npgsql.EnableLegacyTimestampBehavior", true);

            Database.EnsureCreated();
        }
    }
}
