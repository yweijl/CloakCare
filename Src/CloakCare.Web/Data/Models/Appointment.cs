namespace CloakCare.Web.Data.Models;

public class Appointment
{
    public Guid Id { get; init; }
    public DateTime DateTime { get; set; }
    public string Name { get; set; } = default!;
    public string Location { get; set; } = default!;
    public string Companion { get; set; } = string.Empty;
    public bool TaxiOrdered { get; set; }

    public Appointment()
    {
    }

    public Appointment(Appointment appointment)
    {
        Update(appointment);
    }

    public void Update(Appointment appointment)
    {
        DateTime = appointment.DateTime;
        Name = appointment.Name;
        Location = appointment.Location;
        Companion = appointment.Companion;
        TaxiOrdered = appointment.TaxiOrdered;
    }
}