using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

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
        public int? OrderId { get; set; }
        
        [ForeignKey("OrderId")]
        public Order? Order { get; set; }        

        public void DisplayInfo()
        {
            var message = $"Item: {this.Name} | Quanity: {this.Quantity} | Location: {this.Location}";
            Console.WriteLine(message);
        }
    }
}