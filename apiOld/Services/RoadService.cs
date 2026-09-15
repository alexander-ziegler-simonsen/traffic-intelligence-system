using Microsoft.EntityFrameworkCore;
using TisApi.Data.Postgres;
using TisApi.DTOs;
using TisApi.Mappers;
using TisApi.Models;
using TisApi.Services.Interfaces;

namespace TisApi.Services;

public class RoadService(TisPostgresContext db, RoadMapper mapper) : IRoadService
{
    public async Task<IReadOnlyList<RoadDto>> GetAllAsync() =>
        (await db.Roads.AsNoTracking().ToListAsync())
            .Select(mapper.ToDto).ToList();

    public async Task<RoadDto?> GetByIdAsync(int id)
    {
        var road = await db.Roads.AsNoTracking().FirstOrDefaultAsync(r => r.Id == id);
        return road is null ? null : mapper.ToDto(road);
    }

    public async Task<RoadDto> CreateAsync(CreateRoadRequest request)
    {
        var road = mapper.ToEntity(request);
        db.Roads.Add(road);
        await db.SaveChangesAsync();
        return mapper.ToDto(road);
    }

    public async Task<RoadDto?> UpdateAsync(int id, UpdateRoadRequest request)
    {
        var road = await db.Roads.FindAsync(id);
        if (road is null) return null;

        mapper.UpdateEntity(request, road);
        await db.SaveChangesAsync();
        return mapper.ToDto(road);
    }

    public async Task<bool> DeleteAsync(int id)
    {
        var road = await db.Roads.FindAsync(id);
        if (road is null) return false;

        db.Roads.Remove(road);
        await db.SaveChangesAsync();
        return true;
    }
}
