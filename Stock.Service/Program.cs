using MassTransit;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Configuration;
using Stock.Service.Consumers;
using Stock.Service.Models.Contexts;
using Shared;

var builder = Host.CreateApplicationBuilder(args);

builder.Services.AddDbContext<StockDbContext>(options =>
    options.UseSqlServer("Server=.;Database=StockDb;Trusted_Connection=True;TrustServerCertificate=True;")
);

builder.Services.AddMassTransit(config =>
{
    config.AddConsumer<OrderCreatedEventConsumer>();

    config.UsingRabbitMq((context, cfg) =>
    {
        cfg.Host("amqp://guest:guest@localhost:5672/");

        cfg.ReceiveEndpoint(RabbitMQSettings.Stock_OrderCreatedEvent, e =>
        {
            e.ConfigureConsumer<OrderCreatedEventConsumer>(context);
        });
    });
});

var host = builder.Build();
host.Run();
