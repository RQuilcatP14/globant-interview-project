using AntiFraudMicroservice.Domain.Models;
using Microsoft.EntityFrameworkCore;

namespace AntiFraudMicroservice.Infrastructure.Persistence
{
    public class AppDbContext : DbContext
    {
        public AppDbContext(DbContextOptions<AppDbContext> options) : base(options) { }

        public DbSet<Transaction> Transactions { get; set; }
    }
}
