using System.Globalization;
using CloakCare.Web.Data;
using CloakCare.Web.Data.Models;
using Microsoft.AspNetCore.Components;
using MudBlazor;
using MudBlazor.Services;

namespace CloakCare.Web.Pages.Components;

public partial class Agenda : ComponentBase, IDisposable
{
    private CancellationTokenSource? _cts;

    [Inject] private IDialogService DialogService { get; set; } = default!;

    [Inject] private DataService DataService { get; set; } = default!;

    [Inject] private IBrowserViewportService BreakpointListener { get; set; } = default!;


    private List<string> _editEvents = new();
    private string _searchString = "";
    private Appointment _selectedItem = null!;
    private Appointment _appointmentBeforeEdit = default!;
    private List<Appointment> _appointments = new();
    private TimeSpan? _editTime;
    private DateTime? _editDate;
    private bool _loading;
    private bool _isEditing;
    private Breakpoint _breakPoint;

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

    protected override async Task OnAfterRenderAsync(bool firstRender)
    {
        _breakPoint = await BreakpointListener.GetCurrentBreakpointAsync();
        await base.OnAfterRenderAsync(firstRender);
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
        var dialog = await DialogService.ShowAsync<NewAppointment>("Nieuwe afspraak", options);
        var result = await dialog.Result;

        if (!result.Canceled)
        {
            await DataService.AddAppointmentAsync((Appointment)result.Data);
            _appointments.Add((Appointment)result.Data);
            StateHasChanged();
            Snackbar.Add("Afspraak opgeslagen", Severity.Info);
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
        _isEditing = true;
        _appointmentBeforeEdit = new Appointment((Appointment)appointment);

        AddEditionEvent(
            $"RowEditPreview event: made a backup of Appointment {((Appointment)appointment).Name}");
    }

    private async void RowEditCommitted(object appointment)
    {
        _loading = true;
        ((Appointment)appointment).DateTime = _editDate!.Value.Add(_editTime!.Value);
        await DataService.EditAppointAsync((Appointment)appointment);
        _loading = false;
        _isEditing = false;
        StateHasChanged();
    }

    private void ResetItemToOriginalValues(object appointment)
    {
        ((Appointment)appointment).Update(_appointmentBeforeEdit);
        AddEditionEvent(
            $"RowEditCancel event: Editing of Appointment {((Appointment)appointment).Name} canceled");
        _isEditing = false;
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

    private async Task SetFocus(bool isDatePicker)
    {
        if (_breakPoint == Breakpoint.Xs)
        {
            await (isDatePicker ? _datePicker.BlurAsync() : _timePicker.BlurAsync());
        }
    }

    public void Dispose()
    {
        _cts?.Dispose();
    }
}