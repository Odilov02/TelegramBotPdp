namespace TelegramBotPdp;

public static class ConfigurationService
{
    public static  IConfiguration? _configuration;
    public static void AddConfiguration(this IServiceCollection services, IConfiguration configuration)
    {
        _configuration = configuration;
    }
}
