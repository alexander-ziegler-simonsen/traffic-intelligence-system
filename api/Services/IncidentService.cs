using Microsoft.EntityFrameworkCore;
using TisApi.Data.Postgres;
using TisApi.DTOs;
using TisApi.Mappers;
using TisApi.Models;
using TisApi.Services.Interfaces;

namespace TisApi.Services;

public class IncidentService(TisPostgresContext db, IncidentMapper mapper) : IIncidentService
{
    public async Task<IReadOnlyList<IncidentDto>> GetAllAsync(int? cameraId = null, string? type = null, short? minSeverity = null)
    {
        var query = db.Incidents.AsNoTracking();

        if (cameraId.HasValue) query = query.Where(i => i.CameraId == cameraId.Value);
        if (!string.IsNullOrEmpty(type)) query = query.Where(i => i.Type == type);
        if (minSeverity.HasValue) query = query.Where(i => i.Severity >= minSeverity.Value);

        return (await query.OrderByDescending(i => i.RecordedAt).ToListAsync())
            .Select(mapper.ToDto).ToList();
    }

    public async Task<IncidentDto?> GetByIdAsync(int id)
    {
        var incident = await db.Incidents.AsNoTracking().FirstOrDefaultAsync(i => i.Id == id);
        return incident is null ? null : mapper.ToDto(incident);
    }

    public async Task<IncidentDto> CreateAsync(CreateIncidentRequest request)
    {
        var incident = mapper.ToEntity(request);
        incident.RecordedAt = request.RecordedAt ?? DateTimeOffset.UtcNow;

        db.Incidents.Add(incident);
        await db.SaveChangesAsync();

        return mapper.ToDto(incident);
    }

    public async Task<bool> DeleteAsync(int id)
    {
        var incident = await db.Incidents.FindAsync(id);
        if (incident is null) return false;

        db.Incidents.Remove(incident);
        await db.SaveChangesAsync();
        return true;
    }
}
