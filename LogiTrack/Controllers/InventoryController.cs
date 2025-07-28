using LogiTrack.Context;
using LogiTrack.Models;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace LogiTrack.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class InventoryController : ControllerBase
    {
        private readonly LogiTrackContext _context;

        public InventoryController(LogiTrackContext context)
        {
            _context = context;
        }

        // GET: Inventory
        [HttpGet]
        public async Task<IActionResult> InventoryList()
        {
            var inventoryItems = await _context.InventoryItems.ToListAsync();

            if (inventoryItems == null || !inventoryItems.Any())
                return NotFound("No inventory items found.");

            return Ok(inventoryItems);
        }

        // GET: Inventory/{id}
        [HttpGet("{id}")]
        public async Task<IActionResult> GetInventoryItem(int id)
        {
            var item = await _context.InventoryItems.FirstOrDefaultAsync(i => i.ItemId == id);

            if (item == null)
                return NotFound($"Inventory item with ID {id} not found.");

            return Ok(item);
        }

        [HttpPost]
        public async Task<IActionResult> AddInventoryItem([FromBody] InventoryItem newItem)
        {
            if (newItem == null)
                return BadRequest("Invalid inventory item data.");

            await _context.InventoryItems.AddAsync(newItem);
            await _context.SaveChangesAsync();

            return CreatedAtAction(nameof(InventoryList), new { id = newItem.ItemId }, newItem);
        }

        /*
        [HttpPut("{id}")]
        public async Task<IActionResult> UpdateInventoryItem(int id, [FromBody] InventoryItem updatedItem)
        {
            if (updatedItem == null || updatedItem.ItemId != id)
                return BadRequest("Invalid inventory item data.");

            var existingItem = await _context.InventoryItems.FirstOrDefaultAsync(i => i.ItemId == id);

            if (existingItem == null)
                return NotFound($"Inventory item with ID {id} not found.");

            existingItem.Name = updatedItem.Name;
            existingItem.Quantity = updatedItem.Quantity;
            existingItem.Location = updatedItem.Location;

            _context.InventoryItems.Update(existingItem);
            await _context.SaveChangesAsync();

            return Ok(existingItem);
        }
        */

        [HttpPut("{id:int}")]
        public async Task<IActionResult> UpdateInventoryItem([FromRoute] int id, [FromBody] InventoryItem updatedItem)
        {
            if (updatedItem == null)
                return BadRequest("Invalid inventory item data.");

            // Check if IDs match (if updatedItem has an ID)
            if (updatedItem.ItemId != 0 && updatedItem.ItemId != id)
                return BadRequest($"ID mismatch. Route ID: {id}, Item ID: {updatedItem.ItemId}");

            // Ensure the item exists
            var existingItem = await _context.InventoryItems.FindAsync(id);
            if (existingItem == null)
                return NotFound($"Inventory item with ID {id} not found.");

            // Set the ID explicitly to ensure it matches the route
            updatedItem.ItemId = id;

            try
            {
                // Update properties (only modify the ones you need to change)
                existingItem.Name = updatedItem.Name;
                existingItem.Quantity = updatedItem.Quantity;
                existingItem.Location = updatedItem.Location;
                // Update other properties as needed...

                _context.InventoryItems.Update(existingItem);
                await _context.SaveChangesAsync();

                return Ok(existingItem);
            }
            catch (Exception ex)
            {
                // Log the exception
                return StatusCode(500, $"An error occurred while updating the inventory item: {ex.Message}");
            }
        }

        [HttpDelete("{id}")]
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