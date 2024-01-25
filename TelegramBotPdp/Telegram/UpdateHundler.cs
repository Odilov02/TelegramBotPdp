using Microsoft.EntityFrameworkCore;
using Telegram.Bot;
using Telegram.Bot.Polling;
using Telegram.Bot.Types.Enums;
using TelegramBotPdp.Models;

namespace TelegramBotPdp.Telegram
{
    public class UpdateHundler : IUpdateHandler
    {

        public Task HandlePollingErrorAsync(ITelegramBotClient botClient, Exception exception, CancellationToken cancellationToken)
        {
            return Task.CompletedTask;
        }

        public async Task HandleUpdateAsync(ITelegramBotClient botClient, global::Telegram.Bot.Types.Update update, CancellationToken cancellationToken)
        {
            string? student = null;
            var dbContext = new AppDbContext(new DbContextOptions<AppDbContext>();
            //  Student? student = await dbContext.Students.FirstOrDefaultAsync();
            if (update.Message!.Type != MessageType.Text)
                await botClient.SendTextMessageAsync(update.Message!.Chat.Id, "Botga faqat text habar yuborishingiz mumkun!");
            else
            {
                if (update.Message!.Text == "/start")
                    await botClient.SendTextMessageAsync(update.Message!.Chat.Id, "Assalomu alekum! \n Pdp Online Tasdiqlashga xush kelibsiz");
                await botClient.SendTextMessageAsync(update.Message!.Chat.Id, "Assalomu alekum! \n Pdp Online Tasdiqlashga xush kelibsiz");
                if (student is null)
                {
                    var result = await dbContext.Students.AddAsync(new Student()
                    {
                        Id = update.Message.Chat.Id.ToString(),
                        Created = DateTime.Now,
                        IsConfirmed = false,
                        IsRightInformation = false,
                        Name = "salom",
                        PhoneNumber = "sss",
                        Reason = "saaaaaaaaa",
                        State = "salom"
                    });
                    //int a = dbContext.SaveChanges();
                    if (await dbContext.SaveChangesAsync() == 1)
                        await Console.Out.WriteLineAsync($"yangi student qoshildi \nchatId: {update.Message!.Chat.Id}");
                    else
                        await Console.Out.WriteLineAsync("xatolik!");
                    await botClient.SendTextMessageAsync(update.Message!.Chat.Id, "Ism Familiyaingizni kiriting!");

                }
            }
        }

    }
}
