using RabbitMQ.Client;
using TisApi.Messaging.Contracts;
using TisApi.Models;
using TisApi.Services.Interfaces;

namespace TisApi.Messaging.Consumers;

public class IncidentCreatedConsumer(
    IConnection connection,
    IServiceScopeFactory scopeFactory,
    ILogger<IncidentCreatedConsumer> logger)
    : ConsumerBase<IncidentCreated>(connection, scopeFactory, logger)
{
    protected override string QueueName => "incident.created";

    protected override async Task HandleAsync(IncidentCreated msg, IServiceProvider services, CancellationToken ct)
    {
        var incidentService = services.GetRequiredService<IIncidentService>();
        await incidentService.CreateAsync(new CreateIncidentRequest(msg.CameraId, msg.Type, msg.Severity, msg.RecordedAt));
    }
}
