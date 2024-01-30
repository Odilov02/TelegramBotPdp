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
                await botClient.SendTextMessageAsync(id, "Iltimos, kutib turing ma'lumotlaringiz tasdiqlash uchun yuborilgan!", replyMarkup: new ReplyKeyboardRemove());
        else
        {
            await botClient.SendTextMessageAsync(id, "Qaytadan ma'lumotlaringizni kiriting!", replyMarkup: new ReplyKeyboardRemove());
            student.State = State.ForFirstName.ToString();
            student.FirstName = null;
            student.LastName = null;
            student.PhoneNumber = null;
            student.Reason = null;
            await dbContext.SaveChangesAsync();
            await botClient.SendTextMessageAsync(id, "Ismingizni kiriting:", replyMarkup: new ReplyKeyboardRemove());
        }
    }

    public static async Task CreateUserAsync(ITelegramBotClient botClient, global::Telegram.Bot.Types.Update update, AppDbContext dbContext)
    {
        var id = update.Message!.Chat.Id.ToString();
        var result = await dbContext.Students.AddAsync(new Student()
        {
            Id = id,
            IsConfirmed = false,
            State = State.ForFirstName.ToString(),
            IsRightInformation = false
        });
        dbContext.SaveChanges();
        await botClient.SendTextMessageAsync(id, "Ismingizni kiriting:");
    }

    public static async Task ForFirstNameAsync(ITelegramBotClient botClient, global::Telegram.Bot.Types.Update update, AppDbContext dbContext)
    {

        ChatId chatId = update.Message!.Chat.Id;
        var student = await dbContext.Students.FirstOrDefaultAsync(x => x.Id == chatId.ToString());
        student!.FirstName = update.Message.Text;
        student!.State = State.ForLastName.ToString();
        dbContext.Students.Update(student);
        await dbContext.SaveChangesAsync();
        await botClient.SendTextMessageAsync(chatId, "Familiyangizni kiriting:");
    }
       public static async Task ForLastNameAsync(ITelegramBotClient botClient, global::Telegram.Bot.Types.Update update, AppDbContext dbContext)
    {

        ChatId chatId = update.Message!.Chat.Id;
        var student = await dbContext.Students.FirstOrDefaultAsync(x => x.Id == chatId.ToString());
        student!.LastName = update.Message.Text;
        student!.State = State.ForPhoneNumber.ToString();
        dbContext.Students.Update(student);
        await dbContext.SaveChangesAsync();
        await botClient.SendTextMessageAsync(chatId, "PDP Academy saytidan ro'yxatdan o'tgan telefon nomeringizni kiriting\nExemple: 912345678");
    }

    public static async Task ForPhoneNumberAsync(ITelegramBotClient botClient, global::Telegram.Bot.Types.Update update, AppDbContext dbContext)
    {
        bool isNum = true;
        foreach (char ch in update.Message!.Text!)
        {
            if (!char.IsDigit(ch))
            {
                isNum = false;
            }
        }
            ChatId chatId = update.Message!.Chat.Id;
        if (isNum)
        {
            var student = await dbContext.Students.FirstOrDefaultAsync(x => x.Id == chatId.ToString());
            student!.PhoneNumber = update.Message.Text;
            student!.State = State.ForReason.ToString();
            dbContext.Students.Update(student);
            await dbContext.SaveChangesAsync();
            await botClient.SendTextMessageAsync(chatId, "PDP Academy binosiga kelishdan maqsadingiz:");
        }
        else
        {
            await botClient.SendTextMessageAsync(chatId, "Telefon nomerni noto'g'ri formatda kiritdingiz!\nIltimos qayta kiriting:");
           
        }
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
            $"Ism: {student.FirstName}\nFamiliya: {student.LastName}\nTelefon Nomer: {student.PhoneNumber}\n " +
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
            await botClient.SendTextMessageAsync(chatId, "Ma'lumotlaringiz yuborildi.\nIltimos bir oz kuting!", replyMarkup: new ReplyKeyboardRemove());
            var markup = new InlineKeyboardMarkup(
                   new InlineKeyboardButton[][]
                   {
                new InlineKeyboardButton[]
                {
                    InlineKeyboardButton
                        .WithCallbackData(text: "Tasdiqlash!",(chatId+ 0.ToString())),

                    InlineKeyboardButton
                        .WithCallbackData(text: "Bekor Qilish!",(chatId+ 1.ToString()))
                }});
            string result = $"Quyidagi ma'lumotlarga ega student kirish uchun so'rov yubordi:\n\n  " +
                $"Ism: {student.FirstName}\nFamiliya: {student.LastName}\nTelefon Nomer: {student.PhoneNumber}\n " +
                $"Kelishdan Maqsad:\n{student.Reason}";
            await botClient.SendTextMessageAsync(ConfigurationService._configuration!["Admin"]!, result, replyMarkup: markup);
        }
        else
        {
            await botClient.SendTextMessageAsync(chatId, "Qaytadan ma'lumotlaringizni kiriting!");
            student!.State = State.ForFirstName.ToString();
            student.FirstName = null;
            student.LastName = null;
            student.PhoneNumber = null;
            student.Reason = null;
            await dbContext.SaveChangesAsync();
            await botClient.SendTextMessageAsync(chatId, "Ismingizni kiriting:");
        }
    }

    public static async Task ForAdminConfirmedAsync(ITelegramBotClient botClient, global::Telegram.Bot.Types.Update update, AppDbContext dbContext)
    {
        ChatId chatId = update.CallbackQuery!.From.Id;
        string data = update.CallbackQuery.Data!.ToString();
        string studentId=data.Substring(0, data.Length - 1);
        string IsTrue = data.Substring(data.Length-1);
        var student = await dbContext.Students.FirstOrDefaultAsync(x => x.Id == studentId);
        if (IsTrue == "0")
        {
            student!.IsConfirmed = true;
            student!.State = State.ForConfirmed.ToString();
            student.CreatedQRCode=DateTime.Now.AddDays(-1);
            student.Limit = int.Parse(ConfigurationService._configuration!["Limit"]!);
            dbContext.Students.Update(student);
            await dbContext.SaveChangesAsync();
            ReplyKeyboardMarkup reply = new(
                                    new[]
                                    {
                                        new KeyboardButton[]{"QR Code olish"}
                                    })
            { ResizeKeyboard = true };
            await botClient.SendTextMessageAsync(studentId!, "Ma'lumotlaringiz muvaffaqiyatli tasdiqlandi. Endi siz QR Code olishingiz mumkun!", replyMarkup: reply);
            await botClient.SendTextMessageAsync(ConfigurationService._configuration!["Admin"]!, $"ism: {student.FirstName}\nFamiliya: {student.LastName} \n\nMuvaffaqiyatli  tasdiqlandi!");
        }
        else
        {
            dbContext.Students.Remove(student!);
            await dbContext.SaveChangesAsync();
            await botClient.SendTextMessageAsync(studentId!, "Ma'lumotlaringiz Tasdiqlanmadi!\n @Izzatillo_Ubaydullayev ga murojat qiling!");
            await botClient.SendTextMessageAsync(ConfigurationService._configuration!["Admin"]!, $"Ism:{student!.FirstName}\nFamiliya: {student.LastName} \n\nTasdiqlanmadi!");
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

                if (student.CreatedQRCode.Year<DateTime.Now.Year
                  |  student.CreatedQRCode.Month < DateTime.Now.Month
                  | student.CreatedQRCode.Day< DateTime.Now.Day)
                {
                    var data = await QRCodeDataApi.GetData("", "");
                    string path = Guid.NewGuid().ToString() + ".npg";
                    QRCodeGenerator qRCodeGenerator = new();
                    QRCodeData qRCodeData = qRCodeGenerator.CreateQrCode(data, QRCodeGenerator.ECCLevel.Q);
                    QRCode qrCode = new(qRCodeData);
                    Bitmap bitmap = qrCode.GetGraphic(20, System.Drawing.Color.DarkRed, System.Drawing.Color.BurlyWood, true);
                    bitmap.Save(path);
                    using (Stream stream = System.IO.File.OpenRead(path))
                    {
                        await botClient.SendPhotoAsync(chatId: chatId, photo: InputFile.FromStream(stream), caption: "Quyidagi QR Code shu kun davomida amal qiladi");
                        await botClient.SendTextMessageAsync(ConfigurationService._configuration!["Admin"]!, $"Ism: {student.FirstName}\nFamiliya: {student.LastName}\nTelefon Nomer: {student.PhoneNumber}\nLimit: {student.Limit - 1}");
                    }
                    student!.Limit = student!.Limit - 1;
                    student.CreatedQRCode = DateTime.Now;
                    if (student!.Limit == 0)
                    {
                        await botClient.SendTextMessageAsync(chatId, text: $"Sizning limitingiz tugadi yana limit olish uchun start bosib ma'lumotlaringizni qayta yuboring", replyMarkup: new ReplyKeyboardRemove());

                        dbContext.Students.Remove(student);
                    }
                    else
                        await botClient.SendTextMessageAsync(chatId, text: $"Sizning limitingiz {student.Limit} ta qoldi", replyMarkup: reply);
                    await dbContext.SaveChangesAsync();
                    System.IO.File.Delete(path);
                }
                else
                        await botClient.SendTextMessageAsync(chatId, text: "Bir kun davomida 1 ta QR Code olishingiz mumkun", replyMarkup: reply);
            }
            else
            {
                await botClient.SendTextMessageAsync(chatId, text: $"Sizning limitingiz tugagan yana limit olish uchun start bosing!", replyMarkup: reply);

            }
        }
        else
        {
            await botClient.SendTextMessageAsync(chatId, text: "QR Code olish uchun Buttonni bosing!", replyMarkup: reply);
        }
    }
}

