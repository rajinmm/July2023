using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace Inventory.Repository
{
    public class Product
    {
        [Key]
        public int Id { get; private set; }

        [Required]
        public string Name { get;  set; }

        [Required]
        public string Category { get;  set; }
        public decimal Amount { get;  set; }
        public string Description { get;  set; }     
        public decimal BaseDiscountInPercentage { get;  set; }

        public Product(string name, string category, decimal amount, string description, decimal baseDiscountInPercentage)
        {
            if (string.IsNullOrWhiteSpace(name))
                throw new ArgumentException("The Name field is required.", nameof(name));

            if (string.IsNullOrWhiteSpace(category))
                throw new ArgumentException("The Category field is required.", nameof(category));

            if (amount <= 0)
                throw new ArgumentException("The Amount must be greater than 0.", nameof(amount));

            if (baseDiscountInPercentage < 0 || baseDiscountInPercentage > 100)
                throw new ArgumentException("The BaseDiscountInPercentage must be between 0 and 100.", nameof(baseDiscountInPercentage));

            Name = name;
            Category = category;
            Amount = amount;
            Description = description;
            BaseDiscountInPercentage = baseDiscountInPercentage;
        }

        public void UpdateProduct(string name, string category, decimal amount, string description, decimal baseDiscountInPercentage)
        {
            if (string.IsNullOrWhiteSpace(name))
                throw new ArgumentException("The Name field is required.", nameof(name));

            if (string.IsNullOrWhiteSpace(category))
                throw new ArgumentException("The Category field is required.", nameof(category));

            if (amount <= 0)
                throw new ArgumentException("The Amount must be greater than 0.", nameof(amount));

            if (baseDiscountInPercentage < 0 || baseDiscountInPercentage > 100)
                throw new ArgumentException("The BaseDiscountInPercentage must be between 0 and 100.", nameof(baseDiscountInPercentage));

            Name = name;
            Category = category;
            Amount = amount;
            Description = description;
            BaseDiscountInPercentage = baseDiscountInPercentage;
        }
    }
}
