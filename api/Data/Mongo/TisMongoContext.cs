using MongoDB.Driver;
using TisApi.Data.Mongo.Documents;

namespace TisApi.Data.Mongo;

public class TisMongoContext
{
    private readonly IMongoDatabase _db;

    public TisMongoContext(IMongoClient client, IConfiguration configuration)
    {
        var dbName = configuration["Mongo:Database"] ?? "tis_read";
        _db = client.GetDatabase(dbName);
    }

    public IMongoCollection<IncidentReport> IncidentReports =>
        _db.GetCollection<IncidentReport>("incident_reports");
}
