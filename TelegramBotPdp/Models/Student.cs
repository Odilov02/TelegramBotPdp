namespace TelegramBotPdp.Models;

public class Student
{
    public string Id { get; set; }
    public string? FirstName { get; set; }
    public string? LastName { get; set; }
    public string? PhoneNumber { get; set; }
    public string? Reason { get; set; }
    public DateTime CreatedQRCode { get; set; }
    public bool IsConfirmed { get; set; }=false;
    public bool IsRightInformation { get; set; }=false;
    public int Limit { get; set; }
    public string? State { get; set; }
}
