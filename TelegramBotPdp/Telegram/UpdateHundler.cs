using Microsoft.EntityFrameworkCore;
using Telegram.Bot;
using Telegram.Bot.Polling;
using Telegram.Bot.Types.Enums;
using TelegramBotPdp.Models;

namespace TelegramBotPdp.Telegram;

public class UpdateHundler : IUpdateHandler
{

    public Task HandlePollingErrorAsync(ITelegramBotClient botClient, Exception exception, CancellationToken cancellationToken)
    {
        return Task.CompletedTask;
    }

    public async Task HandleUpdateAsync(ITelegramBotClient botClient, global::Telegram.Bot.Types.Update update, CancellationToken cancellationToken)
    {


        try
        {
            var dbContext = new AppDbContext();
            if (update.Message is not null)
            {
                if (update.Message!.Type == MessageType.Text)
                {
                    Student? student = await dbContext.Students.FirstOrDefaultAsync(x => x.Id == update.Message!.Chat.Id.ToString());
                    if (student is null)
                    {
                        await botClient.SendTextMessageAsync(update.Message!.Chat.Id, "Assalomu alekum! \n Pdp Online Tasdiqlashga xush kelibsiz");
                        await BotService.CreateUserAsync(botClient, update, dbContext);
                        if (update.Message.Chat.Id.ToString() == ConfigurationService._configuration!["Admin"])
                        {
                            var admin = (await dbContext.Students.FirstOrDefaultAsync(x => x.Id == ConfigurationService._configuration!["Admin"])) ?? new Student();
                            admin.State = State.ForAdmin.ToString();
                            dbContext.Update(admin);
                            await dbContext.SaveChangesAsync();
                        }
                    }
                    else
                    {
                        await ChoiceSwitch(botClient, update, dbContext, student);
                    }
                }
            }
            else
            {
                global::Telegram.Bot.Types.CallbackQuery? callbackQuery = update.CallbackQuery;
                    var student = (await dbContext.Students.FirstOrDefaultAsync(x => x.Id == callbackQuery!.From.Id.ToString())) ?? new Student();
                if (callbackQuery is not null)
                {
                    if (student.State == State.ForIsRightInformation.ToString())
                    {
                        await BotService.ForIsRightInformation(botClient, update, dbContext);
                    }
                    else if(State.ForAdmin.ToString()==student.State)
                    {
                        if (callbackQuery.From.Id.ToString() == ConfigurationService._configuration!["Admin"])
                        {
                            await BotService.ForAdminConfirmedAsync(botClient, update, dbContext);
                        }
                    }
                }
            }
        }
        catch (Exception e)
        {
              await botClient.SendTextMessageAsync(ConfigurationService._configuration!["Admin"]!, $"Xatolik chiqdi!\n\n{e}");
        }
    }
    static async Task ChoiceSwitch(ITelegramBotClient botClient, global::Telegram.Bot.Types.Update update, AppDbContext dbContext, Student student)
    {
        if (update.Message!.Text == "/start")
            await BotService.ForStart(botClient, update, dbContext);
        else
        {
            switch (student!.State)
            {
                case "ForFullName":
                    await BotService.ForFullNameAsync(botClient, update, dbContext); break;
                case "ForPhoneNumber":
                    await BotService.ForPhoneNumberAsync(botClient, update, dbContext); break;
                case "ForReason":
                    await BotService.ForReasonAsync(botClient, update, dbContext); break;
                            case "ForConfirmed":
                    await BotService.CreateQRCodeAsync(botClient, update, dbContext); break;
            }
        }
    }
}
