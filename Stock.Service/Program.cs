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
    options.UseSqlServer(builder.Configuration.GetConnectionString("MSSQLServer"))
);

builder.Services.AddMassTransit(config =>
{
    config.AddConsumer<OrderCreatedEventConsumer>();

    config.UsingRabbitMq((context, cfg) =>
    {
        cfg.Host(builder.Configuration["RabbitMQ"]);

        cfg.ReceiveEndpoint(RabbitMQSettings.Stock_OrderCreatedEvent, e =>
        {
            e.ConfigureConsumer<OrderCreatedEventConsumer>(context);
        });
    });
});

var host = builder.Build();
host.Run();
