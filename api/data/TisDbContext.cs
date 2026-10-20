using System;
using System.Collections.Generic;
using Microsoft.EntityFrameworkCore;
using api.Models;

namespace api.data;

public partial class TisDbContext : DbContext
{
    public TisDbContext()
    {
    }

    public TisDbContext(DbContextOptions<TisDbContext> options)
        : base(options)
    {
    }

    public virtual DbSet<Disruption> Disruptions { get; set; }

    public virtual DbSet<DisruptionSegment> DisruptionSegments { get; set; }

    public virtual DbSet<DisruptionStop> DisruptionStops { get; set; }

    public virtual DbSet<Journey> Journeys { get; set; }

    public virtual DbSet<JourneyPath> JourneyPaths { get; set; }

    public virtual DbSet<JourneyPathTimePlan> JourneyPathTimePlans { get; set; }

    public virtual DbSet<JourneyPathVersion> JourneyPathVersions { get; set; }

    public virtual DbSet<Stop> Stops { get; set; }

    public virtual DbSet<UsingJourney> UsingJourneys { get; set; }

    public virtual DbSet<UsingJourneyLog> UsingJourneyLogs { get; set; }

    public virtual DbSet<Vehicle> Vehicles { get; set; }

    public virtual DbSet<VehicleType> VehicleTypes { get; set; }

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        modelBuilder.Entity<Disruption>(entity =>
        {
            entity.HasKey(e => e.Id).HasName("disruption_pkey");

            entity.ToTable("disruption");

            entity.Property(e => e.Id)
                .HasDefaultValueSql("uuidv7()")
                .HasColumnName("id");
            entity.Property(e => e.Cause)
                .HasMaxLength(50)
                .HasColumnName("cause");
            entity.Property(e => e.CreatedAt)
                .HasDefaultValueSql("now()")
                .HasColumnName("created_at");
            entity.Property(e => e.Description).HasColumnName("description");
            entity.Property(e => e.ValidFrom).HasColumnName("valid_from");
            entity.Property(e => e.ValidTo).HasColumnName("valid_to");
        });

        modelBuilder.Entity<DisruptionSegment>(entity =>
        {
            entity.HasKey(e => e.Id).HasName("disruption_segment_pkey");

            entity.ToTable("disruption_segment");

            entity.Property(e => e.Id)
                .HasDefaultValueSql("uuidv7()")
                .HasColumnName("id");
            entity.Property(e => e.FkDisruptionId).HasColumnName("fk_disruption_id");
            entity.Property(e => e.FkFromStopId).HasColumnName("fk_from_stop_id");
            entity.Property(e => e.FkToStopId).HasColumnName("fk_to_stop_id");

            entity.HasOne(d => d.FkDisruption).WithMany(p => p.DisruptionSegments)
                .HasForeignKey(d => d.FkDisruptionId)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("disruption_segment_fk_disruption_id_fkey");

            entity.HasOne(d => d.FkFromStop).WithMany(p => p.DisruptionSegmentFkFromStops)
                .HasForeignKey(d => d.FkFromStopId)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("disruption_segment_fk_from_stop_id_fkey");

            entity.HasOne(d => d.FkToStop).WithMany(p => p.DisruptionSegmentFkToStops)
                .HasForeignKey(d => d.FkToStopId)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("disruption_segment_fk_to_stop_id_fkey");
        });

        modelBuilder.Entity<DisruptionStop>(entity =>
        {
            entity.HasKey(e => e.Id).HasName("disruption_stop_pkey");

            entity.ToTable("disruption_stop");

            entity.Property(e => e.Id)
                .HasDefaultValueSql("uuidv7()")
                .HasColumnName("id");
            entity.Property(e => e.FkDisruptionId).HasColumnName("fk_disruption_id");
            entity.Property(e => e.FkStopId).HasColumnName("fk_stop_id");

            entity.HasOne(d => d.FkDisruption).WithMany(p => p.DisruptionStops)
                .HasForeignKey(d => d.FkDisruptionId)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("disruption_stop_fk_disruption_id_fkey");

            entity.HasOne(d => d.FkStop).WithMany(p => p.DisruptionStops)
                .HasForeignKey(d => d.FkStopId)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("disruption_stop_fk_stop_id_fkey");
        });

