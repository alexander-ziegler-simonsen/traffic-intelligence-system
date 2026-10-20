using Microsoft.EntityFrameworkCore;
using NetTopologySuite;
using NetTopologySuite.IO.Converters;
using Scalar.AspNetCore;
using api.data;
using api.Endpoints;
using api.Services.Interfaces;
using api.Services;
using api.Mapping;

var builder = WebApplication.CreateBuilder(args);

// Add services to the container.
// Learn more about configuring OpenAPI at https://aka.ms/aspnet/openapi
builder.Services.AddOpenApi();

// dbcontext - NetTopologySuite maps PostGIS geometry columns
builder.Services.AddDbContext<TisDbContext>(options => options.UseNpgsql(
    builder.Configuration.GetConnectionString("Postgres"),
    npgsql => npgsql.UseNetTopologySuite()));

// geometries are sent and returned as GeoJSON, lng/lat (SRID 4326 like the PostGIS columns)
builder.Services.ConfigureHttpJsonOptions(options =>
    options.SerializerOptions.Converters.Add(new GeoJsonConverterFactory(NtsGeometryServices.Instance.CreateGeometryFactory(4326))));

// mapper
builder.Services.AddSingleton<StopMapper>();
builder.Services.AddSingleton<VehicleTypeMapper>();
builder.Services.AddSingleton<VehicleMapper>();
builder.Services.AddSingleton<JourneyMapper>();
builder.Services.AddSingleton<JourneyPathTimePlanMapper>();
builder.Services.AddSingleton<JourneyPathMapper>();
builder.Services.AddSingleton<UsingJourneyMapper>();
builder.Services.AddSingleton<UsingJourneyLogMapper>();
builder.Services.AddSingleton<DisruptionMapper>();
builder.Services.AddSingleton<DisruptionStopMapper>();
builder.Services.AddSingleton<DisruptionSegmentMapper>();
builder.Services.AddSingleton<JourneyPathVersionMapper>();

// services
builder.Services.AddScoped<IVehicleTypeService, VehicleTypeService>();
builder.Services.AddScoped<IVehicleService, VehicleService>();
builder.Services.AddScoped<IStopService, StopService>();
builder.Services.AddScoped<IJourneyService, JourneyService>();
builder.Services.AddScoped<IJourneyPathService, JourneyPathService>();
builder.Services.AddScoped<IJourneyPathTimePlanService, JourneyPathTimePlanService>();
builder.Services.AddScoped<IUsingJourneyService, UsingJourneyService>();
builder.Services.AddScoped<IUsingJourneyLogService, UsingJourneyLogService>();
builder.Services.AddScoped<IDisruptionService, DisruptionService>();
builder.Services.AddScoped<IDisruptionStopService, DisruptionStopService>();
builder.Services.AddScoped<IDisruptionSegmentService, DisruptionSegmentService>();
builder.Services.AddScoped<IJourneyPathVersionService, JourneyPathVersionService>();

var app = builder.Build();

// Configure the HTTP request pipeline.
if (app.Environment.IsDevelopment())
{
    app.MapOpenApi();
    app.MapScalarApiReference();
}

app.UseHttpsRedirection();

app.MapVehicleTypeEndpoints();
app.MapVehicleEndpoints();
app.MapStopEndpoints();
app.MapJourneyEndpoints();
app.MapJourneyPathEndpoints();
app.MapJourneyPathTimePlanEndpoints();
app.MapUsingJourneyEndpoints();
app.MapUsingJourneyLogEndpoints();
app.MapDisruptionEndpoints();
app.MapDisruptionStopEndpoints();
app.MapDisruptionSegmentEndpoints();
app.MapJourneyPathVersionEndpoints();

app.Run();