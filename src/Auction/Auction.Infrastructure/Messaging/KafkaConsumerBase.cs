using System.Text.Json;
using Auction.SharedKernel;
using Confluent.Kafka;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;

namespace Auction.Infrastructure.Messaging;

/// <summary>
/// Base class para consumers Kafka com padrão BackgroundService
/// </summary>
public abstract class KafkaConsumerBase<TEvent> : BackgroundService where TEvent : IDomainEvent
{
    private readonly IConsumer<string, string> _consumer;
    protected readonly IServiceProvider ServiceProvider;
    protected readonly ILogger Logger;
    private readonly string _topic;
    private readonly JsonSerializerOptions _jsonOptions;

    protected KafkaConsumerBase(
        IConfiguration configuration,
        IServiceProvider serviceProvider,
        ILogger logger,
        string topic,
        string consumerGroupId)
    {
        var config = new ConsumerConfig
        {
            BootstrapServers = configuration["Kafka:BootstrapServers"] ?? "localhost:9092",
            GroupId = consumerGroupId,
            AutoOffsetReset = AutoOffsetReset.Earliest,
            EnableAutoCommit = false, // Commit manual após processamento
            MaxPollIntervalMs = 300000, // 5 minutos
            SessionTimeoutMs = 10000,
            EnablePartitionEof = false
        };

        _consumer = new ConsumerBuilder<string, string>(config)
            .SetErrorHandler((_, error) =>
            {
                logger.LogError("Kafka Consumer Error: {Reason}", error.Reason);
            })
            .Build();

        ServiceProvider = serviceProvider;
        Logger = logger;
        _topic = topic;
        
        _jsonOptions = new JsonSerializerOptions
        {
            PropertyNameCaseInsensitive = true
        };
    }

    protected override async Task ExecuteAsync(CancellationToken stoppingToken)
    {
        _consumer.Subscribe(_topic);
        
        Logger.LogInformation("Kafka Consumer started for topic: {Topic}", _topic);

        while (!stoppingToken.IsCancellationRequested)
        {
            try
            {
                var consumeResult = _consumer.Consume(stoppingToken);

                if (consumeResult?.Message == null)
                    continue;

                var @event = JsonSerializer.Deserialize<TEvent>(
                    consumeResult.Message.Value, 
                    _jsonOptions);

                if (@event is not null)
                {
                    using var scope = ServiceProvider.CreateScope();
                    
                    await ProcessEventAsync(@event, scope.ServiceProvider, stoppingToken);

                    // Commit offset apenas após processamento bem-sucedido
                    _consumer.Commit(consumeResult);

                    Logger.LogInformation(
                        "Event {EventType} processed: Partition={Partition}, Offset={Offset}, Key={Key}",
                        typeof(TEvent).Name,
                        consumeResult.Partition.Value,
                        consumeResult.Offset.Value,
                        consumeResult.Message.Key);
                }
            }
            catch (ConsumeException ex)
            {
                Logger.LogError(ex, "Error consuming message from Kafka topic: {Topic}", _topic);
            }
            catch (Exception ex)
            {
                Logger.LogError(ex, "Error processing event from topic: {Topic}", _topic);
                // TODO: Implementar Dead Letter Queue (DLQ) aqui se necessário
            }
        }

        _consumer.Close();
        Logger.LogInformation("Kafka Consumer stopped for topic: {Topic}", _topic);
    }

    /// <summary>
    /// Método abstrato para processar o evento. Implementado pelas subclasses.
    /// </summary>
    protected abstract Task ProcessEventAsync(
        TEvent @event, 
        IServiceProvider serviceProvider, 
        CancellationToken cancellationToken);

    public override void Dispose()
    {
        _consumer?.Close();
        _consumer?.Dispose();
        base.Dispose();
    }
}
