using System.ComponentModel.DataAnnotations;

namespace LogiTrack.Models
{
    public class Order
    {
        [Key]
        public int OrderId { get; set; }

        [Required]
        [StringLength(100)]
        [RegularExpression(@"^\p{L}[\p{L}\p{N}\s]*$", ErrorMessage = "Customer name can only contain letters and numbers")]
        public required string CustomerName { get; set; }

        [Required]
        [DataType(DataType.Date)]
        public DateTime DatePlaced { get; set; }
        public List<OrderItem> OrderItems { get; set; } = [];

        public void AddItem(InventoryItem item, int Quantity)
        {
            ArgumentNullException.ThrowIfNull(item);

            var orderItem = new OrderItem
            {
                Order = this,
                OrderId = this.OrderId,
                InventoryItem = item,
                InventoryItemId = item.ItemId,
                Quantity = Quantity
            };

            OrderItems.Add(orderItem);
        }

        public bool RemoveItem(int itemId)
        {
            if (itemId <= 0) return false;

            var itemToRemove = OrderItems.FirstOrDefault(item => item.InventoryItemId == itemId);
            if (itemToRemove == null) return false;

            return OrderItems.Remove(itemToRemove);
        }

        public string GetOrderSummary()
        {
            int totalQuantity = OrderItems.Sum(i => i.Quantity);
            string itemNames = string.Join(", ", OrderItems.Select(i => i.InventoryItem.Name).Take(3));

            if (OrderItems.Count > 3)
                itemNames += "...";

            return $"Order #{OrderId} | Customer: {CustomerName} | Items: {OrderItems.Count} (Qty: {totalQuantity}) | " +
                $"Products: {itemNames} | Placed: {DatePlaced:M/d/yyyy}";
        }
    }
}