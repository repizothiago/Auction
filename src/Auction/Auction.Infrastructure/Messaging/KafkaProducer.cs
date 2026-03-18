using Auction.Application.Interfaces;
using Auction.Infrastructure.Options;
using Auction.SharedKernel;
using Confluent.Kafka;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using System.Text.Json;

namespace Auction.Infrastructure.Messaging;

public class KafkaProducer : IMessageBus, IDisposable
{
    private readonly IProducer<string, string> _producer;
    private readonly ILogger<KafkaProducer> _logger;
    private readonly JsonSerializerOptions _jsonOptions;

    public KafkaProducer(IOptions<KafkaOptions> kafkaOptions, ILogger<KafkaProducer> logger)
    {
        var options = kafkaOptions.Value;

        var config = new ProducerConfig
        {
            BootstrapServers = options.BootstrapServers,
            Acks = options.Producer.Acks switch
            {
                "all" => Acks.All,
                "1" => Acks.Leader,
                "0" => Acks.None,
                _ => Acks.All
            },
            EnableIdempotence = options.Producer.EnableIdempotence,
            MaxInFlight = options.Producer.MaxInFlight,
            MessageTimeoutMs = options.Producer.MessageTimeoutMs,
            RequestTimeoutMs = options.Producer.RequestTimeoutMs,
            MessageSendMaxRetries = 3,
            CompressionType = CompressionType.Snappy,
            LingerMs = 5,
            BatchSize = 16384
        };

        _producer = new ProducerBuilder<string, string>(config)
            .SetErrorHandler((_, error) =>
            {
                logger.LogError("Kafka Producer Error: {Reason}", error.Reason);
            })
            .Build();

        _logger = logger;

        _jsonOptions = new JsonSerializerOptions
        {
            PropertyNameCaseInsensitive = true,
            WriteIndented = false
        };
    }

    public async Task PublishAsync<TEvent>(
        string topic,
        TEvent @event,
        string partitionKey,
        CancellationToken cancellationToken = default) where TEvent : IDomainEvent
    {
        try
        {
            var message = JsonSerializer.Serialize(@event, _jsonOptions);

            var result = await _producer.ProduceAsync(topic, new Message<string, string>
            {
                Key = partitionKey, // Garante que eventos do mesmo leilão vão para a mesma partição
                Value = message,
                Timestamp = Timestamp.Default,
                Headers = new Headers
                {
                    { "event-type", System.Text.Encoding.UTF8.GetBytes(@event.GetType().Name) },
                    { "timestamp", System.Text.Encoding.UTF8.GetBytes(DateTime.UtcNow.ToString("O")) }
                }
            }, cancellationToken);

            _logger.LogInformation(
                "Event published to Kafka: Topic={Topic}, Partition={Partition}, Offset={Offset}, Key={Key}",
                result.Topic, result.Partition.Value, result.Offset.Value, partitionKey);
        }
        catch (ProduceException<string, string> ex)
        {
            _logger.LogError(ex,
                "Failed to publish event to Kafka: Topic={Topic}, Key={Key}, Error={ErrorCode}",
                topic, partitionKey, ex.Error.Code);
            throw;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Unexpected error publishing event to Kafka: {@Event}", @event);
            throw;
        }
    }

    public async Task PublishBatchAsync<TEvent>(
        string topic,
        IEnumerable<TEvent> events,
        Func<TEvent, string> partitionKeySelector,
        CancellationToken cancellationToken = default) where TEvent : IDomainEvent
    {
        var tasks = events.Select(e => PublishAsync(topic, e, partitionKeySelector(e), cancellationToken));
        await Task.WhenAll(tasks);

        _logger.LogInformation("Batch of {Count} events published to topic {Topic}", events.Count(), topic);
    }

    public void Dispose()
    {
        _logger.LogInformation("Flushing Kafka producer...");
        _producer?.Flush(TimeSpan.FromSeconds(10));
        _producer?.Dispose();
    }
}
