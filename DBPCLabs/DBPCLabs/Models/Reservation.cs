using System.ComponentModel.DataAnnotations;

namespace DBPCLabs.Models
{
    public class Reservation
    {
        public int Id { get; set; }

        public bool IsGroupReservation { get; set; } = false;

        public int? ComputerId { get; set; }
        public string? ComputerName { get; set; } 
        public int? StudentId { get; set; }
        public string? StudentName { get; set; }
        
        public int? LaboratoryId { get; set; }
        public string? LaboratoryName { get; set; }
        public int? GroupId { get; set; }
        public string? GroupName { get; set; }
        
        public int? TeacherId { get; set; }
        public string? TeacherName { get; set; }

        public DateTime StartTime { get; set; } = DateTime.Today.AddHours(9);
        public DateTime EndTime { get; set; } = DateTime.Today.AddHours(10).AddMinutes(30);

        [StringLength(200, ErrorMessage = "Опис не може перевищувати 200 символів")]
        public string Purpose { get; set; } = string.Empty;
    }
}