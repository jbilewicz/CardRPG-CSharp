using Microsoft.EntityFrameworkCore;
using CardRPG.Models;

namespace CardRPG.Data;

public class GameDBContext : DbContext
{
    // Creating db table named Users
    public DbSet<User> Users {get;set;}

    //connection configuration
    protected override void OnConfiguring(DbContextOptionsBuilder optionsBuilder)
    {
        optionsBuilder.UseSqlite("Data Source = cardrpg.db");
    }
}