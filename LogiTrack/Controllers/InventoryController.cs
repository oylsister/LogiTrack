using LogiTrack.Context;
using LogiTrack.Models;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace LogiTrack.Controllers
{
    [ApiController]
    [Route("[controller]")]
    public class InventoryController : Controller
    {
        private readonly LogiTrackContext _context;

        public InventoryController(LogiTrackContext context)
        {
            _context = context;
        }

        // GET: Inventory
        [HttpGet("/api/inventory")]
        public async Task<IActionResult> InventoryList()
        {
            var inventoryItems = await _context.InventoryItems.ToListAsync();

            if (inventoryItems == null || !inventoryItems.Any())
                return NotFound("No inventory items found.");

            return Ok(inventoryItems);
        }

        [HttpPost("/api/inventory")]
        public async Task<IActionResult> AddInventoryItem([FromBody] InventoryItem newItem)
        {
            if (newItem == null)
                return BadRequest("Invalid inventory item data.");

            await _context.InventoryItems.AddAsync(newItem);
            await _context.SaveChangesAsync();

            return CreatedAtAction(nameof(InventoryList), new { id = newItem.ItemId }, newItem);
        }

        [HttpDelete("/api/inventory/{id}")]
        public async Task<IActionResult> DeleteInventoryItem(int id)
        {
            var item = await _context.InventoryItems.FirstOrDefaultAsync(i => i.ItemId == id);

            if (item == null)
                return NotFound($"Inventory item with ID {id} not found.");

            _context.InventoryItems.Remove(item);
            await _context.SaveChangesAsync();

            return NoContent();
        }
    }
}