        modelBuilder.Entity<Journey>(entity =>
        {
            entity.HasKey(e => e.Id).HasName("journey_pkey");

            entity.ToTable("journey");

            entity.Property(e => e.Id)
                .HasDefaultValueSql("uuidv7()")
                .HasColumnName("id");
            entity.Property(e => e.Direction)
                .HasMaxLength(20)
                .HasColumnName("direction");
            entity.Property(e => e.FkVehicleTypeId).HasColumnName("fk_vehicle_type_id");
            entity.Property(e => e.Status)
                .HasMaxLength(20)
                .HasColumnName("status");

            entity.HasOne(d => d.FkVehicleType).WithMany(p => p.Journeys)
                .HasForeignKey(d => d.FkVehicleTypeId)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("journey_fk_vehicle_type_id_fkey");
        });

        modelBuilder.Entity<JourneyPath>(entity =>
        {
            entity.HasKey(e => e.Id).HasName("journey_path_pkey");

            entity.ToTable("journey_path");

            entity.Property(e => e.Id)
                .HasDefaultValueSql("uuidv7()")
                .HasColumnName("id");
            entity.Property(e => e.FkJourneyPathVersionId).HasColumnName("fk_journey_path_version_id");
            entity.Property(e => e.FkStopId).HasColumnName("fk_stop_id");
            entity.Property(e => e.Order).HasColumnName("order");

            entity.HasOne(d => d.FkJourneyPathVersion).WithMany(p => p.JourneyPaths)
                .HasForeignKey(d => d.FkJourneyPathVersionId)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("journey_path_fk_journey_path_version_id_fkey");

            entity.HasOne(d => d.FkStop).WithMany(p => p.JourneyPaths)
                .HasForeignKey(d => d.FkStopId)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("journey_path_fk_stop_id_fkey");
        });

        modelBuilder.Entity<JourneyPathTimePlan>(entity =>
        {
            entity.HasKey(e => e.Id).HasName("journey_path_time_plan_pkey");

            entity.ToTable("journey_path_time_plan");

            entity.Property(e => e.Id)
                .HasDefaultValueSql("uuidv7()")
                .HasColumnName("id");
            entity.Property(e => e.FkJourneyPathId).HasColumnName("fk_journey_path_id");
            entity.Property(e => e.PlannedTime).HasColumnName("planned_time");

            entity.HasOne(d => d.FkJourneyPath).WithMany(p => p.JourneyPathTimePlans)
                .HasForeignKey(d => d.FkJourneyPathId)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("journey_path_time_plan_fk_journey_path_id_fkey");
        });

        modelBuilder.Entity<JourneyPathVersion>(entity =>
        {
            entity.HasKey(e => e.Id).HasName("journey_path_version_pkey");

            entity.ToTable("journey_path_version");

            entity.HasIndex(e => new { e.FkJourneyId, e.VersionNo }, "journey_path_version_fk_journey_id_version_no_key").IsUnique();

            entity.Property(e => e.Id)
                .HasDefaultValueSql("uuidv7()")
                .HasColumnName("id");
            entity.Property(e => e.CreatedAt)
                .HasDefaultValueSql("now()")
                .HasColumnName("created_at");
            entity.Property(e => e.FkDisruptionId).HasColumnName("fk_disruption_id");
            entity.Property(e => e.FkJourneyId).HasColumnName("fk_journey_id");
            entity.Property(e => e.Kind)
                .HasMaxLength(20)
                .HasColumnName("kind");
            entity.Property(e => e.ValidFrom).HasColumnName("valid_from");
            entity.Property(e => e.ValidTo).HasColumnName("valid_to");
            entity.Property(e => e.VersionNo).HasColumnName("version_no");

            entity.HasOne(d => d.FkDisruption).WithMany(p => p.JourneyPathVersions)
                .HasForeignKey(d => d.FkDisruptionId)
                .HasConstraintName("journey_path_version_fk_disruption_id_fkey");

            entity.HasOne(d => d.FkJourney).WithMany(p => p.JourneyPathVersions)
                .HasForeignKey(d => d.FkJourneyId)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("journey_path_version_fk_journey_id_fkey");
        });

