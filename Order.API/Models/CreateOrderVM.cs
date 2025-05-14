namespace Order.API.Models
{
    public class CreateOrderVM
    {
        public int BuyerId { get; set; }
        public List<CreateOrderItemVM> OrderItems { get; set; }
    }
    public class CreateOrderItemVM
    {
        public int ProductId { get; set; }
        public int Count { get; set; }
        public decimal Price { get; set; }
    }
}
