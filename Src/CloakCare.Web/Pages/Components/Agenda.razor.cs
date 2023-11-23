using System.Globalization;
using CloakCare.Web.Data;
using Microsoft.AspNetCore.Components;
using MudBlazor;

namespace CloakCare.Web.Pages.Components;

public partial class Agenda : ComponentBase
{
    [Inject]
    private IDialogService DialogService { get; set; } = default!;
    
    private List<string> _editEvents = new();
    private string _searchString = "";
    private Appointment _selectedItem = null!;
    private Appointment _appointmentBeforeEdit = default!;
    private List<Appointment> _appointments = new ();
    private TimeSpan? _editTime;
    private DateTime? _editDate;


    protected override async Task OnInitializedAsync()
    {
        // Elements = await httpClient.GetFromJsonAsync<List<Element>>("webapi/periodictable");
        for (var i = 0; i < 100; i++)
        {
            _appointments.Add(new Appointment()
            {
                DateTime = DateTime.Now.AddDays(i),
                Name = $"Doctor {i}",
                Location = $"Location {i}",
            });
        }
    }

    private async Task RemoveAppointment(Appointment appointment)
    {
        Snackbar.Add(appointment.Name);
        _appointments.Remove(appointment);
        StateHasChanged();
    }
    
    private async Task AddAppointment()
    {
        var options = new DialogOptions { CloseOnEscapeKey = true };
        var dialog = await DialogService.ShowAsync<NewAppointment>("Nieuwe afspraak", options);
        var result = await dialog.Result;

        if (!result.Canceled)
        {
            _appointments.Add((Appointment)result.Data);
        }

    }

    private void ClearEventLog()
    {
        _editEvents.Clear();
    }

    private void AddEditionEvent(string message)
    {
        _editEvents.Add(message);
        StateHasChanged();
    }

    private void BackupItem(object appointment)
    {
        _editDate = ((Appointment)appointment).DateTime;
        _editTime = ((Appointment)appointment).DateTime.TimeOfDay;
        
        _appointmentBeforeEdit = new Appointment
        {
                DateTime = ((Appointment)appointment).DateTime,
                Name = ((Appointment)appointment).Name,
                Location = ((Appointment)appointment).Location
            };
        AddEditionEvent($"RowEditPreview event: made a backup of Appointment {((Appointment)appointment).Name}");
    }

    private void ItemHasBeenCommitted(object appointment)
    {
        ((Appointment)appointment).DateTime = _editDate!.Value.Add(_editTime!.Value);
        AddEditionEvent($"RowEditCommit event: Changes to Appointment {((Appointment)appointment).Name} committed");
    }

    private void ResetItemToOriginalValues(object appointment)
    {
        ((Appointment)appointment).DateTime = _appointmentBeforeEdit.DateTime;
        ((Appointment)appointment).Name = _appointmentBeforeEdit.Name;
        ((Appointment)appointment).Location = _appointmentBeforeEdit.Location;
        AddEditionEvent(
            $"RowEditCancel event: Editing of Appointment {((Appointment)appointment).Name} canceled");
    }

    private bool FilterFunc(Appointment appointment)
    {
        if (string.IsNullOrWhiteSpace(_searchString))
            return true;
        if (appointment.DateTime.ToString(CultureInfo.InvariantCulture).Contains(_searchString, StringComparison.OrdinalIgnoreCase))
            return true;
        if (appointment.Name.Contains(_searchString, StringComparison.OrdinalIgnoreCase))
            return true;
        if (appointment.Location.Contains(_searchString, StringComparison.OrdinalIgnoreCase))
            return true;
        if (appointment.Companion.Contains(_searchString, StringComparison.OrdinalIgnoreCase))
            return true;
        if ($"{appointment.DateTime} {appointment.Name} {appointment.Location} {appointment.Companion} ".Contains(_searchString))
            return true;
        return false;
    }
}
    