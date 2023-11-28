namespace CloakCare.Web.Data.Models;

public class Appointment
{
    public Guid Id { get; set; }
    public DateTime DateTime { get; set; }
    public string Name { get; set; } = default!;
    public string Location { get; set; } = default!;
    public string Companion { get; set; } = string.Empty;

    public void Update(Appointment appointment)
    {
        DateTime = appointment.DateTime;
        Name = appointment.Name;
        Location = appointment.Location;
        Companion = appointment.Companion;
    }
}