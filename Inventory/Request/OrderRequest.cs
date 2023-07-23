namespace Inventory.Request
{
    public class OrderRequest
    {
        public string CustomerPhone { get; set; }
        public string Note { get; set; }
        public List<OrderItemRequest> Items { get; set; }
    }

    public class OrderItemRequest
    {
        public int ProductId { get; set; }
        public int Quantity { get; set; }
    }
    public class UpdateOrderRequest
    {
        public string CustomerPhone { get; set; }
        public string Note { get; set; }
    }


}
