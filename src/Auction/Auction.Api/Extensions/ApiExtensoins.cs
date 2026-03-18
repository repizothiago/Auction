using Auction.Api.Converters;
using Auction.Api.Middleware;
using Auction.Infrastructure.Utilities;
using Scalar.AspNetCore;

namespace Auction.Api.Extensions;

public static class ApiExtensions
{
    public static IServiceCollection AddApiServices(this IServiceCollection services)
    {
        services.AddControllers()
            .AddJsonOptions(options =>
            {
                options.JsonSerializerOptions.Converters.Add(new MoneyJsonConverter());
            });

        services.AddEndpointsApiExplorer();
        services.AddOpenApi();
        services.AddSwaggerGen();

        return services;
    }

    public static WebApplication UseApiPipeline(this WebApplication app)
    {
        if (app.Environment.IsDevelopment())
        {
            app.UseDevelopmentMiddleware();
        }

        app.UseHttpsRedirection();

        // Converter para IApplicationBuilder para usar o middleware
        ((IApplicationBuilder)app).UseIdempotency();

        app.UseAuthorization();

        app.MapHealthChecks("/health");
        app.MapControllers();

        return app;
    }

    public static async Task<WebApplication> SeedDatabaseAsync(this WebApplication app)
    {
        if (!app.Environment.IsDevelopment())
            return app;

        try
        {
            var configuration = app.Services.GetRequiredService<IConfiguration>();
            var logger = app.Services.GetRequiredService<ILogger<Program>>();

            var connectionString = configuration.GetSection("Database:ConnectionString").Value;

            if (string.IsNullOrWhiteSpace(connectionString))
            {
                logger.LogWarning("Connection string não configurada. Seed ignorado.");
                return app;
            }

            var baseDir = AppDomain.CurrentDomain.BaseDirectory;
            var projectRoot = Path.GetFullPath(Path.Combine(baseDir, "..", "..", "..", ".."));
            var sqlFilePath = Path.Combine(projectRoot, "Auction.Infrastructure", "Migrations", "SeedData.sql");

            if (File.Exists(sqlFilePath))
            {
                await DatabaseSeederUtility.SeedFromSqlFileAsync(connectionString, sqlFilePath);
                logger.LogInformation("✓ Seed data aplicado com sucesso! 10 leilões ativos disponíveis.");
            }
            else
            {
                logger.LogWarning("⚠ Arquivo de seed não encontrado: {SqlFilePath}", sqlFilePath);
            }
        }
        catch (Exception ex)
        {
            var logger = app.Services.GetRequiredService<ILogger<Program>>();
            logger.LogError(ex, "⚠ Erro ao aplicar seed data: {Message}", ex.Message);
        }

        return app;
    }

    private static WebApplication UseDevelopmentMiddleware(this WebApplication app)
    {
        app.MapOpenApi();
        app.MapScalarApiReference();
        return app;
    }
}
