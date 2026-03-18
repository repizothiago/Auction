using Auction.Application.Interfaces;
using Auction.Application.Interfaces.Repositories;
using Auction.Infrastructure.Caching;
using Auction.Infrastructure.Consumers;
using Auction.Infrastructure.Messaging;
using Auction.Infrastructure.Options;
using Auction.Infrastructure.Persistence;
using Auction.Infrastructure.Persistence.Repositories;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using StackExchange.Redis;

namespace Auction.Infrastructure;

public static class DependencyInjection
{
    /// <summary>
    /// Mascara a senha da connection string para logs
    /// </summary>
    private static string MaskConnectionString(string connectionString)
    {
        var parts = connectionString.Split(';');
        var masked = new List<string>();

        foreach (var part in parts)
        {
            if (part.Contains("Password", StringComparison.OrdinalIgnoreCase))
            {
                masked.Add("Password=***");
            }
            else
            {
                masked.Add(part);
            }
        }

        return string.Join(";", masked);
    }

    public static IServiceCollection AddInfrastructure(
        this IServiceCollection services,
        IConfiguration configuration)
    {
        // Configurar Options Pattern
        services.Configure<DatabaseOptions>(configuration.GetSection(DatabaseOptions.SectionName));
        services.Configure<RedisOptions>(configuration.GetSection(RedisOptions.SectionName));
        services.Configure<KafkaOptions>(configuration.GetSection(KafkaOptions.SectionName));

        // PostgreSQL - Entity Framework Core
        services.AddDbContext<AppDbContext>((serviceProvider, options) =>
        {
            var dbOptions = configuration.GetSection(DatabaseOptions.SectionName).Get<DatabaseOptions>();

            if (dbOptions is null || string.IsNullOrWhiteSpace(dbOptions.ConnectionString))
            {
                throw new InvalidOperationException(
                    "Database configuration is missing or invalid. " +
                    "Check appsettings.json for 'Database:ConnectionString' section.");
            }

            options.UseNpgsql(dbOptions.ConnectionString, npgsqlOptions =>
            {
                npgsqlOptions.EnableRetryOnFailure(
                    maxRetryCount: dbOptions.MaxRetryCount,
                    maxRetryDelay: TimeSpan.FromSeconds(dbOptions.MaxRetryDelaySeconds),
                    errorCodesToAdd: null);

                npgsqlOptions.CommandTimeout(dbOptions.CommandTimeoutSeconds);
                npgsqlOptions.MigrationsAssembly(typeof(AppDbContext).Assembly.FullName);
            });

            options.EnableSensitiveDataLogging(dbOptions.EnableSensitiveDataLogging);
            options.EnableDetailedErrors(dbOptions.EnableDetailedErrors);

            // Log SQL queries no console (apenas para debug)
            options.LogTo(Console.WriteLine, Microsoft.Extensions.Logging.LogLevel.Information);

            // Suprimir warning sobre modelo não-determinístico (causado por HasData com DateTime estático)
            options.ConfigureWarnings(warnings =>
                warnings.Ignore(Microsoft.EntityFrameworkCore.Diagnostics.RelationalEventId.PendingModelChangesWarning));
        });

        // Redis - Cache Distribuído
        services.AddSingleton<IConnectionMultiplexer>(sp =>
        {
            var redisOptions = configuration.GetSection(RedisOptions.SectionName).Get<RedisOptions>()
                ?? new RedisOptions();

            var configOptions = new ConfigurationOptions
            {
                EndPoints = { redisOptions.ConnectionString },
                Password = redisOptions.Password,
                AbortOnConnectFail = redisOptions.AbortOnConnectFail,
                ConnectTimeout = redisOptions.ConnectTimeoutMs,
                SyncTimeout = redisOptions.SyncTimeoutMs,
                AsyncTimeout = redisOptions.AsyncTimeoutMs
            };

            return ConnectionMultiplexer.Connect(configOptions);
        });

        services.AddSingleton<ICacheService, RedisCacheService>();

        // Kafka - Message Bus
        services.AddSingleton<IMessageBus, KafkaProducer>();

        // Repositories
        services.AddScoped<IAuctionRepository, AuctionRepository>();
        services.AddScoped<IBidRepository, BidRepository>();
        services.AddScoped<IUserRepository, UserRepository>();
        services.AddScoped<ICategoryRepository, CategoryRepository>();

        // Unit of Work
        services.AddScoped<IUnitOfWork, UnitOfWork>();

        return services;
    }

    /// <summary>
    /// Adiciona Kafka Consumers como Background Services
    /// </summary>
    public static IServiceCollection AddKafkaConsumers(this IServiceCollection services)
    {
        // Registrar consumers como Hosted Services (BackgroundService)
        services.AddHostedService<AuctionCancelledEventConsumer>();
        services.AddHostedService<BidPlacementRequestedConsumer>();

        // Adicionar outros consumers aqui conforme necessário
        // services.AddHostedService<AuctionCreatedEventConsumer>();
        // services.AddHostedService<BidPlacedEventConsumer>();

        return services;
    }

    /// <summary>
    /// Adiciona SignalR para notificações em tempo real
    /// </summary>
    public static IServiceCollection AddRealtimeNotifications(
        this IServiceCollection services)
    {
        services.AddSignalR(options =>
        {
            options.EnableDetailedErrors = false;
            options.HandshakeTimeout = TimeSpan.FromSeconds(30);
            options.KeepAliveInterval = TimeSpan.FromSeconds(15);
        });

        return services;
    }

    /// <summary>
    /// Adiciona Health Checks para infraestrutura
    /// </summary>
    public static IServiceCollection AddInfrastructureHealthChecks(
        this IServiceCollection services,
        IConfiguration configuration)
    {
        var dbOptions = configuration.GetSection(DatabaseOptions.SectionName).Get<DatabaseOptions>()
            ?? new DatabaseOptions();
        var redisOptions = configuration.GetSection(RedisOptions.SectionName).Get<RedisOptions>()
            ?? new RedisOptions();

        services.AddHealthChecks()
            .AddNpgSql(
                dbOptions.ConnectionString,
                name: "postgresql",
                timeout: TimeSpan.FromSeconds(5),
                tags: new[] { "db", "ready" })
            .AddRedis(
                $"{redisOptions.ConnectionString},password={redisOptions.Password}",
                name: "redis",
                timeout: TimeSpan.FromSeconds(5),
                tags: new[] { "cache", "ready" });

        return services;
    }
}
