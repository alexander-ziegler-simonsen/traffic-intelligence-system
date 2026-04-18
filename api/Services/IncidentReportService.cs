using MongoDB.Driver;
using TisApi.Data.Mongo;
using TisApi.Data.Mongo.Documents;
using TisApi.DTOs;
using TisApi.Mappers;
using TisApi.Services.Interfaces;

namespace TisApi.Services;

public class IncidentReportService(TisMongoContext mongo, IncidentReportMapper mapper) : IIncidentReportService
{
    public async Task<IReadOnlyList<IncidentReportDto>> GetAllAsync(
        int? minSeverity = null,
        string? roadName = null,
        string? cameraLabel = null)
    {
        var filter = Builders<IncidentReport>.Filter.Empty;

        if (minSeverity.HasValue)
            filter &= Builders<IncidentReport>.Filter.Gte(r => r.Severity, minSeverity.Value);
        if (!string.IsNullOrEmpty(roadName))
            filter &= Builders<IncidentReport>.Filter.Eq(r => r.Road.Name, roadName);
        if (!string.IsNullOrEmpty(cameraLabel))
            filter &= Builders<IncidentReport>.Filter.Eq(r => r.Camera.Label, cameraLabel);

        var docs = await mongo.IncidentReports
            .Find(filter)
            .SortByDescending(r => r.RecordedAt)
            .ToListAsync();

        return docs.Select(mapper.ToDto).ToList();
    }

    public async Task<IncidentReportDto?> GetByIdAsync(string id)
    {
        var filter = Builders<IncidentReport>.Filter.Eq(r => r.Id, id);
        var doc = await mongo.IncidentReports.Find(filter).FirstOrDefaultAsync();
        return doc is null ? null : mapper.ToDto(doc);
    }
}
