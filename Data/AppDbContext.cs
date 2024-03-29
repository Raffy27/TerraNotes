using Microsoft.EntityFrameworkCore;
using TerraNotes.Models;

namespace TerraNotes.Data
{
    public class AppDbContext: DbContext
    {
        public AppDbContext(DbContextOptions<AppDbContext> options) : base(options)
        {
        }

        public DbSet<User> Users { get; set; }
        public DbSet<APIKey> APIKeys { get; set; }
        public DbSet<Note> Notes { get; set; }
    }
}