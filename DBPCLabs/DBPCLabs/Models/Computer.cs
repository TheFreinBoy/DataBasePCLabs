namespace DBPCLabs.Models;

public class Computer : BaseEntity
{
    public string InventoryNumber { get; set; } = string.Empty;
    public string Cpu { get; set; } = string.Empty;
    public int RamGb { get; set; }
    
    public int LaboratoryId { get; set; }
    public Laboratory? Laboratory { get; set; } 

    public List<Software> InstalledSoftware { get; set; } = new();
    public List<Reservation> Reservations { get; set; } = new();
}
