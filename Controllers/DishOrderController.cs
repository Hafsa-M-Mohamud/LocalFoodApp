using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Assignment3BAD.Database;
using Assignment3BAD.Models;
using Microsoft.Extensions.Logging;
using System;
using System.Security.Claims;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace Assignment3BAD.Controllers
{
[Route("api/[controller]")]
[ApiController]
public class DishOrderController : ControllerBase
{
    private readonly MyDBContext _context;
    private readonly ILogger<DishOrderController> _logger;

    public DishOrderController(MyDBContext context, ILogger<DishOrderController> logger)
    {
        _context = context;
        _logger = logger;
    }

        // GET: api/dishorder
        [HttpGet]
        [Authorize(Roles = "Admin")] // Only Admin can access this endpoint
        public async Task<ActionResult<IEnumerable<DishOrder>>> GetDishOrders()
        {
            return await _context
                .DishOrders.Include(d => d.Dish) // Include related Dish information if needed
                .Include(d => d.Order) // Include related Order information if needed
                .ToListAsync();
        }

        // GET: api/dishorder/5
        [HttpGet("{id}")]
        [Authorize(Roles = "Admin")] // Only Admin can access this endpoint
        public async Task<ActionResult<DishOrder>> GetDishOrder(int id)
        {
            var dishOrder = await _context
                .DishOrders.Include(d => d.Dish) // Include related Dish information
                .Include(d => d.Order) // Include related Order information
                .FirstOrDefaultAsync(d => d.DishOrderID == id);

            if (dishOrder == null)
            {
                return NotFound();
            }

            return dishOrder;
        }

    // POST: api/dishorder
    [HttpPost]
    public async Task<ActionResult<DishOrder>> PostDishOrder(DishOrder dishOrder)
    {
        try
        {
            if (!ModelState.IsValid)
            {
                _logger.LogWarning(
                    "Ugyldig DishOrder data modtaget: {ValidationDetails}", 
                    new { 
                        Operation = "POST",
                        EntityType = "DishOrder",
                        UserId = User.FindFirstValue(ClaimTypes.NameIdentifier),
                        UserName = User.Identity?.Name,
                        Timestamp = DateTime.UtcNow,
                        ModelStateErrors = ModelState.Values
                            .SelectMany(v => v.Errors)
                            .Select(e => e.ErrorMessage)
                    });
                return BadRequest(ModelState);
            }

            _logger.LogInformation(
                "Oprettelse af ny DishOrder: {DishOrderDetails}", 
                new { 
                    Operation = "POST",
                    EntityType = "DishOrder",
                    UserId = User.FindFirstValue(ClaimTypes.NameIdentifier),
                    UserName = User.Identity?.Name,
                    Timestamp = DateTime.UtcNow,
                    DishId = dishOrder.DishID,
                    OrderId = dishOrder.OrderID,
                    Quantity = dishOrder.Quantity
                });

            _context.DishOrders.Add(dishOrder);
            await _context.SaveChangesAsync();

            _logger.LogInformation(
                "DishOrder oprettet succesfuldt: {DishOrderResult}", 
                new { 
                    Operation = "POST",
                    EntityType = "DishOrder",
                    UserId = User.FindFirstValue(ClaimTypes.NameIdentifier),
                    UserName = User.Identity?.Name,
                    Timestamp = DateTime.UtcNow,
                    DishOrderId = dishOrder.DishOrderID
                });

            return CreatedAtAction(nameof(GetDishOrder), new { id = dishOrder.DishOrderID }, dishOrder);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, 
                "Fejl ved oprettelse af DishOrder: {ErrorDetails}", 
                new { 
                    Operation = "POST",
                    EntityType = "DishOrder",
                    UserId = User.FindFirstValue(ClaimTypes.NameIdentifier),
                    UserName = User.Identity?.Name,
                    Timestamp = DateTime.UtcNow,
                    ErrorMessage = ex.Message
                });
            throw;
        }
    }

    // PUT: api/dishorder/5
    [HttpPut("{id}")]
    public async Task<IActionResult> PutDishOrder(int id, DishOrder dishOrder)
    {
        try
        {
            if (id != dishOrder.DishOrderID)
            {
                _logger.LogWarning(
                    "ID mismatch ved opdatering af DishOrder: {Details}", 
                    new { 
                        Operation = "PUT",
                        EntityType = "DishOrder",
                        UserId = User.FindFirstValue(ClaimTypes.NameIdentifier),
                        UserName = User.Identity?.Name,
                        Timestamp = DateTime.UtcNow,
                        RequestedId = id,
                        ProvidedId = dishOrder.DishOrderID
                    });
                return BadRequest("ID in URL does not match ID in body.");
            }

            var existingDishOrder = await _context.DishOrders.FindAsync(id);
            if (existingDishOrder == null)
            {
                _logger.LogWarning(
                    "Forsøg på at opdatere ikke-eksisterende DishOrder: {Details}", 
                    new { 
                        Operation = "PUT",
                        EntityType = "DishOrder",
                        UserId = User.FindFirstValue(ClaimTypes.NameIdentifier),
                        UserName = User.Identity?.Name,
                        Timestamp = DateTime.UtcNow,
                        DishOrderId = id
                    });
                return NotFound();
            }

            _logger.LogInformation(
                "Opdatering af DishOrder: {Details}", 
                new { 
                    Operation = "PUT",
                    EntityType = "DishOrder",
                    UserId = User.FindFirstValue(ClaimTypes.NameIdentifier),
                    UserName = User.Identity?.Name,
                    Timestamp = DateTime.UtcNow,
                    DishOrderId = id,
                    OldQuantity = existingDishOrder.Quantity,
                    NewQuantity = dishOrder.Quantity,
                    OldDishId = existingDishOrder.DishID,
                    NewDishId = dishOrder.DishID,
                    OldOrderId = existingDishOrder.OrderID,
                    NewOrderId = dishOrder.OrderID
                });

            existingDishOrder.Quantity = dishOrder.Quantity;
            existingDishOrder.DishID = dishOrder.DishID;
            existingDishOrder.OrderID = dishOrder.OrderID;

            await _context.SaveChangesAsync();

            _logger.LogInformation(
                "DishOrder opdateret succesfuldt: {Result}", 
                new { 
                    Operation = "PUT",
                    EntityType = "DishOrder",
                    UserId = User.FindFirstValue(ClaimTypes.NameIdentifier),
                    UserName = User.Identity?.Name,
                    Timestamp = DateTime.UtcNow,
                    DishOrderId = id
                });

            return NoContent();
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, 
                "Fejl ved opdatering af DishOrder: {ErrorDetails}", 
                new { 
                    Operation = "PUT",
                    EntityType = "DishOrder",
                    UserId = User.FindFirstValue(ClaimTypes.NameIdentifier),
                    UserName = User.Identity?.Name,
                    Timestamp = DateTime.UtcNow,
                    DishOrderId = id,
                    ErrorMessage = ex.Message
                });
            throw;
        }
    }

    // DELETE: api/dishorder/5
    [HttpDelete("{id}")]
    public async Task<IActionResult> DeleteDishOrder(int id)
    {
        try
        {
            var dishOrder = await _context.DishOrders.FindAsync(id);
            if (dishOrder == null)
            {
                _logger.LogWarning(
                    "Forsøg på at slette ikke-eksisterende DishOrder: {Details}", 
                    new { 
                        Operation = "DELETE",
                        EntityType = "DishOrder",
                        UserId = User.FindFirstValue(ClaimTypes.NameIdentifier),
                        UserName = User.Identity?.Name,
                        Timestamp = DateTime.UtcNow,
                        DishOrderId = id
                    });
                return NotFound();
            }

            _logger.LogInformation(
                "Sletning af DishOrder påbegyndt: {Details}", 
                new { 
                    Operation = "DELETE",
                    EntityType = "DishOrder",
                    UserId = User.FindFirstValue(ClaimTypes.NameIdentifier),
                    UserName = User.Identity?.Name,
                    Timestamp = DateTime.UtcNow,
                    DishOrderId = id,
                    DishId = dishOrder.DishID,
                    OrderId = dishOrder.OrderID,
                    Quantity = dishOrder.Quantity
                });

            _context.DishOrders.Remove(dishOrder);
            await _context.SaveChangesAsync();

            _logger.LogInformation(
                "DishOrder slettet succesfuldt: {Result}", 
                new { 
                    Operation = "DELETE",
                    EntityType = "DishOrder",
                    UserId = User.FindFirstValue(ClaimTypes.NameIdentifier),
                    UserName = User.Identity?.Name,
                    Timestamp = DateTime.UtcNow,
                    DishOrderId = id
                });

            return NoContent();
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, 
                "Fejl ved sletning af DishOrder: {ErrorDetails}", 
                new { 
                    Operation = "DELETE",
                    EntityType = "DishOrder",
                    UserId = User.FindFirstValue(ClaimTypes.NameIdentifier),
                    UserName = User.Identity?.Name,
                    Timestamp = DateTime.UtcNow,
                    DishOrderId = id,
                    ErrorMessage = ex.Message
                });
            throw;
        }
    }

}
}
