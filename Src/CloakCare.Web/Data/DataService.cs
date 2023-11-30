using Azure.Identity;
using CloakCare.Web.Data.Models;
using Microsoft.Azure.Cosmos;
using Microsoft.Extensions.Caching.Memory;
using Microsoft.Extensions.Options;

namespace CloakCare.Web.Data;

public class DataService
{
    private readonly IMemoryCache _cache;
    private readonly ILogger<DataService> _logger;
    private readonly Container _container;
    private readonly string _patientId;
    private Patient? _patient;

    public DataService(IOptions<CosmosSettings> settings, ILogger<DataService> logger, IMemoryCache cache)
    {
        _logger = logger;
        _cache = cache;
        var client = new CosmosClient(settings.Value.Endpoint, new DefaultAzureCredential(),
            new CosmosClientOptions()
            {
                SerializerOptions = new CosmosSerializationOptions
                {
                    PropertyNamingPolicy = CosmosPropertyNamingPolicy.CamelCase,
                },
            });
        _container = client.GetDatabase(settings.Value.DbName)
            .GetContainer(settings.Value.Container);
        _patientId = settings.Value.PatientId;
    }

    public async Task<List<Appointment>> GetAppointmentsAsync(CancellationToken cancellationToken)
    {
        _patient = await _cache.GetOrCreateAsync<Patient>(_patientId, async entry =>
        {
            entry.SetSlidingExpiration(TimeSpan.FromMinutes(1));
            entry.AbsoluteExpirationRelativeToNow = TimeSpan.FromMinutes(2);
            var result =
                await _container.ReadItemAsync<Patient>(_patientId, new PartitionKey(_patientId),
                    cancellationToken: cancellationToken);
            return result.Resource;

        });
        
        return _patient!.Appointments;
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
            var result = await _container.ReplaceItemAsync(_patient, _patientId);
            _cache.Set(_patientId, result.Resource!);
        }
        catch (CosmosException e)
        {
            _logger.LogError(e, "Failed to update patient: {patient}", _patient);
            throw;
        }
    }
}