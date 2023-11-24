using Azure.Identity;
using Microsoft.Azure.Cosmos;
using Microsoft.Azure.Cosmos.Linq;
using Microsoft.Extensions.Options;

namespace CloakCare.Web.Data;

public class DataService
{
    private readonly Container _container;

    public DataService(IOptions<CosmosSettings> settings)
    {
       var client = new CosmosClient(settings.Value.Endpoint, new DefaultAzureCredential());
       _container = client.GetDatabase(settings.Value.DbName).GetContainer(settings.Value.Container);
    }

    public async Task<IEnumerable<Appointment>> GetAppointmentsAsync()
    {
        try
        {
            var queryable = _container.GetItemLinqQueryable<Appointment>();
            using var feed = queryable
                .OrderByDescending(x => x.DateTime)
                .ToFeedIterator();

            List<Appointment> results = new();

            while (feed.HasMoreResults)
            {
                var response = await feed.ReadNextAsync();
                results.AddRange(response);
            }
            return results;

        }
        catch (CosmosException e)
        {
            Console.WriteLine(e);
            throw;
        }

    }
}