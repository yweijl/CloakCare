using CloakCare.Web.Data;
using CloakCare.Web.Data.Models;
using Microsoft.AspNetCore.Components;
using Microsoft.JSInterop;
using MudBlazor;

namespace CloakCare.Web.Pages.Components;

public partial class MobileAgenda : ComponentBase, IDisposable
{
    private bool _isEditing = false;
    private CancellationTokenSource? _cts;
    [Inject] protected ISnackbar Snackbar { get; set; } = default!;
    [Inject] private IDialogService DialogService { get; set; } = default!;
    [Inject] private DataService DataService { get; set; } = default!;
    [Inject] private IJSRuntime Js { get; set; } = default!;

    private List<Appointment> _appointments = new();
    private bool _loading;


    protected override async Task OnInitializedAsync()
    {
        _cts = new CancellationTokenSource();
        _loading = true;
        _appointments = (await DataService.GetAppointmentsAsync(_cts.Token))
            .Where(x => x.DateTime >= DateTime.Now).OrderBy(x => x.DateTime).ToList();
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
    
    private async void EditAppointment(Appointment appointment)
    {
        var parameters =
            new DialogParameters<AppointmentForm> { { x => x.EditAppointment, appointment } };
        
        var dialog = await DialogService.ShowAsync<AppointmentForm>("Nieuwe afspraak", parameters);
        var result = await dialog.Result;

        if (!result.Canceled)
        {
            appointment.Update((Appointment)result.Data); 
            await DataService.EditAppointAsync(appointment);
            StateHasChanged();
            Snackbar.Add("Afspraak gewijzigd", Severity.Info);
        }
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