using Microsoft.EntityFrameworkCore;
using TisApi.Data.Postgres.Entities;

namespace TisApi.Data.Postgres;

public class TisPostgresContext(DbContextOptions<TisPostgresContext> options) : DbContext(options)
{
    public DbSet<Road> Roads { get; set; }
    public DbSet<Camera> Cameras { get; set; }
    public DbSet<Incident> Incidents { get; set; }

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        modelBuilder.Entity<Road>()
            .HasMany(r => r.Cameras)
            .WithOne(c => c.Road)
            .HasForeignKey(c => c.RoadId)
            .OnDelete(DeleteBehavior.Cascade);

        modelBuilder.Entity<Camera>()
            .HasMany(c => c.Incidents)
            .WithOne(i => i.Camera)
            .HasForeignKey(i => i.CameraId)
            .OnDelete(DeleteBehavior.Cascade);

        modelBuilder.Entity<Camera>()
            .HasIndex(c => c.Label)
            .IsUnique();
    }
}
