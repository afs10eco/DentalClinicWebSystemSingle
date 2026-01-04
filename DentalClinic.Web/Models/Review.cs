using System.ComponentModel.DataAnnotations;

namespace DentalClinic.Web.Models;

public class Review
{
    public int Id { get; set; }

    [Required]
    public int AppointmentId { get; set; }
    public Appointment? Appointment { get; set; }

    [Range(1, 5)]
    public int Rating { get; set; } = 5;

    [StringLength(800)]
    public string? Notes { get; set; }

    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
}
