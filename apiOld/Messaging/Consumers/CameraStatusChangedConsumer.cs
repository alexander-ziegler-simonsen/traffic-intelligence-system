using RabbitMQ.Client;
using TisApi.Messaging.Contracts;
using TisApi.Models;
using TisApi.Services.Interfaces;

namespace TisApi.Messaging.Consumers;

public class CameraStatusChangedConsumer(
    IConnection connection,
    IServiceScopeFactory scopeFactory,
    ILogger<CameraStatusChangedConsumer> logger)
    : ConsumerBase<CameraStatusChanged>(connection, scopeFactory, logger)
{
    protected override string QueueName => "camera.status-changed";

    protected override async Task HandleAsync(CameraStatusChanged msg, IServiceProvider services, CancellationToken ct)
    {
        var cameraService = services.GetRequiredService<ICameraService>();

        var camera = await cameraService.GetByIdAsync(msg.CameraId);
        if (camera is null) return;

        await cameraService.UpdateAsync(msg.CameraId,
            new UpdateCameraRequest(camera.Label, camera.Latitude, camera.Longitude, msg.NewStatus));
    }
}
