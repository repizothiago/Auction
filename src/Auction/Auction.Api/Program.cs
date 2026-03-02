using Auction.Api.Extensions;

var builder = WebApplication.CreateBuilder(args);

builder.Services.AddControllers();
builder.Services.AddOpenApi();
builder.Services.AddApiExtensions();

var app = builder.Build();

if (app.Environment.IsDevelopment())
{
    app.UseDevelopmentMiddleware();
}

app.UseHttpsRedirection();

app.UseAuthorization();

app.MapControllers();

app.Run();
