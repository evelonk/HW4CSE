using Yarp.ReverseProxy.Configuration;

namespace ApiGateway;

public class Program
{
    public static void Main(string[] args)
    {
        var builder = WebApplication.CreateBuilder(args);
        builder.Services.AddEndpointsApiExplorer();
        builder.Services.AddSwaggerGen();
        builder.Services.AddReverseProxy()
            .LoadFromMemory(
                new[]
                {
                    new RouteConfig
                    {
                        RouteId = "orders-route",
                        ClusterId = "orders-cluster",
                        Match = new RouteMatch
                        {
                            Path = "/api/orders/{**catch-all}"
                        }
                    },
                    new RouteConfig
                    {
                        RouteId = "payments-route",
                        ClusterId = "payments-cluster",
                        Match = new RouteMatch
                        {
                            Path = "/api/payments/{**catch-all}"
                        }
                    }
                },
                new[]
                {
                    new ClusterConfig
                    {
                        ClusterId = "orders-cluster",
                        Destinations = new Dictionary<string, DestinationConfig>
                        {
                            ["orders"] = new()
                            {
                                Address = "http://orders-service/"
                            }
                        }
                    },
                    new ClusterConfig
                    {
                        ClusterId = "payments-cluster",
                        Destinations = new Dictionary<string, DestinationConfig>
                        {
                            ["payments"] = new()
                            {
                                Address = "http://payments-service/"
                            }
                        }
                    }
                }
            );

        var app = builder.Build();
        app.MapGet("/aggregate/orders/swagger.json", async context =>
        {
            using var client = new HttpClient();
            var json = await client.GetStringAsync(
                "http://orders-service/swagger/v1/swagger.json"
            );

            context.Response.ContentType = "application/json";
            await context.Response.WriteAsync(json);
        });

        app.MapGet("/aggregate/payments/swagger.json", async context =>
        {
            using var client = new HttpClient();
            var json = await client.GetStringAsync(
                "http://payments-service/swagger/v1/swagger.json"
            );

            context.Response.ContentType = "application/json";
            await context.Response.WriteAsync(json);
        });
        app.UseSwagger();
        app.UseSwaggerUI(options =>
        {
            options.RoutePrefix = "gw-swagger";

            options.SwaggerEndpoint(
                "/aggregate/orders/swagger.json",
                "Orders Service"
            );

            options.SwaggerEndpoint(
                "/aggregate/payments/swagger.json",
                "Payments Service"
            );
        });
        app.MapReverseProxy();
        app.Run();
    }
}
