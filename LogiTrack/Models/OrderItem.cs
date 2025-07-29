using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Text.Json.Serialization;

namespace LogiTrack.Models
{
    public class OrderItem
    {
        [Key]
        public int OrderItemId { get; set; }
        public int OrderId { get; set; }
        public int InventoryItemId { get; set; }

        [Required]
        public int Quantity { get; set; }

        [JsonIgnore]
        [ForeignKey("OrderId")]
        public Order Order { get; set; }

        [JsonIgnore]
        [ForeignKey("InventoryItemId")]
        public InventoryItem InventoryItem { get; set; }
    }
}