using TelegramBotPdp.Telegram;
namespace TelegramBotPdp;

public class Program
{
    public static   void Main(string[] args)
    {
        var builder = WebApplication.CreateBuilder(args);
        builder.Services.AddControllers();
        builder.Services.AddEndpointsApiExplorer();
        builder.Services.AddTelegramBot(builder.Configuration);
        var app = builder.Build();
        builder.Services.AddConfiguration(builder.Configuration);
        app.UseHttpsRedirection();
        app.UseAuthorization();
        app.MapControllers();
        app.Run();
    }
}
