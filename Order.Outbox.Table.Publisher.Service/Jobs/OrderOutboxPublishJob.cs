using MassTransit;
using MassTransit.Transports;
using Order.Outbox.Table.Publisher.Service.Entities;
using Quartz;
using Shared;
using System.Text.Json;

namespace Order.Outbox.Table.Publisher.Service.Jobs
{
    public class OrderOutboxPublishJob(IPublishEndpoint publishEndpoint) : IJob
    {
        public async Task Execute(IJobExecutionContext context)
        {
            if (OrderOutboxSingletonDatabase.DataReaderState)
            {
                OrderOutboxSingletonDatabase.DataReaderBusy();

                List<OrderOutbox> orderOutboxes = (await OrderOutboxSingletonDatabase.QueryAsync<OrderOutbox>
                    ($@"SELECT * FROM ORDEROUTBOXES Where ProcessedDate IS NULL ORDER BY OCCUREDON ASC")).ToList();

                foreach (var orderOutbox in orderOutboxes)
                {
                    if (orderOutbox.Type == nameof(OrderCreatedEvent))
                    {
                        OrderCreatedEvent @event = JsonSerializer.Deserialize<OrderCreatedEvent>(orderOutbox.Payload);
                        if (@event != null)
                        {
                            await publishEndpoint.Publish(@event);
                            OrderOutboxSingletonDatabase.ExecuteAsync($"UPDATE  ORDEROUTBOXES SET ProcessedDate = GETDATE() WHERE ID ={orderOutbox.Id}");
                        }
                    }
                }

                OrderOutboxSingletonDatabase.DataReaderReady();
                await Console.Out.WriteLineAsync("Order outbox table checked!");
            }
        }
    }
}
