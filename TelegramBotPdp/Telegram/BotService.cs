using Microsoft.EntityFrameworkCore;
using QRCoder;
using System.Drawing;
using Telegram.Bot;
using Telegram.Bot.Types;
using Telegram.Bot.Types.ReplyMarkups;
using TelegramBotPdp.Models;
namespace TelegramBotPdp.Telegram;
public class BotService
{
    public static async Task ForStart(ITelegramBotClient botClient, global::Telegram.Bot.Types.Update update, AppDbContext dbContext)
    {

        string id = update.Message!.Chat.Id.ToString();
        var student = await dbContext.Students.FirstOrDefaultAsync(x => x.Id == id);
        if (student!.IsRightInformation)
            if (student.IsConfirmed)
                await botClient.SendTextMessageAsync(id, "QR Code olishingiz mumkun", replyMarkup: new ReplyKeyboardRemove());
            else
                await botClient.SendTextMessageAsync(id, "Iltimos kutib turing malumotlaringiz tasdiqlash uchun yuborilgan!", replyMarkup: new ReplyKeyboardRemove());
        else
        {
            await botClient.SendTextMessageAsync(id, "Qaytadan malumotlaringizni kiriting!", replyMarkup: new ReplyKeyboardRemove());
            student.State = State.ForFullName.ToString();
            student.Name = null;
            student.PhoneNumber = null;
            student.Reason = null;
            student.Created = DateTime.Now;
            await dbContext.SaveChangesAsync();
            await botClient.SendTextMessageAsync(id, "Ism-Familiyangizni kiriting:", replyMarkup: new ReplyKeyboardRemove());
        }
    }

    public static async Task CreateUserAsync(ITelegramBotClient botClient, global::Telegram.Bot.Types.Update update, AppDbContext dbContext)
    {
        var id = update.Message!.Chat.Id.ToString();
        var result = await dbContext.Students.AddAsync(new Student()
        {
            Id = id,
            Created = DateTime.Now,
            IsConfirmed = false,
            State = State.ForFullName.ToString(),
            IsRightInformation = false
        });
        dbContext.SaveChanges();
        await botClient.SendTextMessageAsync(id, "Ism-Familiyangizni kiriting:");
    }

    public static async Task ForFullNameAsync(ITelegramBotClient botClient, global::Telegram.Bot.Types.Update update, AppDbContext dbContext)
    {

        ChatId chatId = update.Message!.Chat.Id;
        var student = await dbContext.Students.FirstOrDefaultAsync(x => x.Id == chatId.ToString());
        student!.Name = update.Message.Text;
        student!.State = State.ForPhoneNumber.ToString();
        dbContext.Students.Update(student);
        await dbContext.SaveChangesAsync();
        await botClient.SendTextMessageAsync(chatId, "Phone Number");
    }

    public static async Task ForPhoneNumberAsync(ITelegramBotClient botClient, global::Telegram.Bot.Types.Update update, AppDbContext dbContext)
    {

        ChatId chatId = update.Message!.Chat.Id;
        var student = await dbContext.Students.FirstOrDefaultAsync(x => x.Id == chatId.ToString());
        student!.PhoneNumber = update.Message.Text;
        student!.State = State.ForReason.ToString();
        dbContext.Students.Update(student);
        await dbContext.SaveChangesAsync();
        await botClient.SendTextMessageAsync(chatId, "reason");
    }

    public static async Task ForReasonAsync(ITelegramBotClient botClient, global::Telegram.Bot.Types.Update update, AppDbContext dbContext)
    {
        ChatId chatId = update.Message!.Chat.Id;
        var student = await dbContext.Students.FirstOrDefaultAsync(x => x.Id == chatId.ToString());
        student!.Reason = update.Message!.Text;
        student!.State = State.ForIsRightInformation.ToString();
        dbContext.Students.Update(student);
        await dbContext.SaveChangesAsync();
        var markup = new InlineKeyboardMarkup(
                 new InlineKeyboardButton[][]
                 {
                new InlineKeyboardButton[]
                {
                    InlineKeyboardButton
                        .WithCallbackData(text: "Tasdiqlash!", true.ToString()),

                    InlineKeyboardButton
                        .WithCallbackData(text: "Bekor Qilish!", callbackData: false.ToString())
                }});
        string result = $"Malumotlaringizni tog'riligini tekshiring!\n\n  " +
            $"Ism-Familiya: {student.Name}\n Telefon Nomer: {student.PhoneNumber}\n " +
            $"Kelishdan Maqsad:\n{student.Reason}";
        await botClient.SendTextMessageAsync(chatId, result, replyMarkup: markup);
    }

