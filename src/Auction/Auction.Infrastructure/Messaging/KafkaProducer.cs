using Auction.Application.Interfaces;
using Auction.Infrastructure.Options;
using Auction.SharedKernel;
using Confluent.Kafka;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using System.Text;
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
                logger.LogError("[Mensageria] Erro no producer Kafka: {Motivo}", error.Reason);
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
            var correlationId = CorrelationContext.GetOrCreate();

            var result = await _producer.ProduceAsync(topic, new Message<string, string>
            {
                Key = partitionKey,
                Value = message,
                Timestamp = Timestamp.Default,
                Headers = new Headers
                {
                    { "event-type", Encoding.UTF8.GetBytes(@event.GetType().Name) },
                    { "timestamp", Encoding.UTF8.GetBytes(DateTime.UtcNow.ToString("O")) },
                    { "correlation-id", Encoding.UTF8.GetBytes(correlationId) }
                }
            }, cancellationToken);

            _logger.LogInformation(
                "[Mensageria] Evento publicado no Kafka: Topico={Topico}, Particao={Particao}, Offset={Offset}, Chave={Chave}",
                result.Topic, result.Partition.Value, result.Offset.Value, partitionKey);
        }
        catch (ProduceException<string, string> ex)
        {
            _logger.LogError(ex,
                "[Mensageria] Falha ao publicar evento no Kafka: Topico={Topico}, Chave={Chave}, CodigoErro={CodigoErro}",
                topic, partitionKey, ex.Error.Code);
            throw;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex,
                "[Mensageria] Erro inesperado ao publicar no Kafka: TipoEvento={TipoEvento}",
                @event.GetType().Name);
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

        _logger.LogInformation(
            "[Mensageria] Lote de eventos publicado: Total={Total}, Topico={Topico}",
            events.Count(), topic);
    }

    public void Dispose()
    {
        _logger.LogInformation("[Mensageria] Liberando recursos do producer Kafka...");
        _producer?.Flush(TimeSpan.FromSeconds(10));
        _producer?.Dispose();
    }
}
