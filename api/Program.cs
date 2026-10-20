using Microsoft.EntityFrameworkCore;
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

// dbcontext
builder.Services.AddDbContext<TisDbContext>(options => options.UseNpgsql(builder.Configuration.GetConnectionString("Postgres")));

// mapper
builder.Services.AddSingleton<StopMapper>();
builder.Services.AddSingleton<VehicleTypeMapper>();
builder.Services.AddSingleton<VehicleMapper>();
builder.Services.AddSingleton<JourneyMapper>();
builder.Services.AddSingleton<JourneyPathTimePlanMapper>();
builder.Services.AddSingleton<JourneyPathMapper>();
builder.Services.AddSingleton<UsingJourneyMapper>();
builder.Services.AddSingleton<UsingJourneyLogMapper>();

// services
builder.Services.AddScoped<IVehicleTypeService, VehicleTypeService>();
builder.Services.AddScoped<IVehicleService, VehicleService>();
builder.Services.AddScoped<IStopService, StopService>();
builder.Services.AddScoped<IJourneyService, JourneyService>();
builder.Services.AddScoped<IJourneyPathService, JourneyPathService>();
builder.Services.AddScoped<IJourneyPathTimePlanService, JourneyPathTimePlanService>();
builder.Services.AddScoped<IUsingJourneyService, UsingJourneyService>();
builder.Services.AddScoped<IUsingJourneyLogService, UsingJourneyLogService>();

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

app.Run();