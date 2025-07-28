namespace LogiTrack.DTO
{
    public class OrderCreateDto
    {
        public required string CustomerName { get; set; }
        public DateTime? OrderDate { get; set; }
        
        public List<OrderItemCreateDto> Items { get; set; } = new List<OrderItemCreateDto>();
    }

    public class OrderItemCreateDto
    {
        public int ItemId { get; set; }
        public int Quantity { get; set; }
    }
}