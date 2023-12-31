using System.Globalization;
using CloakCare.Web.Data;
using CloakCare.Web.Data.Models;
using Microsoft.AspNetCore.Components;
using Microsoft.JSInterop;
using MudBlazor;

namespace CloakCare.Web.Pages.Components;

public partial class Agenda : ComponentBase, IDisposable
{
    private CancellationTokenSource? _cts;
    [Inject] protected ISnackbar Snackbar { get; set; } = default!;
    [Inject] private IDialogService DialogService { get; set; } = default!;
    [Inject] private DataService DataService { get; set; } = default!;
    [Inject] private IJSRuntime Js { get; set; } = default!;

    private string _searchString = "";
    private Appointment _selectedItem = null!;
    private Appointment _appointmentBeforeEdit = default!;
    private List<Appointment> _appointments = new();
    private TimeSpan? _editTime;
    private DateTime? _editDate;
    private bool _loading;

    private MudDatePicker _datePicker = default!;
    private MudTimePicker _timePicker = default!;

    protected override async Task OnInitializedAsync()
    {
        _cts = new CancellationTokenSource();
        _loading = true;
        _appointments = (await DataService.GetAppointmentsAsync(_cts.Token))
            .Where(x => x.DateTime >= DateTime.Now).ToList();
        _loading = false;
    }

    private async Task RemoveAppointment(Appointment appointment)
    {
        var toDelete = await DialogService.ShowMessageBox(
            "Afspraak verwijderen",
            "Weet je zeker dat je de afspraak wilt verwijderen?",
            yesText: "Ja", cancelText: "Nee");

        if (!toDelete ?? true)
        {
            return;
        }

        _loading = true;
        await DataService.RemoveAppointmentAsync(appointment);
        _appointments.Remove(appointment);
        StateHasChanged();
        _loading = false;
        Snackbar.Add("Afspraak verwijderd", Severity.Info);
    }

    private async Task AddAppointment()
    {
        var options = new DialogOptions { CloseOnEscapeKey = true };
        var dialog = await DialogService.ShowAsync<AppointmentForm>("Nieuwe afspraak", options);
        var result = await dialog.Result;

        if (!result.Canceled)
        {
            await DataService.AddAppointmentAsync((Appointment)result.Data);
            _appointments.Add((Appointment)result.Data);
            StateHasChanged();
            Snackbar.Add("Afspraak opgeslagen", Severity.Info);
        }
    }
    
    private void BackupItem(object appointment)
    {
        _editDate = ((Appointment)appointment).DateTime;
        _editTime = ((Appointment)appointment).DateTime.TimeOfDay;
        _appointmentBeforeEdit = new Appointment((Appointment)appointment);
    }

    private async void RowEditCommitted(object appointment)
    {
        _loading = true;
        ((Appointment)appointment).DateTime = _editDate!.Value.Add(_editTime!.Value);
        await DataService.EditAppointAsync((Appointment)appointment);
        _loading = false;
        StateHasChanged();
        Snackbar.Add("Afspraak gewijzigd", Severity.Info);
    }

    private void ResetItemToOriginalValues(object appointment)
    {
        ((Appointment)appointment).Update(_appointmentBeforeEdit);
    }

    private bool FilterFunc(Appointment appointment)
    {
        if (string.IsNullOrWhiteSpace(_searchString))
            return true;
        if (appointment.DateTime.Date.ToString(CultureInfo.InvariantCulture)
            .Contains(_searchString, StringComparison.OrdinalIgnoreCase))
            return true;
        if (appointment.DateTime.TimeOfDay.ToString()
            .Contains(_searchString, StringComparison.OrdinalIgnoreCase))
            return true;
        if (appointment.Name.Contains(_searchString, StringComparison.OrdinalIgnoreCase))
            return true;
        if (appointment.Location.Contains(_searchString, StringComparison.OrdinalIgnoreCase))
            return true;
        if (appointment.Companion.Contains(_searchString, StringComparison.OrdinalIgnoreCase))
            return true;
        if ($"{appointment.DateTime.Date} {appointment.DateTime.TimeOfDay} {appointment.Name} {appointment.Location} {appointment.Companion} "
            .Contains(_searchString))
            return true;

        return false;
    }

    public void Dispose()
    {
        _cts?.Dispose();
    }

    private async Task DownloadIcs(Appointment appointment)
    {
        var icsStream = appointment.CreateIcsStream();
        var fileName = $"{appointment.Name}.ics";

        using var streamRef = new DotNetStreamReference(stream: icsStream);

        await Js.InvokeVoidAsync("downloadFileFromStream", fileName, streamRef);
    }
}