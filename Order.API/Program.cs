using MassTransit;
using Microsoft.EntityFrameworkCore;
using Order.API.Models;
using Order.API.Models.Contexts;
using Order.API.Models.Entities;
using Shared;
using System.Text.Json;

var builder = WebApplication.CreateBuilder(args);

builder.Services.AddControllers();

builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();

builder.Services.AddDbContext<OrderDbContext>(options => options.UseSqlServer(builder.Configuration.GetConnectionString("MSSQLServer")));

var app = builder.Build();
app.UseSwagger();
app.UseSwaggerUI();

app.UseHttpsRedirection();

app.UseAuthorization();

app.MapControllers();

app.UseSwaggerUI();

app.MapPost("/create-order", async (CreateOrderVM model, OrderDbContext orderDbContext) =>
{
    Order.API.Models.Entities.Order order = new()
    {
        BuyerId = model.BuyerId,
        CreatedDate = DateTime.UtcNow,
        TotalPrice = model.OrderItems.Sum(oi => oi.Count * oi.Price),
        OrderItems = model.OrderItems.Select(oi => new Order.API.Models.Entities.OrderItem
        {
            Price = oi.Price,
            Count = oi.Count,
            ProductId = oi.ProductId,
        }).ToList(),
    };

    await orderDbContext.Orders.AddAsync(order);
    await orderDbContext.SaveChangesAsync();

    var idempotentToken = Guid.NewGuid();

    OrderCreatedEvent @event = new()
    {
        BuyerId = order.BuyerId,
        OrderId = order.Id,
        TotalPrice = order.TotalPrice,
        OrderItems = order.OrderItems.Select(oi => new Shared.OrderItem
        {
            Count = oi.Count,
            ProductId = oi.ProductId,
            Price = oi.Price
        }).ToList(),
        IdempotentToken = idempotentToken,
    };

    OrderOutbox orderOutbox = new OrderOutbox()
    {
        OccuredOn = DateTime.Now,
        ProcessedDate = null,
        Payload = JsonSerializer.Serialize(@event),
        Type = @event.GetType().Name,
        IdempotentToken = idempotentToken
    };

    await orderDbContext.AddAsync(orderOutbox);
    await orderDbContext.SaveChangesAsync();


});
app.Run();
