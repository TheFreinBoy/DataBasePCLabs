namespace DBPCLabs.Models;

public class Laboratory : BaseEntity
{
    public string Name { get; set; } = string.Empty;
    public string RoomNumber { get; set; } = string.Empty;
    public int Capacity { get; set; }

    
    public List<Computer> Computers { get; set; } = new();
}