using Microsoft.EntityFrameworkCore;
using TisApi.Data.Postgres.Entities;

namespace TisApi.Data.Postgres;

public class TisPostgresContext(DbContextOptions<TisPostgresContext> options) : DbContext(options)
{
    // Roads
    public DbSet<Road> Roads { get; set; }
    public DbSet<Camera> Cameras { get; set; }

    // Incidents
    public DbSet<Incident> Incidents { get; set; }
    public DbSet<IncidentEvent> IncidentEvents { get; set; }
    public DbSet<RouteImpact> RouteImpacts { get; set; }
    public DbSet<RerouteDecision> RerouteDecisions { get; set; }

    // Bus
    public DbSet<BusRoute> BusRoutes { get; set; }
    public DbSet<BusStop> BusStops { get; set; }
    public DbSet<BusRouteAssignment> BusRouteAssignments { get; set; }
    public DbSet<BusJourney> BusJourneys { get; set; }
    public DbSet<BusJourneyStopEvent> BusJourneyStopEvents { get; set; }

    // Train
    public DbSet<TrainRoute> TrainRoutes { get; set; }
    public DbSet<TrainStation> TrainStations { get; set; }
    public DbSet<TrainRouteAssignment> TrainRouteAssignments { get; set; }
    public DbSet<TrainJourney> TrainJourneys { get; set; }
    public DbSet<TrainJourneyStopEvent> TrainJourneyStopEvents { get; set; }

    // Traffic Lights
    public DbSet<TrafficLight> TrafficLights { get; set; }
    public DbSet<TrafficLightPhase> TrafficLightPhases { get; set; }
    public DbSet<TrafficLightOverrideEvent> TrafficLightOverrideEvents { get; set; }

    // Subscribers
    public DbSet<Subscriber> Subscribers { get; set; }
    public DbSet<WebhookDelivery> WebhookDeliveries { get; set; }

    // Simulation
    public DbSet<SimulatorConfig> SimulatorConfigs { get; set; }

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        // Roads
        modelBuilder.Entity<Road>()
            .HasMany(r => r.Cameras)
            .WithOne(c => c.Road)
            .HasForeignKey(c => c.RoadId)
            .OnDelete(DeleteBehavior.Cascade);

        modelBuilder.Entity<Camera>()
            .HasIndex(c => c.Label)
            .IsUnique();

        // Incidents
        modelBuilder.Entity<Incident>()
            .HasMany(i => i.IncidentEvents)
            .WithOne(e => e.Incident)
            .HasForeignKey(e => e.FkIncidentId)
            .OnDelete(DeleteBehavior.Cascade);

        modelBuilder.Entity<Incident>()
            .HasMany(i => i.RouteImpacts)
            .WithOne(ri => ri.Incident)
            .HasForeignKey(ri => ri.FkIncidentId)
            .OnDelete(DeleteBehavior.Cascade);

        modelBuilder.Entity<Incident>()
            .HasMany(i => i.WebhookDeliveries)
            .WithOne(wd => wd.Incident)
            .HasForeignKey(wd => wd.FkIncidentId)
            .OnDelete(DeleteBehavior.SetNull);

        modelBuilder.Entity<RouteImpact>()
            .HasMany(ri => ri.RerouteDecisions)
            .WithOne(rd => rd.RouteImpact)
            .HasForeignKey(rd => rd.FkRouteImpactId)
            .OnDelete(DeleteBehavior.Cascade);

        // Bus
        modelBuilder.Entity<BusRoute>()
            .HasMany(r => r.BusStops)
            .WithOne(s => s.BusRoute)
            .HasForeignKey(s => s.FkRouteId)
            .OnDelete(DeleteBehavior.Cascade);

        modelBuilder.Entity<BusRoute>()
            .HasMany(r => r.RouteAssignments)
            .WithOne(a => a.BusRoute)
            .HasForeignKey(a => a.FkRouteId)
            .OnDelete(DeleteBehavior.Cascade);

        modelBuilder.Entity<BusRoute>()
            .HasMany(r => r.Journeys)
            .WithOne(j => j.BusRoute)
            .HasForeignKey(j => j.FkRouteId)
            .OnDelete(DeleteBehavior.Cascade);

        modelBuilder.Entity<BusRouteAssignment>()
            .HasMany(a => a.Journeys)
            .WithOne(j => j.RouteAssignment)
            .HasForeignKey(j => j.FkRouteAssignmentId)
            .OnDelete(DeleteBehavior.Restrict);

        modelBuilder.Entity<BusJourney>()
            .HasMany(j => j.StopEvents)
            .WithOne(e => e.Journey)
            .HasForeignKey(e => e.FkJourneyId)
            .OnDelete(DeleteBehavior.Cascade);

        modelBuilder.Entity<BusStop>()
            .HasMany(s => s.JourneyStopEvents)
            .WithOne(e => e.BusStop)
            .HasForeignKey(e => e.FkBusStopId)
            .OnDelete(DeleteBehavior.Restrict);

        // Train
        modelBuilder.Entity<TrainRoute>()
            .HasMany(r => r.Stations)
            .WithOne(s => s.TrainRoute)
            .HasForeignKey(s => s.FkRouteId)
            .OnDelete(DeleteBehavior.Cascade);

        modelBuilder.Entity<TrainRoute>()
            .HasMany(r => r.RouteAssignments)
            .WithOne(a => a.TrainRoute)
            .HasForeignKey(a => a.FkRouteId)
            .OnDelete(DeleteBehavior.Cascade);

        modelBuilder.Entity<TrainRoute>()
            .HasMany(r => r.Journeys)
            .WithOne(j => j.TrainRoute)
            .HasForeignKey(j => j.FkRouteId)
            .OnDelete(DeleteBehavior.Cascade);

        modelBuilder.Entity<TrainRouteAssignment>()
            .HasMany(a => a.Journeys)
            .WithOne(j => j.RouteAssignment)
            .HasForeignKey(j => j.FkRouteAssignmentId)
            .OnDelete(DeleteBehavior.Restrict);

        modelBuilder.Entity<TrainJourney>()
            .HasMany(j => j.StopEvents)
            .WithOne(e => e.Journey)
            .HasForeignKey(e => e.FkJourneyId)
            .OnDelete(DeleteBehavior.Cascade);

        modelBuilder.Entity<TrainStation>()
            .HasMany(s => s.JourneyStopEvents)
            .WithOne(e => e.Station)
            .HasForeignKey(e => e.FkStationId)
            .OnDelete(DeleteBehavior.Restrict);

        // Traffic Lights
        modelBuilder.Entity<TrafficLight>()
            .HasMany(t => t.Phases)
            .WithOne(p => p.TrafficLight)
            .HasForeignKey(p => p.FkTrafficLightId)
            .OnDelete(DeleteBehavior.Cascade);

        modelBuilder.Entity<TrafficLight>()
            .HasMany(t => t.OverrideEvents)
            .WithOne(e => e.TrafficLight)
            .HasForeignKey(e => e.FkTrafficLightId)
            .OnDelete(DeleteBehavior.Cascade);

        modelBuilder.Entity<TrafficLightPhase>()
            .HasMany(p => p.OverrideEvents)
            .WithOne(e => e.Phase)
            .HasForeignKey(e => e.FkPhaseId)
            .OnDelete(DeleteBehavior.Restrict);

        // Subscribers
        modelBuilder.Entity<Subscriber>()
            .HasMany(s => s.WebhookDeliveries)
            .WithOne(wd => wd.Subscriber)
            .HasForeignKey(wd => wd.FkSubscriberId)
            .OnDelete(DeleteBehavior.Cascade);
    }
}
