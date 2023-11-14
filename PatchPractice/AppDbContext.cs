using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Options;

namespace PatchPractice
{
    public class AppDbContext : DbContext
    {
        public DbSet<Person> Persons { get; set; }

        public AppDbContext(DbContextOptions<AppDbContext> options) : base(options)
        {
        }
    }
}
