namespace TelegramBotPdp.Models;

public class Student
{
    public string Id { get; set; }
    public string Name { get; set; }
    public string PhoneNumber { get; set; }
    public string Reason { get; set; }
    public DateTime Created { get; set; } = DateTime.Now;
    public bool IsConfirmed { get; set; }
    public bool IsRightInformation { get; set; }
    public string State { get; set; }
}
