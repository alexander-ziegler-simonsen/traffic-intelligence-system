using RabbitMQ.Client;
using TisApi.Messaging.Contracts;
using TisApi.Services.Interfaces;

namespace TisApi.Messaging.Consumers;

public class IncidentDeletedConsumer(
    IConnection connection,
    IServiceScopeFactory scopeFactory,
    ILogger<IncidentDeletedConsumer> logger)
    : ConsumerBase<IncidentDeleted>(connection, scopeFactory, logger)
{
    protected override string QueueName => "incident.deleted";

    protected override async Task HandleAsync(IncidentDeleted msg, IServiceProvider services, CancellationToken ct)
    {
        var incidentService = services.GetRequiredService<IIncidentService>();
        await incidentService.DeleteAsync(msg.IncidentId);
    }
}
