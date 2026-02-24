namespace DBPCLabs.Models;

public class Reservation : BaseEntity
{
    public DateTime ReservationDate { get; set; }
    public DateTime StartTime { get; set; }
    public DateTime EndTime { get; set; }

    public int ComputerId { get; set; }
    public Computer? Computer { get; set; }

    public int UserId { get; set; }
    public User? User { get; set; }
}