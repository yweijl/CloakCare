namespace CloakCare.Web.Data.Models;

public class Patient
{
    public Guid Id { get; set; }
    public List<Appointment> Appointments { get; set; } = new();
}