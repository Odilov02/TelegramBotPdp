
using Microsoft.EntityFrameworkCore;
using TelegramBotPdp.Models;

namespace TelegramBotPdp;

public class AppDbContext : DbContext
{
    public DbSet<Student> Students { get; set; }

    protected override void OnConfiguring(DbContextOptionsBuilder optionsBuilder)
    {
        optionsBuilder.UseSqlServer("Data Source=ServerName;Initial Catalog=DatabaseName;User ID=UserName;Password=Password");
        base.OnConfiguring(optionsBuilder);
    }
}