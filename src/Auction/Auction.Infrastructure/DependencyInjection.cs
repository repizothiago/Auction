using Auction.Application.Interfaces;
using Auction.Application.Interfaces.Repositories;
using Auction.Infrastructure.Caching;
using Auction.Infrastructure.Messaging;
using Auction.Infrastructure.Persistence;
using Auction.Infrastructure.Persistence.Configurations;
using Auction.Infrastructure.Persistence.Repositories;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using StackExchange.Redis;

namespace Auction.Infrastructure;

public static class DependencyInjection
{
    public static IServiceCollection AddInfrastructure(
        this IServiceCollection services,
        IConfiguration configuration)
    {
        // PostgreSQL - Entity Framework Core
        services.AddDbContext<AppDbContext>(options =>
        {
            var connectionString = configuration.GetConnectionString("DefaultConnection");

            options.UseNpgsql(connectionString, npgsqlOptions =>
            {
                npgsqlOptions.EnableRetryOnFailure(
                    maxRetryCount: 5,
                    maxRetryDelay: TimeSpan.FromSeconds(30),
                    errorCodesToAdd: null);

                npgsqlOptions.CommandTimeout(30);
                npgsqlOptions.MigrationsAssembly(typeof(AppDbContext).Assembly.FullName);
            });

            // Configurações para desenvolvimento
            options.EnableSensitiveDataLogging(false);
            options.EnableDetailedErrors(false);
        });

        // Redis - Cache Distribuído
        services.AddSingleton<IConnectionMultiplexer>(sp =>
        {
            var redisConfiguration = configuration.GetConnectionString("Redis") 
                ?? "localhost:6379";

            var options = ConfigurationOptions.Parse(redisConfiguration);
            options.AbortOnConnectFail = false;
            options.ConnectTimeout = 5000;
            options.SyncTimeout = 5000;
            options.AsyncTimeout = 5000;

            return ConnectionMultiplexer.Connect(options);
        });

        services.AddSingleton<ICacheService, RedisCacheService>();

        // Kafka - Message Bus
        services.AddSingleton<IMessageBus, KafkaProducer>();

        // Repositories
        services.AddScoped<IAuctionRepository, AuctionRepository>();
        services.AddScoped<IUserRepository, UserRepository>();
        services.AddScoped<ICategoryRepository, CategoryRepository>();

        // Unit of Work
        services.AddScoped<IUnitOfWork, UnitOfWork>();

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
            options.ClientTimeoutInterval = TimeSpan.FromSeconds(30);
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
        services.AddHealthChecks()
            .AddNpgSql(
                configuration.GetConnectionString("DefaultConnection")!,
                name: "postgresql",
                timeout: TimeSpan.FromSeconds(5),
                tags: new[] { "db", "ready" })
            .AddRedis(
                configuration.GetConnectionString("Redis") ?? "localhost:6379",
                name: "redis",
                timeout: TimeSpan.FromSeconds(5),
                tags: new[] { "cache", "ready" });

        return services;
    }
}
