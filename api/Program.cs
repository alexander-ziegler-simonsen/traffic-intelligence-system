using Microsoft.EntityFrameworkCore;
using MongoDB.Driver;
using StackExchange.Redis;
using RabbitMQ.Client;
using TisApi.Data.Mongo;
using TisApi.Data.Postgres;
using TisApi.Data.Redis;
using TisApi.Mappers;
using TisApi.Messaging.Consumers;
using TisApi.Services;
using TisApi.Services.Interfaces;

var builder = WebApplication.CreateBuilder(args);

builder.Services.AddControllers();
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();

// Postgres — EF Core
builder.Services.AddDbContext<TisPostgresContext>(options =>
    options.UseNpgsql(builder.Configuration.GetConnectionString("Postgres")));

// MongoDB — singleton client + scoped context
builder.Services.AddSingleton<IMongoClient>(_ =>
    new MongoClient(builder.Configuration.GetConnectionString("Mongo")));
builder.Services.AddScoped<TisMongoContext>();

// Redis — singleton multiplexer + scoped context
builder.Services.AddSingleton<IConnectionMultiplexer>(_ =>
    ConnectionMultiplexer.Connect(builder.Configuration.GetConnectionString("Redis")!));
builder.Services.AddScoped<TisRedisContext>();

// RabbitMQ — raw connection (singleton, channels created per consumer)
builder.Services.AddSingleton<IConnection>(_ =>
{
    var factory = new ConnectionFactory
    {
        HostName = builder.Configuration["RabbitMQ:Host"],
        VirtualHost = builder.Configuration["RabbitMQ:VirtualHost"] ?? "/",
        UserName = builder.Configuration["RabbitMQ:Username"],
        Password = builder.Configuration["RabbitMQ:Password"]
    };
    return factory.CreateConnectionAsync().GetAwaiter().GetResult();
});

builder.Services.AddHostedService<IncidentCreatedConsumer>();
builder.Services.AddHostedService<IncidentDeletedConsumer>();
builder.Services.AddHostedService<CameraStatusChangedConsumer>();

// Mappers
builder.Services.AddSingleton<RoadMapper>();
builder.Services.AddSingleton<CameraMapper>();
builder.Services.AddSingleton<IncidentMapper>();
builder.Services.AddSingleton<IncidentReportMapper>();
builder.Services.AddSingleton<LiveMapper>();

// Services
builder.Services.AddScoped<IRoadService, RoadService>();
builder.Services.AddScoped<ICameraService, CameraService>();
builder.Services.AddScoped<IIncidentService, IncidentService>();
builder.Services.AddScoped<IIncidentReportService, IncidentReportService>();
builder.Services.AddScoped<ILiveService, LiveService>();

var app = builder.Build();

if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI();
}

app.UseHttpsRedirection();
app.UseAuthorization();
app.MapControllers();

app.Run();
