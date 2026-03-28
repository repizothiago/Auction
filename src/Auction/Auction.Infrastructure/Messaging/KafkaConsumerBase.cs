using Auction.Infrastructure.Options;
using Auction.SharedKernel;
using Confluent.Kafka;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using System.Text;
using System.Text.Json;

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
        IOptions<KafkaOptions> kafkaOptions,
        IServiceProvider serviceProvider,
        ILogger logger,
        string topic,
        string consumerGroupId)
    {
        var options = kafkaOptions.Value;

        var config = new ConsumerConfig
        {
            BootstrapServers = options.BootstrapServers,
            GroupId = consumerGroupId,
            AutoOffsetReset = options.Consumer.AutoOffsetReset switch
            {
                "earliest" => AutoOffsetReset.Earliest,
                "latest" => AutoOffsetReset.Latest,
                _ => AutoOffsetReset.Earliest
            },
            EnableAutoCommit = options.Consumer.EnableAutoCommit,
            MaxPollIntervalMs = options.Consumer.MaxPollIntervalMs,
            SessionTimeoutMs = options.Consumer.SessionTimeoutMs,
            EnablePartitionEof = false
        };

        _consumer = new ConsumerBuilder<string, string>(config)
            .SetErrorHandler((_, error) =>
            {
                logger.LogError("[Mensageria] Erro no consumer Kafka: {Motivo}", error.Reason);
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

        Logger.LogInformation("[Mensageria] Consumer Kafka iniciado: Topico={Topico}", _topic);

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
                    var correlationId = consumeResult.Message.Headers
                        .TryGetLastBytes("correlation-id", out var headerBytes)
                            ? Encoding.UTF8.GetString(headerBytes)
                            : Guid.NewGuid().ToString();

                    CorrelationContext.Current = correlationId;

                    using var scope = ServiceProvider.CreateScope();
                    using (Serilog.Context.LogContext.PushProperty("CorrelationId", correlationId))
                    {
                        await ProcessEventAsync(@event, scope.ServiceProvider, stoppingToken);

                        _consumer.Commit(consumeResult);

                        Logger.LogInformation(
                            "[Mensageria] Evento processado com sucesso: TipoEvento={TipoEvento}, Particao={Particao}, Offset={Offset}, Chave={Chave}",
                            typeof(TEvent).Name,
                            consumeResult.Partition.Value,
                            consumeResult.Offset.Value,
                            consumeResult.Message.Key);
                    }
                }
            }
            catch (ConsumeException ex)
            {
                Logger.LogError(ex, "[Mensageria] Erro ao consumir mensagem do Kafka: Topico={Topico}", _topic);
            }
            catch (Exception ex)
            {
                Logger.LogError(ex, "[Mensageria] Erro ao processar evento do tópico: Topico={Topico}", _topic);
                // TODO: Implementar Dead Letter Queue (DLQ) aqui se necessário
            }
        }

        _consumer.Close();
        Logger.LogInformation("[Mensageria] Consumer Kafka encerrado: Topico={Topico}", _topic);
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
