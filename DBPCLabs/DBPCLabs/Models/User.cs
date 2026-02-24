namespace DBPCLabs.Models;

public class User : BaseEntity
{
    public string FullName { get; set; } = string.Empty;
    public string Role { get; set; } = "Student"; // Student / Teacher
    public string TicketNumber { get; set; } = string.Empty;
}