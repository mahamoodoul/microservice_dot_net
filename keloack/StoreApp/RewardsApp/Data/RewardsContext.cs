using Microsoft.EntityFrameworkCore;
using RewardsApp.Models;

namespace RewardsApp.Data
{
    public class RewardsContext : DbContext
    {
        public RewardsContext(DbContextOptions<RewardsContext> options) : base(options)
        {
        }

        public DbSet<Reward> Rewards { get; set; }
    }
}
