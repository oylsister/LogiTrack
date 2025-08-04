using LogiTrack.Context;
using LogiTrack.DTO;
using LogiTrack.Models;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Caching.Memory;

namespace LogiTrack.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class OrderController : ControllerBase
    {
        private readonly LogiTrackContext _context;
        private readonly IMemoryCache _cache;

        public OrderController(LogiTrackContext context, IMemoryCache cache)
        {
            _context = context;
            _cache = cache;
        }

        [Authorize]
        [HttpGet]
        public async Task<IActionResult> GetAllOrders()
        {
            if (_cache.TryGetValue("AllOrders", out List<Order>? cachedOrders))
            {
                if (cachedOrders != null)
                {
                    //Console.WriteLine("Found in cache");
                    return Ok(cachedOrders);
                }
            }

            var allItem = await _context.Orders
                .AsNoTracking()
                .Include(o => o.OrderItems)
                .ThenInclude(oi => oi.InventoryItem)
                .ToListAsync();

            if (allItem == null || !allItem.Any())
                return NotFound("No orders found.");

            // Cache the result for 5 minutes
            var cacheOptions = new MemoryCacheEntryOptions()
                .SetAbsoluteExpiration(TimeSpan.FromMinutes(15))
                .SetPriority(CacheItemPriority.Normal);
                
            _cache.Set("AllOrders", allItem, cacheOptions);

            //Console.WriteLine("Directly");

            return Ok(allItem);
        }

        [Authorize]
        [HttpGet("allitems")]
        public async Task<IActionResult> GetAllItemFromAllOrders()
        {
            var allitem = await _context.OrderItems.AsNoTracking().ToListAsync();

            if (allitem == null || !allitem.Any())
                return NotFound("No item found.");

            return Ok(allitem);
        }

        [Authorize]
        [HttpGet("{id}")]
        public async Task<IActionResult> GetOrderById(int id)
        {
            if (_cache.TryGetValue("AllOrders", out List<Order>? cachedOrders))
            {
                if (cachedOrders != null)
                {
                    var cacheOrder = cachedOrders.Where(o => o.OrderId == id).FirstOrDefault();
                    if (cacheOrder != null)
                    {
                        //Console.WriteLine("From Cache");
                        return Ok(cacheOrder);
                    }
                }
            }

            var order = await _context.Orders
                .AsNoTracking()
                .Include(o => o.OrderItems)
                .ThenInclude(oi => oi.InventoryItem)
                .FirstOrDefaultAsync(o => o.OrderId == id);

            if (order == null)
                return NotFound($"Order with ID {id} not found.");

            //Console.WriteLine("This is from directly");

            return Ok(order);
        }

        [Authorize]
        [HttpPost]
        public async Task<IActionResult> CreateOrder([FromBody] OrderCreateDto newOrder)
        {
            if (newOrder == null)
                return BadRequest("Invalid order data.");

            // Extract all item IDs from the order
            var itemIds = newOrder.Items.Select(i => i.ItemId).ToList();
            
            // Fetch all required items in a single database query
            var inventoryItems = await _context.InventoryItems
                .Where(i => itemIds.Contains(i.ItemId))
                .ToDictionaryAsync(i => i.ItemId, i => i);
            
            // Check if all requested items exist
            foreach (var itemDto in newOrder.Items)
            {
                if (!inventoryItems.TryGetValue(itemDto.ItemId, out var item))
                    return NotFound($"Inventory item with ID {itemDto.ItemId} not found.");
                    
                if (item.Quantity < itemDto.Quantity)
                    return BadRequest($"Insufficient quantity for item {item.Name}. Available: {item.Quantity}, Requested: {itemDto.Quantity}");
                    
                // Update inventory quantity
                item.Quantity -= itemDto.Quantity;
            }
            
            var order = new Order
            {
                CustomerName = newOrder.CustomerName,
                DatePlaced = newOrder.OrderDate ?? DateTime.UtcNow,
                OrderItems = new List<OrderItem>()
            };
            
            // Add items to order
            foreach (var itemDto in newOrder.Items)
            {
                var item = inventoryItems[itemDto.ItemId];
                order.AddItem(item, itemDto.Quantity);
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