using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Text.Json.Serialization;

namespace LogiTrack.Models
{
    public class InventoryItem
    {
        [Key]
        public int ItemId { get; set; }

        [Required]
        [StringLength(100)]
        [RegularExpression(@"^\p{L}[\p{L}\p{N}\s]*$", ErrorMessage = "Item name can only contain letters and numbers")]
        public required string Name { get; set; }

        [Required]
        public int Quantity { get; set; }
        public string? Location { get; set; }

        [JsonIgnore]
        public List<OrderItem> OrderItems { get; set; } = [];

        public void DisplayInfo()
        {
            var message = $"Item: {this.Name} | Quanity: {this.Quantity} | Location: {this.Location}";
            Console.WriteLine(message);
        }
    }
}