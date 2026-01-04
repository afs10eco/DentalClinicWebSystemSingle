using System.ComponentModel.DataAnnotations;

namespace DentalClinic.Web.Models;

public class Treatment
{
    public int Id { get; set; }

    [Required, StringLength(120)]
    public string Name { get; set; } = string.Empty;

    [Range(0, 100000)]
    public decimal Price { get; set; }

    [Range(5, 600)]
    public int DurationMinutes { get; set; } = 30;

    [StringLength(400)]
    public string? Description { get; set; }

    public ICollection<Appointment> Appointments { get; set; } = new List<Appointment>();
}
