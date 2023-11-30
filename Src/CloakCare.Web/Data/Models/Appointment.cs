using System.Text;

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

    public Stream CreateIcsStream()
    {
        return new MemoryStream(Encoding.UTF8.GetBytes(GenerateIcs()));
    }

    private string GenerateIcs()
    {
            var stringBuilder = new StringBuilder();
            stringBuilder.AppendLine("BEGIN:VCALENDAR");
            stringBuilder.AppendLine("VERSION:1.0");
            stringBuilder.AppendLine("PRODID:-//CloakCare//Calendar 1.0//EN");
            stringBuilder.AppendLine("BEGIN:VEVENT");
            stringBuilder.AppendLine($"SUMMARY:{Name}");
            stringBuilder.AppendLine($"LOCATION:{Location}");
            stringBuilder.AppendLine($"DTEND:{DateTime.AddHours(1).ToString("yyyyMMddTHHmmss")}");
            stringBuilder.AppendLine($"DTSTART:{DateTime.ToString("yyyyMMddTHHmmss")}");
            stringBuilder.AppendLine($"DTSTAMP:{DateTime.Now.ToString("yyyyMMddTHHmmss")}");
            stringBuilder.AppendLine($"UID:{Guid.NewGuid()}");
            stringBuilder.AppendLine("END:VEVENT");
            stringBuilder.AppendLine("END:VCALENDAR");
            return stringBuilder.ToString();
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