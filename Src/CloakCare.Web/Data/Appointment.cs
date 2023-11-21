namespace CloakCare.Web.Data;

public class Appointment
{
    public DateTime DateTime { get; set; }
    public string Name { get; set; } = default!;
    public string Location { get; set; } = default!;
    public string Companion { get; set; } = string.Empty;
}