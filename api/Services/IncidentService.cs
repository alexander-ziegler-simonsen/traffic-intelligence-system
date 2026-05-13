using Microsoft.EntityFrameworkCore;
using TisApi.Data.Postgres;
using TisApi.DTOs;
using TisApi.Mappers;
using TisApi.Models;
using TisApi.Services.Interfaces;

namespace TisApi.Services;

public class IncidentService(TisPostgresContext db, IncidentMapper mapper) : IIncidentService
{
    public async Task<IReadOnlyList<IncidentDto>> GetAllAsync(string? type = null, string? status = null)
    {
        var query = db.Incidents.AsNoTracking();

        if (!string.IsNullOrEmpty(type))   query = query.Where(i => i.Type   == type);
        if (!string.IsNullOrEmpty(status)) query = query.Where(i => i.Status == status);

        return (await query.OrderByDescending(i => i.ReportedAt).ToListAsync())
            .Select(mapper.ToDto).ToList();
    }

    public async Task<IncidentDto?> GetByIdAsync(Guid id)
    {
        var incident = await db.Incidents.AsNoTracking().FirstOrDefaultAsync(i => i.Id == id);
        return incident is null ? null : mapper.ToDto(incident);
    }

    public async Task<IncidentDto> CreateAsync(CreateIncidentRequest request)
    {
        var incident = mapper.ToEntity(request);
        incident.ReportedAt = request.ReportedAt ?? DateTimeOffset.UtcNow;

        db.Incidents.Add(incident);
        await db.SaveChangesAsync();

        return mapper.ToDto(incident);
    }

    public async Task<bool> DeleteAsync(Guid id)
    {
        var incident = await db.Incidents.FindAsync(id);
        if (incident is null) return false;

        db.Incidents.Remove(incident);
        await db.SaveChangesAsync();
        return true;
    }
}
