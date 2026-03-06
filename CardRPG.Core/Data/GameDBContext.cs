using Microsoft.EntityFrameworkCore;
using CardRPG.Core.Models;

namespace CardRPG.Core.Data;

public class GameDBContext : DbContext
{
    public DbSet<User> Users { get; set; }

    protected override void OnConfiguring(DbContextOptionsBuilder optionsBuilder)
    {
        optionsBuilder.UseSqlite("Data Source = cardrpg.db");
    }
}