using Auction.Api.Extensions;
using Auction.Application;
using Auction.Infrastructure;

var builder = WebApplication.CreateBuilder(args);

builder.Services.AddControllers();
builder.Services.AddOpenApi();
builder.Services.AddApiExtensions();

// Adicionar camadas da aplicação
builder.Services.AddApplication();
builder.Services.AddInfrastructure(builder.Configuration);
builder.Services.AddInfrastructureHealthChecks(builder.Configuration);
builder.Services.AddKafkaConsumers();

var app = builder.Build();

if (app.Environment.IsDevelopment())
{
    app.UseDevelopmentMiddleware();
}

app.UseHttpsRedirection();
app.UseAuthorization();

app.MapHealthChecks("/health");
app.MapControllers();

app.Run();