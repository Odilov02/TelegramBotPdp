using Telegram.Bot.Types;

namespace TelegramBotPdp.Models;

public class BotUser
{
    public ChatId? ChatId { get; set; }
    public string? Message { get; set; }
}

enum State
{
    ForStart=1,
    ForFirstName,
    ForLastName,
    ForPhoneNumber,
    ForReason,
    ForIsRightInformation,
    ForRightInformation,
    ForConfirmed,
    ForWait,
    ForQR,
    ForAdmin
}