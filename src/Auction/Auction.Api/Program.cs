using Auction.Api.Extensions;
using Auction.Application;
using Auction.Infrastructure;

try
{
    var builder = WebApplication.CreateBuilder(args);

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
catch (Exception ex)
{
    Console.Error.WriteLine($"Fatal error during application startup: {ex.Message}");
    Console.Error.WriteLine(ex.StackTrace);
    throw;
}
finally
{
    Console.WriteLine("Application shutdown complete.");
}
