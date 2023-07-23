namespace Inventory.Request
{
    public class ProductRequest
    {
        public string Name { get; set; }
        public string Category { get; set; }
        public decimal Amount { get; set; }
        public string Description { get; set; }
        public decimal BaseDiscountInPercentage { get; set; }
    }
}
