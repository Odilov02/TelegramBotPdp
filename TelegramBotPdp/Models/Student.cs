namespace TelegramBotPdp.Models;

public class Student
{
    public string Id { get; set; }
    public string? Name { get; set; }
    public string? PhoneNumber { get; set; }
    public string? Reason { get; set; }
    public DateTime Created { get; set; }
    public bool IsConfirmed { get; set; }=false;
    public bool IsRightInformation { get; set; }=false;
    public int Limit { get; set; }
    public string? State { get; set; }
}
