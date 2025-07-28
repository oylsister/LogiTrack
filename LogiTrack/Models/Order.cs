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
        public List<InventoryItem> ItemList { get; set; } = [];

        public void AddItem(InventoryItem item)
        {
            ArgumentNullException.ThrowIfNull(item);

            item.Order = this;
            item.OrderId = this.OrderId;
            ItemList.Add(item);
        }

        public bool RemoveItem(int itemId)
        {
            if (itemId <= 0) return false;

            var itemToRemove = ItemList.FirstOrDefault(item => item.ItemId == itemId);
            if (itemToRemove == null) return false;

            return ItemList.Remove(itemToRemove);
        }

        public string GetOrderSummary()
        {
            int totalQuantity = ItemList.Sum(i => i.Quantity);
            string itemNames = string.Join(", ", ItemList.Select(i => i.Name).Take(3));

            if (ItemList.Count > 3)
                itemNames += "...";

            return $"Order #{OrderId} | Customer: {CustomerName} | Items: {ItemList.Count} (Qty: {totalQuantity}) | " +
                $"Products: {itemNames} | Placed: {DatePlaced:M/d/yyyy}";
        }
    }
}