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

    public virtual DbSet<Journey> Journeys { get; set; }

    public virtual DbSet<JourneyPath> JourneyPaths { get; set; }

    public virtual DbSet<JourneyPathTimePlan> JourneyPathTimePlans { get; set; }

    public virtual DbSet<Stop> Stops { get; set; }

    public virtual DbSet<UsingJourney> UsingJourneys { get; set; }

    public virtual DbSet<UsingJourneyLog> UsingJourneyLogs { get; set; }

    public virtual DbSet<Vehicle> Vehicles { get; set; }

    public virtual DbSet<VehicleType> VehicleTypes { get; set; }

    protected override void OnConfiguring(DbContextOptionsBuilder optionsBuilder)
#warning To protect potentially sensitive information in your connection string, you should move it out of source code. You can avoid scaffolding the connection string by using the Name= syntax to read it from configuration - see https://go.microsoft.com/fwlink/?linkid=2131148. For more guidance on storing connection strings, see https://go.microsoft.com/fwlink/?LinkId=723263.
        => optionsBuilder.UseNpgsql("Host=127.0.0.1;Port=5432;Database=tis_db;Username=tis_user;Password=tis_password");

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
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
            entity.Property(e => e.FkJourneyId).HasColumnName("fk_journey_id");
            entity.Property(e => e.FkStopId).HasColumnName("fk_stop_id");
            entity.Property(e => e.Order).HasColumnName("order");

            entity.HasOne(d => d.FkJourney).WithMany(p => p.JourneyPaths)
                .HasForeignKey(d => d.FkJourneyId)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("journey_path_fk_journey_id_fkey");

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
