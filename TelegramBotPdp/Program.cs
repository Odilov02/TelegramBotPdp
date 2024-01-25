using Microsoft.EntityFrameworkCore;
using TelegramBotPdp.Telegram;
namespace TelegramBotPdp;

public class Program
{
    public static void Main(string[] args)
    {
        try
        {

            AppContext.SetSwitch("Npgsql.EnableLegacyTimestampBehavior", true);
            var builder = WebApplication.CreateBuilder(args);
            builder.Services.AddControllers();
            builder.Services.AddEndpointsApiExplorer();
            builder.Services.AddDbContext<AppDbContext>(options =>
            {
                options.UseNpgsql(builder.Configuration["Default"]);
            });
            builder.Services.AddTelegramBot(builder.Configuration);
            var app = builder.Build();
            app.UseHttpsRedirection();

            app.UseAuthorization();


            app.MapControllers();

            app.Run();

        }
        catch (Exception e)
        {
            Console.WriteLine(e.Message);
        }
    }
}
