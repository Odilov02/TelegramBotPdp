using Microsoft.EntityFrameworkCore;
using TelegramBotPdp.Models;

namespace TelegramBotPdp;

public class AppDbContext : DbContext
{
    public DbSet<Student> Students { get; set; }
    public AppDbContext(DbContextOptions<AppDbContext> options)
       : base(options)
    {

    }
    protected override void OnConfiguring(DbContextOptionsBuilder optionsBuilder)
    {
        optionsBuilder.UseNpgsql("Server=::1;Port=5432;Database=BotPDP;User Id=postgres;Password=020819;");
    }
}
