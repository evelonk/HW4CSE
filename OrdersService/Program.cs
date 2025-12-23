using Microsoft.EntityFrameworkCore;
using OrdersService.BackgroundServices;
using OrdersService.Infrastructure;

namespace OrdersService
{
    public class Program
    {
        public static void Main(string[] args)
        {
            var builder = WebApplication.CreateBuilder(args);

            builder.Services.AddControllers();
            builder.Services.AddEndpointsApiExplorer();
            builder.Services.AddSwaggerGen();
            builder.Services.AddHostedService<OutboxProcessor>();
            builder.Services.AddHostedService<PaymentCompletedConsumer>();
            builder.Services.AddHostedService<PaymentFailedConsumer>();
            builder.Services.AddDbContext<OrdersDbContext>(options =>
                options.UseNpgsql(builder.Configuration.GetConnectionString("OrdersDb")));

            var app = builder.Build();

            app.UseSwagger();
            app.UseSwaggerUI();
            app.MapControllers();
            using (var scope = app.Services.CreateScope())
            {
                var db = scope.ServiceProvider.GetRequiredService<OrdersDbContext>();

                var retries = 5;
                while (retries > 0)
                {
                    try
                    {
                        db.Database.Migrate();
                        break;
                    }
                    catch (Exception ex)
                    {
                        retries--;
                        Console.WriteLine($"Database not ready, retrying... ({retries})");
                        Thread.Sleep(3000);

                        if (retries == 0)
                            throw;
                    }
                }
            }

            app.Run();
        }
    }
}
