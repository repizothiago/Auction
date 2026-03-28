using Auction.Api.Extensions;
using Auction.Application;
using Auction.Infrastructure;
using Serilog;

Log.Logger = new LoggerConfiguration()
    .MinimumLevel.Debug()
    .WriteTo.Console(outputTemplate: "[{Timestamp:HH:mm:ss} {Level:u3}] {Message:lj}{NewLine}{Exception}")
    .CreateBootstrapLogger();

try
{
    Log.Information("Iniciando aplicação Auction.Api...");

    var builder = WebApplication.CreateBuilder(args);

    builder.Host.UseSerilog((context, services, config) =>
        config
            .ReadFrom.Configuration(context.Configuration)
            .ReadFrom.Services(services)
            .Enrich.FromLogContext());

    builder.Services.AddHttpContextAccessor();

    builder.Services.AddApiServices();
    builder.Services.AddApplication();
    builder.Services.AddInfrastructure(builder.Configuration);
    builder.Services.AddInfrastructureHealthChecks(builder.Configuration);
    builder.Services.AddKafkaConsumers();

    var app = builder.Build();

    await app.SeedDatabaseAsync();

    app.UseApiPipeline();

    app.Run();
}
catch (Exception ex) when (ex is not HostAbortedException)
{
    Log.Fatal(ex, "Erro fatal durante a inicialização da aplicação");
    throw;
}
finally
{
    Log.Information("Aplicação encerrada. Liberando recursos do Serilog...");
    Log.CloseAndFlush();
}
