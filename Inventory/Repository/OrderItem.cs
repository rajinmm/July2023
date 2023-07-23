using System.ComponentModel.DataAnnotations;

namespace Inventory.Repository
{
    public class OrderItem
    {
        [Key]
        public int OrderItemId { get; set; }

        [Required]
        public int OrdersId { get; set; }

        [Required]
        public int ProductId { get; set; }

        [Required]
        public int Quantity { get; set; }
        public decimal NetAmount { get; set; }
        public decimal ActualAmount { get; set; }
        public virtual Orders Orders { get; set; }

        public virtual Product Product { get; set; }
        public void AddOrderItem(int ordersId, int productId, int quantity, decimal netAmount, decimal actualAmount)
        {
            if (productId <= 0)
                throw new ArgumentException("There is product with product Id 0 ", nameof(ProductId));
            if (quantity <= 0)
                throw new ArgumentException("The Quantity must be greater than 0.", nameof(Quantity));

            OrdersId = ordersId;
            ProductId = productId;
            Quantity = quantity;
            NetAmount = netAmount;
            ActualAmount = actualAmount;
          
        }
    }
}
