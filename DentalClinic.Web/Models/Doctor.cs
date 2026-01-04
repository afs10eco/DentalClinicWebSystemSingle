using System.ComponentModel.DataAnnotations;

namespace DentalClinic.Web.Models;

public class Doctor
{
    public int Id { get; set; }

    [Required, StringLength(100)]
    public string FullName { get; set; } = string.Empty;

    [Required, StringLength(80)]
    public string Specialty { get; set; } = string.Empty;

    [Phone, StringLength(30)]
    public string? Phone { get; set; }

    [EmailAddress, StringLength(120)]
    public string? Email { get; set; }

    public ICollection<Appointment> Appointments { get; set; } = new List<Appointment>();
}