    public static async Task ForIsRightInformation(ITelegramBotClient botClient, global::Telegram.Bot.Types.Update update, AppDbContext dbContext)
    {
        ChatId chatId = update.CallbackQuery!.From.Id;
        var student = await dbContext.Students.FirstOrDefaultAsync(x => x.Id == chatId.ToString());

        if (update.CallbackQuery!.Data == "True")
        {
            student!.IsRightInformation = true;
            student!.State = State.ForIsRightInformation.ToString();
            dbContext.Students.Update(student);
            await dbContext.SaveChangesAsync();
            await botClient.SendTextMessageAsync(chatId, "Sizning malumotingiz yuborildi\nIltimos bir oz kuting", replyMarkup: new ReplyKeyboardRemove());
            var markup = new InlineKeyboardMarkup(
                   new InlineKeyboardButton[][]
                   {
                new InlineKeyboardButton[]
                {
                    InlineKeyboardButton
                        .WithCallbackData(text: "Tasdiqlash!",(chatId+ true.ToString())),

                    InlineKeyboardButton
                        .WithCallbackData(text: "Bekor Qilish!",(chatId+ false.ToString()))
                }});
            string result = $"Pastdagi Ma'lumotlarga ega student kirish uchun so'rov yubordi!\n\n  " +
                $"Ism-Familiya: {student.Name}\n Telefon Nomer: {student.PhoneNumber}\n " +
                $"Kelishdan Maqsad:\n{student.Reason}";
            await botClient.SendTextMessageAsync(ConfigurationService._configuration!["Admin"]!, result, replyMarkup: markup);
        }
        else
        {
            await botClient.SendTextMessageAsync(chatId, "Qaytadan malumotlaringizni kiriting!");
            student!.State = State.ForFullName.ToString();
            student.Name = null;
            student.PhoneNumber = null;
            student.Reason = null;
            student.Created = DateTime.Now;
            await dbContext.SaveChangesAsync();
            await botClient.SendTextMessageAsync(chatId, "Ism-Familiyangizni kiriting:");
        }
    }

    public static async Task ForAdminConfirmedAsync(ITelegramBotClient botClient, global::Telegram.Bot.Types.Update update, AppDbContext dbContext)
    {
        ChatId chatId = update.CallbackQuery!.From.Id;
        string studentId = update.CallbackQuery.Data!.ToString().Substring(0, 10);
        string IsTrue = update.CallbackQuery.Data.ToString().Substring(10);
        var student = await dbContext.Students.FirstOrDefaultAsync(x => x.Id == studentId);
        if (IsTrue == "True")
        {
            student!.IsConfirmed = true;
            student!.State = State.ForConfirmed.ToString();
            student.Limit = int.Parse(ConfigurationService._configuration!["Limit"]!);
            dbContext.Students.Update(student);
            await dbContext.SaveChangesAsync();
            ReplyKeyboardMarkup reply = new(
                                    new[]
                                    {
                                        new KeyboardButton[]{"QR Code olish"}
                                    })
            { ResizeKeyboard = true };
            await botClient.SendTextMessageAsync(studentId!, "Malumotlaringiz muvafaqiyatli tasdiqlandi endi siz QR Code olishingiz mumkun!", replyMarkup: reply);
            await botClient.SendTextMessageAsync(ConfigurationService._configuration!["Admin"]!, $"{student.Name} \n\n Mufaqqiyatli  tasdiqlandi!");
        }
        else
        {
            dbContext.Students.Remove(student!);
            await dbContext.SaveChangesAsync();
            await botClient.SendTextMessageAsync(studentId!, "Malumotlaringiz Tasdiqlanmadi!\n @Izzatillo_Ubaydullayev ga murojat qiling!");
            await botClient.SendTextMessageAsync(ConfigurationService._configuration!["Admin"]!, $"{student!.Name} \n\n Tasdiqlanmadi!");
        }

    }
    public static async Task CreateQRCodeAsync(ITelegramBotClient botClient, global::Telegram.Bot.Types.Update update, AppDbContext dbContext)
    {

        ChatId chatId = update.Message!.Chat.Id;
        ReplyKeyboardMarkup reply = new(
                       new[]
                       {
                                        new KeyboardButton[]{"QR Code olish"}
                       })
        { ResizeKeyboard = true };
        if (update!.Message.Text == "QR Code olish")
        {
            var student = await dbContext.Students.FirstOrDefaultAsync(x => x.Id == chatId.ToString());

            if (student!.Limit > 0)
            {
                string path = Guid.NewGuid().ToString() + ".npg";
                QRCodeGenerator qRCodeGenerator = new();
                QRCodeData qRCodeData = qRCodeGenerator.CreateQrCode($"https://t.me/{Guid.NewGuid()}", QRCodeGenerator.ECCLevel.Q);
                QRCode qrCode = new(qRCodeData);
                Bitmap bitmap = qrCode.GetGraphic(20, System.Drawing.Color.DarkRed, System.Drawing.Color.BurlyWood, true);
                bitmap.Save(path);
                using (Stream stream = System.IO.File.OpenRead(path))

                {
                    await botClient.SendPhotoAsync(chatId: chatId, photo: InputFile.FromStream(stream), caption: "Quyidagi QR Code 12 soat davomida amal qiladi");

                }
                student!.Limit = student!.Limit - 1;
                if (student!.Limit == 0)
                {
                    await botClient.SendTextMessageAsync(chatId, text: $"Sizning limitingiz tugadi yana limit olish uchun start bosib malumotlaringizni qayta yuboring", replyMarkup: new ReplyKeyboardRemove());
                    student.State = State.ForFullName.ToString();
                    student.IsConfirmed = false;
                    student.IsRightInformation = false;
                    dbContext.Students.Update(student);
                }
                else
                    await botClient.SendTextMessageAsync(chatId, text: $"Sizning limitingiz {student.Limit} ta qoldi", replyMarkup: reply);
                await dbContext.SaveChangesAsync();
                System.IO.File.Delete(path);

            }
            else
            {
                await botClient.SendTextMessageAsync(chatId, text: $"sizning limitingiz tugagan yana limit olish uchun start bosing!", replyMarkup: reply);

            }
        }
        else
        {
            await botClient.SendTextMessageAsync(chatId, text: "QR Code olish uchun Buttonni bosing!", replyMarkup: reply);
        }
    }
}

