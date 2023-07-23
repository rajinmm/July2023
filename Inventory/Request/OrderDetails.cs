namespace Inventory.Request
{
    public class OrderDetailsResponse
    {
        public int OrderId { get; set; }
        public string CustomerPhone { get; set; }
        public string Note { get; set; }
        public DateTime CreatedOnDatetime { get; set; }
        public string Status { get; set; }
        public List<OrderItemDetailsResponse> Items { get; set; }
        public decimal ActualTotalPrice { get; set; }
        public decimal NetTotalPrice { get; set; }
    }
    public class OrderItemDetailsResponse
    {
        public int ProductId { get; set; }
        public string ProductName { get; set; }
        public int Quantity { get; set; }
        public decimal NetAmount { get; set; }
        public decimal ActualAmount { get; set; }
    }
}
