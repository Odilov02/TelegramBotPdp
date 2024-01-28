using Microsoft.EntityFrameworkCore;
using TelegramBotPdp.Models;

namespace TelegramBotPdp;

public class AppDbContext : DbContext
{
    public DbSet<Student> Students { get; set; }

    protected override void OnConfiguring(DbContextOptionsBuilder optionsBuilder)
    {
        optionsBuilder.UseSqlite("Data Source=Data.db");
        base.OnConfiguring(optionsBuilder);
    }
}