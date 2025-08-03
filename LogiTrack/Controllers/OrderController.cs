using LogiTrack.Context;
using LogiTrack.DTO;
using LogiTrack.Models;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace LogiTrack.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class OrderController : ControllerBase
    {
        private readonly LogiTrackContext _context;

        public OrderController(LogiTrackContext context)
        {
            _context = context;
        }

        [Authorize]
        [HttpGet]
        public async Task<IActionResult> GetAllOrders()
        {
            var allItem = await _context.Orders
                .Include(o => o.OrderItems)
                .ThenInclude(oi => oi.InventoryItem)
                .ToListAsync();

            if (allItem == null || !allItem.Any())
                return NotFound("No orders found.");

            return Ok(allItem);
        }

        [Authorize]
        [HttpGet("allitems")]
        public async Task<IActionResult> GetAllItemFromAllOrders()
        {
            var allitem = await _context.OrderItems.ToListAsync();

            if (allitem == null || !allitem.Any())
                return NotFound("No item found.");

            return Ok(allitem);
        }

        [Authorize]
        [HttpGet("{id}")]
        public async Task<IActionResult> GetOrderById(int id)
        {
            var order = await _context.Orders
                .Include(o => o.OrderItems)
                .ThenInclude(oi => oi.InventoryItem)
                .FirstOrDefaultAsync(o => o.OrderId == id);

            if (order == null)
                return NotFound($"Order with ID {id} not found.");

            return Ok(order);
        }

        [Authorize]
        [HttpPost]
        public async Task<IActionResult> CreateOrder([FromBody] OrderCreateDto newOrder)
        {
            if (newOrder == null)
                return BadRequest("Invalid order data.");

            var order = new Order
            {
                CustomerName = newOrder.CustomerName,
                DatePlaced = newOrder.OrderDate ?? DateTime.UtcNow,
                OrderItems = new List<OrderItem>()
            };

            foreach (var itemDto in newOrder.Items)
            {
                var item = await _context.InventoryItems.FirstOrDefaultAsync(i => i.ItemId == itemDto.ItemId);
                if (item == null)
                    return NotFound($"Inventory item with ID {itemDto.ItemId} not found.");

                if (item.Quantity < itemDto.Quantity)
                    return BadRequest($"Insufficient quantity for item {item.Name}. Available: {item.Quantity}, Requested: {itemDto.Quantity}");

                item.Quantity -= itemDto.Quantity; // Deduct the quantity from inventory
                order.AddItem(item, itemDto.Quantity); // Add the item to the order
            }

            await _context.Orders.AddAsync(order);
            await _context.SaveChangesAsync();

            return CreatedAtAction(nameof(GetOrderById), new { id = order.OrderId }, order);
        }

        [Authorize]
        [HttpDelete("{id}")]
        public async Task<IActionResult> DeleteOrder(int id)
        {
            var order = await _context.Orders.FindAsync(id);

            if (order == null)
                return NotFound($"Order with ID {id} not found.");

            _context.Orders.Remove(order);
            await _context.SaveChangesAsync();

            return NoContent();
        }
    }
}