        modelBuilder.Entity<Stop>(entity =>
        {
            entity.HasKey(e => e.Id).HasName("stop_pkey");

            entity.ToTable("stop");

            entity.Property(e => e.Id)
                .HasDefaultValueSql("uuidv7()")
                .HasColumnName("id");
            entity.Property(e => e.FkVehicleTypeId).HasColumnName("fk_vehicle_type_id");
            entity.Property(e => e.Latitude).HasColumnName("latitude");
            entity.Property(e => e.Longitude).HasColumnName("longitude");

            entity.HasOne(d => d.FkVehicleType).WithMany(p => p.Stops)
                .HasForeignKey(d => d.FkVehicleTypeId)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("stop_fk_vehicle_type_id_fkey");
        });

        modelBuilder.Entity<UsingJourney>(entity =>
        {
            entity.HasKey(e => e.Id).HasName("using_journey_pkey");

            entity.ToTable("using_journey");

            entity.Property(e => e.Id)
                .HasDefaultValueSql("uuidv7()")
                .HasColumnName("id");
            entity.Property(e => e.FkJourneyId).HasColumnName("fk_journey_id");
            entity.Property(e => e.FkVehicleId).HasColumnName("fk_vehicle_id");

            entity.HasOne(d => d.FkJourney).WithMany(p => p.UsingJourneys)
                .HasForeignKey(d => d.FkJourneyId)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("using_journey_fk_journey_id_fkey");

            entity.HasOne(d => d.FkVehicle).WithMany(p => p.UsingJourneys)
                .HasForeignKey(d => d.FkVehicleId)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("using_journey_fk_vehicle_id_fkey");
        });

        modelBuilder.Entity<UsingJourneyLog>(entity =>
        {
            entity.HasKey(e => e.Id).HasName("using_journey_log_pkey");

            entity.ToTable("using_journey_log");

            entity.Property(e => e.Id)
                .HasDefaultValueSql("uuidv7()")
                .HasColumnName("id");
            entity.Property(e => e.FkStopId).HasColumnName("fk_stop_id");
            entity.Property(e => e.FkUsingJourneyId).HasColumnName("fk_using_journey_id");
            entity.Property(e => e.Timestamp)
                .HasDefaultValueSql("now()")
                .HasColumnName("timestamp");

            entity.HasOne(d => d.FkStop).WithMany(p => p.UsingJourneyLogs)
                .HasForeignKey(d => d.FkStopId)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("using_journey_log_fk_stop_id_fkey");

            entity.HasOne(d => d.FkUsingJourney).WithMany(p => p.UsingJourneyLogs)
                .HasForeignKey(d => d.FkUsingJourneyId)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("using_journey_log_fk_using_journey_id_fkey");
        });

        modelBuilder.Entity<Vehicle>(entity =>
        {
            entity.HasKey(e => e.Id).HasName("vehicle_pkey");

            entity.ToTable("vehicle");

            entity.Property(e => e.Id)
                .HasDefaultValueSql("uuidv7()")
                .HasColumnName("id");
            entity.Property(e => e.FkVehicleTypeId).HasColumnName("fk_vehicle_type_id");
            entity.Property(e => e.VehicleNumber)
                .HasMaxLength(100)
                .HasColumnName("vehicle_number");

            entity.HasOne(d => d.FkVehicleType).WithMany(p => p.Vehicles)
                .HasForeignKey(d => d.FkVehicleTypeId)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("vehicle_fk_vehicle_type_id_fkey");
        });

        modelBuilder.Entity<VehicleType>(entity =>
        {
            entity.HasKey(e => e.Id).HasName("vehicle_type_pkey");

            entity.ToTable("vehicle_type");

            entity.Property(e => e.Id)
                .HasDefaultValueSql("uuidv7()")
                .HasColumnName("id");
            entity.Property(e => e.Name)
                .HasMaxLength(100)
                .HasColumnName("name");
        });

        OnModelCreatingPartial(modelBuilder);
    }

    partial void OnModelCreatingPartial(ModelBuilder modelBuilder);
}
