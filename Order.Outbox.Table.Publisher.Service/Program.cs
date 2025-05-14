var builder = Host.CreateApplicationBuilder(args);


//builder.Services.AddMassTransit(configurator =>
//{
//    configurator.UsingRabbitMq((context, _configure) =>
//    {
//        _configure.Host(builder.Configuration["RabbitMQ"]);
//    });
//});

//var sendEndpoint = await sendEndpointProvider.GetSendEndpoint(new Uri($"queue:{RabbitMQSettings.Stock_OrderCreatedEvent}"));

//await sendEndpoint.Send<OrderCreatedEvent>(@event);

var host = builder.Build();
host.Run();
