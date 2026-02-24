namespace DBPCLabs.Models;

public class Software : BaseEntity
{
    public string Name { get; set; } = string.Empty;
    public string Version { get; set; } = string.Empty;
    public string LicenseType { get; set; } = string.Empty;

    public List<Computer> Computers { get; set; } = new();
}