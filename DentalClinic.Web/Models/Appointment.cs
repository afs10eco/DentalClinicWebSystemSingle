using System.ComponentModel.DataAnnotations;

namespace DentalClinic.Web.Models;

public class Appointment
{
    public int Id { get; set; }

    [Required]
    public int PatientId { get; set; }
    public Patient? Patient { get; set; }

    [Required]
    public int DoctorId { get; set; }
    public Doctor? Doctor { get; set; }

    [Required]
    public int TreatmentId { get; set; }
    public Treatment? Treatment { get; set; }

    [Required, DataType(DataType.Date)]
    public DateTime Date { get; set; } = DateTime.Today.AddDays(1);

    [Required, DataType(DataType.Time)]
    public TimeSpan Time { get; set; } = new(10, 0, 0);

    [StringLength(500)]
    public string? Notes { get; set; }

    public bool Completed { get; set; }

    public Review? Review { get; set; }
}
