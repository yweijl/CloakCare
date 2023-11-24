namespace CloakCare.Web.Data;

public class CosmosSettings
{
    public required string DbName { get; set; } 
    public required string Endpoint { get; set; }
    public required string Container { get; set; }
}