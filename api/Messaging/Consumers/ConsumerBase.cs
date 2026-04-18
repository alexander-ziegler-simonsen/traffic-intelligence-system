using System.Text.Json;
using RabbitMQ.Client;
using RabbitMQ.Client.Events;

namespace TisApi.Messaging.Consumers;

public abstract class ConsumerBase<TMessage> : BackgroundService
{
    private readonly IConnection _connection;
    private readonly IServiceScopeFactory _scopeFactory;
    private readonly ILogger _logger;

    protected abstract string QueueName { get; }

    protected ConsumerBase(IConnection connection, IServiceScopeFactory scopeFactory, ILogger logger)
    {
        _connection = connection;
        _scopeFactory = scopeFactory;
        _logger = logger;
    }

    protected override async Task ExecuteAsync(CancellationToken stoppingToken)
    {
        var channel = await _connection.CreateChannelAsync(cancellationToken: stoppingToken);

        await channel.QueueDeclareAsync(
            queue: QueueName, durable: true, exclusive: false, autoDelete: false,
            cancellationToken: stoppingToken);

        var consumer = new AsyncEventingBasicConsumer(channel);

        consumer.ReceivedAsync += async (_, ea) =>
        {
            try
            {
                var message = JsonSerializer.Deserialize<TMessage>(
                    ea.Body.Span,
                    new JsonSerializerOptions { PropertyNameCaseInsensitive = true });

                if (message is not null)
                {
                    using var scope = _scopeFactory.CreateScope();
                    await HandleAsync(message, scope.ServiceProvider, stoppingToken);
                }

                await channel.BasicAckAsync(ea.DeliveryTag, multiple: false);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Failed to handle {MessageType}", typeof(TMessage).Name);
                await channel.BasicNackAsync(ea.DeliveryTag, multiple: false, requeue: false);
            }
        };

        await channel.BasicConsumeAsync(QueueName, autoAck: false, consumer: consumer, cancellationToken: stoppingToken);
        await Task.Delay(Timeout.Infinite, stoppingToken);
    }

    protected abstract Task HandleAsync(TMessage message, IServiceProvider services, CancellationToken ct);
}
