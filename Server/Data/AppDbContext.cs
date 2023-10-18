using Library.Models;
using Microsoft.EntityFrameworkCore;

namespace Server.Data
{
    public class AppDbContext : DbContext
    {
        public AppDbContext(DbContextOptions<AppDbContext> options) : base(options)
        {
        }
        public DbSet<Registration> Registrations { get; set; }
        public DbSet<UserRole> UserRoles { get; set; }
        public DbSet<TokenInfo> TokenInfo { get; set; }
        public DbSet<PostJob> Jobs { get; set; }
        public DbSet<CompanyInfo> CompanyInfo { get; set; }
        public DbSet<Category> Categories { get; set; }
    }
}
