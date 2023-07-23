using System.ComponentModel.DataAnnotations;

namespace Inventory.Repository
{
    public class Orders
    {
        [Key]
        public int OrdersId { get; set; }

        [Required]
        public string CustomerPhone { get; set; }
        public string Note { get; set; }
        public DateTime CreatedOnDatetime { get; set; }
        public string Status { get; set; }

        public List<OrderItem> Items { get; private set; }
        public decimal ActualTotalPrice { get; set; }
        public decimal NetTotalPrice { get; set; }

        public Orders(string customerPhone, string note)
        {
            if (string.IsNullOrWhiteSpace(customerPhone))
                throw new ArgumentException("The Customer Phone field is required.", nameof(CustomerPhone));

            CustomerPhone = customerPhone;
            Note = note;     
            CreatedOnDatetime = DateTime.UtcNow;
            Status = "pending";
        }

        public void UpdateOrder(string customerPhone, string note)
        {
            // Validation and update logic can be added here.
            CustomerPhone = customerPhone;
            Note = note;
        }   

        public bool RemoveOrderItem(int itemId)
        {
            // Logic to remove the specified item can be added here.
            var itemToRemove = Items.FirstOrDefault(item => item.ProductId == itemId);
            if (itemToRemove != null)
            {
                Items.Remove(itemToRemove);
                return true;
            }
            return false;
        }


    }
}
