using Microsoft.EntityFrameworkCore;
using TisApi.Data.Postgres;
using TisApi.DTOs;
using TisApi.Mappers;
using TisApi.Models;
using TisApi.Services.Interfaces;

namespace TisApi.Services;

public class CameraService(TisPostgresContext db, CameraMapper mapper) : ICameraService
{
    public async Task<IReadOnlyList<CameraDto>> GetAllAsync(int? roadId = null)
    {
        var query = db.Cameras.AsNoTracking();
        if (roadId.HasValue)
            query = query.Where(c => c.RoadId == roadId.Value);
        return (await query.ToListAsync()).Select(mapper.ToDto).ToList();
    }

    public async Task<CameraDto?> GetByIdAsync(int id)
    {
        var camera = await db.Cameras.AsNoTracking().FirstOrDefaultAsync(c => c.Id == id);
        return camera is null ? null : mapper.ToDto(camera);
    }

    public async Task<CameraDto> CreateAsync(CreateCameraRequest request)
    {
        var camera = mapper.ToEntity(request);
        db.Cameras.Add(camera);
        await db.SaveChangesAsync();
        return mapper.ToDto(camera);
    }

    public async Task<CameraDto?> UpdateAsync(int id, UpdateCameraRequest request)
    {
        var camera = await db.Cameras.FindAsync(id);
        if (camera is null) return null;

        mapper.UpdateEntity(request, camera);
        await db.SaveChangesAsync();

        return mapper.ToDto(camera);
    }

    public async Task<bool> DeleteAsync(int id)
    {
        var camera = await db.Cameras.FindAsync(id);
        if (camera is null) return false;

        db.Cameras.Remove(camera);
        await db.SaveChangesAsync();
        return true;
    }
}
