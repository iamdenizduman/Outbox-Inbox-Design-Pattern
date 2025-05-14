
namespace Shared
{
    public class OrderCreatedEvent
    {
        public int OrderId { get; set; }
        public int BuyerId { get; set; }
        public decimal TotalPrice { get; set; }
        public List<OrderItem> OrderItems { get; set; }
        public Guid IdempotentToken { get; set; }
    }
    public class OrderItem
    {
        public int ProductId { get; set; }
        public int Count { get; set; }
        public decimal Price { get; set; }
    }
}
