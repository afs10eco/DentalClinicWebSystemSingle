using System.ComponentModel.DataAnnotations;

namespace DentalClinic.Web.Models;

public class Patient
{
    public int Id { get; set; }

    [Required, StringLength(100)]
    public string FullName { get; set; } = string.Empty;

    [Required, DataType(DataType.Date)]
    public DateTime BirthDate { get; set; } = DateTime.Today.AddYears(-18);

    [Phone, StringLength(30)]
    public string? Phone { get; set; }

    [EmailAddress, StringLength(120)]
    public string? Email { get; set; }

    public ICollection<Appointment> Appointments { get; set; } = new List<Appointment>();
}
