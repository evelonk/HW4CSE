using Microsoft.EntityFrameworkCore;
using PaymentsService.BackgroundServices;
using PaymentsService.Infrastructure;
using PaymentsService.Infrastructure.Messaging;

namespace PaymentsService;

public class Program
{
    public static async Task Main(string[] args)
    {
        var builder = WebApplication.CreateBuilder(args);
        builder.Services.AddControllers();
        builder.Services.AddEndpointsApiExplorer();
        builder.Services.AddSwaggerGen();
        builder.Services.AddDbContext<PaymentsDbContext>(options =>
            options.UseNpgsql(
                builder.Configuration.GetConnectionString("PaymentsDb")));
        builder.Services.AddHostedService<OrderCreatedConsumer>();
        builder.Services.AddHostedService<OutboxProcessor>();
        builder.Services.AddSingleton<RabbitMqPublisher>(sp =>
        {
            return RabbitMqPublisher.CreateAsync()
                .GetAwaiter()
                .GetResult();
        });

        builder.Services.Configure<HostOptions>(options =>
        {
            options.BackgroundServiceExceptionBehavior =
                BackgroundServiceExceptionBehavior.Ignore;
        });

        var app = builder.Build();
        app.UseSwagger();
        app.UseSwaggerUI();
        app.MapControllers();
        await ApplyMigrationsAsync(app);
        await app.RunAsync();
    }

    private static async Task ApplyMigrationsAsync(WebApplication app)
    {
        using var scope = app.Services.CreateScope();
        var db = scope.ServiceProvider.GetRequiredService<PaymentsDbContext>();

        const int maxRetries = 5;
        var delay = TimeSpan.FromSeconds(3);

        for (var attempt = 1; attempt <= maxRetries; attempt++)
        {
            try
            {
                await db.Database.MigrateAsync();
                Console.WriteLine("Database migrations applied");
                return;
            }
            catch (Exception ex)
            {
                Console.WriteLine(
                    $"Database not ready (attempt {attempt}/{maxRetries}): {ex.Message}");

                if (attempt == maxRetries)
                    throw;

                await Task.Delay(delay);
            }
        }
    }
}
