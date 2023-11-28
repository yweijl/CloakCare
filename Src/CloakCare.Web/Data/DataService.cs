using Azure.Identity;
using CloakCare.Web.Data.Models;
using Microsoft.Azure.Cosmos;
using Microsoft.Extensions.Options;

namespace CloakCare.Web.Data;

public class DataService
{
    private readonly ILogger<DataService> _logger;
    private readonly Container _container;
    private readonly string _patientId;
    private readonly PartitionKey _partitionKey;
    private Patient? _patient;

    public DataService(IOptions<CosmosSettings> settings, ILogger<DataService> logger)
    {
        _logger = logger;
        var client = new CosmosClient(settings.Value.Endpoint, new DefaultAzureCredential());
        _container = client.GetDatabase(settings.Value.DbName)
            .GetContainer(settings.Value.Container);
        _patientId = settings.Value.PatientId;
        _partitionKey = new PartitionKey(_patientId);
    }

    public async Task<List<Appointment>> GetAppointmentsAsync(CancellationToken cancellationToken)
    {
        var result =
            await _container.ReadItemAsync<Patient>(_patientId,
                _partitionKey, cancellationToken: cancellationToken);

        _patient = result.Resource;
        return _patient.Appointments;
    }

    public async Task AddAppointmentAsync(Appointment appointment)
    {
        _patient!.Appointments.Add(appointment);
        await UpdateAsync();
    }

    public async Task EditAppointAsync(Appointment appointment)
    {
        var currentAppointment = _patient!.Appointments.First(x => x.Id == appointment.Id);
        currentAppointment.Update(appointment);
        await UpdateAsync();
    }

    public async Task RemoveAppointmentAsync(Appointment appointment)
    {
        _patient!.Appointments.Remove(appointment);
        await UpdateAsync();
    }

    private async Task UpdateAsync()
    {
        try
        {
            await _container.ReplaceItemAsync(_patient, _patientId, _partitionKey);
        }
        catch (CosmosException e)
        {
            _logger.LogError(e, "Failed to update patient: {patient}", _patient);
            throw;
        }
    }
}