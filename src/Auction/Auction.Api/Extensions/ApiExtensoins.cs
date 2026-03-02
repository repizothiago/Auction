using Scalar.AspNetCore;

namespace Auction.Api.Extensions;

public static class ApiExtensions
{
    public static WebApplication UseDevelopmentMiddleware(this WebApplication app)
    {
        app.MapOpenApi();
        //app.UseSwaggerUI(options =>
        //{
        //    options.SwaggerEndpoint("/openapi/v1.json", "Auction API v1");
        //});
        app.MapScalarApiReference();
        return app;
    }

    public static IServiceCollection AddApiExtensions(this IServiceCollection services)
    {
        services.AddEndpointsApiExplorer();
        services.AddSwaggerGen();

        return services;    
    }
}
