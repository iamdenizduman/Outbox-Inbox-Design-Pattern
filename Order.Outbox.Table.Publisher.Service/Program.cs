using MassTransit;
using Order.Outbox.Table.Publisher.Service.Jobs;
using Quartz;

var builder = Host.CreateApplicationBuilder(args);

builder.Services.AddMassTransit(configurator =>
{
    configurator.UsingRabbitMq((context, _configure) =>
    {
        _configure.Host(builder.Configuration["RabbitMQ"]);
    });
});

builder.Services.AddQuartz(confg =>
{
    JobKey jobKey = new("OrderOutboxPublishJobKey");
    
    confg.AddJob<OrderOutboxPublishJob>(opt => opt.WithIdentity((jobKey)));

    TriggerKey triggerKey = new("OrderOutboxPublishTrigger");
    confg.AddTrigger(opt => opt.ForJob(jobKey)
    .WithIdentity(triggerKey)
    .StartAt(DateTime.Now)
    .WithSimpleSchedule
    (builder => builder.WithIntervalInSeconds(5).RepeatForever()));

});

builder.Services.AddQuartzHostedService(opt => opt.WaitForJobsToComplete = true);

var host = builder.Build();
host.Run();
