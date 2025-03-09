using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Assignment3BAD.Database;
using Assignment3BAD.Models;
using Microsoft.Extensions.Logging;
using System.Security.Claims;
using System;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace Assignment3BAD.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class DishesController : ControllerBase
    {
        private readonly MyDBContext _context;
        private readonly ILogger<DishesController> _logger;

        public DishesController(MyDBContext context, ILogger<DishesController> logger)
        {
            _context = context;
            _logger = logger;
        }

        // Get all dishes
        [HttpGet]
        public async Task<ActionResult<IEnumerable<Dish>>> GetDishes()
        {
             var Dish = await _context.Dishes
                .Select(d => new 
                {
                    d.DishID,
                    d.Name,
                    d.Quantity,
                    d.Price,
                    d.StartTime,
                    d.EndTime

                })
                .ToListAsync();

            return Ok(Dish);        }

        // Get dish by ID
        [HttpGet("{id}")]
        [Authorize(Roles = "Admin")] // Only Admin can access this endpoint
        public async Task<ActionResult<Dish>> GetDishById(int id)
        {
            var dish = await _context.Dishes.FindAsync(id);
            if (dish == null)
                return NotFound();
            return dish;
        }

        // get dishes on a specific order (dishorders)
        [HttpGet("dishorders/{dishId}")]
        [Authorize(Roles = "Admin")] // Only Admin can access this endpoint
        public async Task<ActionResult<IEnumerable<DishOrder>>> GetDishOrdersByDishId(int dishId)
        {
            var dishOrders = await _context
                .DishOrders.Where(dishOrder => dishOrder.DishID == dishId) // Filter by DishID
                .Include(dishOrder => dishOrder.Order) // Include related Order details
                .ToListAsync();

            if (dishOrders == null || !dishOrders.Any())
            {
                return NotFound("No orders found for this dish.");
            }

            return Ok(dishOrders);
        }

        // Add a new dish
        [HttpPost]
        [Authorize(Roles = "Admin")] // Only Admin can access this endpoint
        public async Task<ActionResult<Dish>> AddDish(Dish newDish)
        {
            try
            {
                if (!ModelState.IsValid)
                {
                    _logger.LogWarning(
                        "Ugyldig Dish data modtaget: {ValidationDetails}", 
                        new { 
                            Operation = "POST",
                            EntityType = "Dish",
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
                    "Oprettelse af ny ret påbegyndt: {DishDetails}", 
                    new { 
                        Operation = "POST",
                        EntityType = "Dish",
                        UserId = User.FindFirstValue(ClaimTypes.NameIdentifier),
                        UserName = User.Identity?.Name,
                        Timestamp = DateTime.UtcNow,
                        DishName = newDish.Name,
                        Price = newDish.Price,
                        CookId = newDish.CookID,
                        StartTime = newDish.StartTime,
                        EndTime = newDish.EndTime
                    });

                _context.Dishes.Add(newDish);
                await _context.SaveChangesAsync();

                _logger.LogInformation(
                    "Ret oprettet succesfuldt: {DishResult}", 
                    new { 
                        Operation = "POST",
                        EntityType = "Dish",
                        UserId = User.FindFirstValue(ClaimTypes.NameIdentifier),
                        UserName = User.Identity?.Name,
                        Timestamp = DateTime.UtcNow,
                        DishId = newDish.DishID,
                        DishName = newDish.Name
                    });

                return CreatedAtAction(nameof(GetDishById), new { id = newDish.DishID }, newDish);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, 
                    "Fejl ved oprettelse af ret: {ErrorDetails}", 
                    new { 
                        Operation = "POST",
                        EntityType = "Dish",
                        UserId = User.FindFirstValue(ClaimTypes.NameIdentifier),
                        UserName = User.Identity?.Name,
                        Timestamp = DateTime.UtcNow,
                        DishName = newDish.Name,
                        ErrorMessage = ex.Message
                    });
                throw;
            }
        }

        // Update an existing dish
        [HttpPut("{id}")]
        [Authorize(Roles = "Admin")] // Only Admin can access this endpoint
        public async Task<IActionResult> UpdateDish(int id, Dish updatedDish)
        {
            try
            {
                if (id != updatedDish.DishID)
                {
                    _logger.LogWarning(
                        "ID mismatch ved opdatering af ret: {Details}", 
                        new { 
                            Operation = "PUT",
                            EntityType = "Dish",
                            UserId = User.FindFirstValue(ClaimTypes.NameIdentifier),
                            UserName = User.Identity?.Name,
                            Timestamp = DateTime.UtcNow,
                            RequestedId = id,
                            ProvidedId = updatedDish.DishID
                        });
                    return BadRequest("Dish ID mismatch.");
                }

                var existingDish = await _context.Dishes.FindAsync(id);
                if (existingDish == null)
                {
                    _logger.LogWarning(
                        "Forsøg på at opdatere ikke-eksisterende ret: {Details}", 
                        new { 
                            Operation = "PUT",
                            EntityType = "Dish",
                            UserId = User.FindFirstValue(ClaimTypes.NameIdentifier),
                            UserName = User.Identity?.Name,
                            Timestamp = DateTime.UtcNow,
                            DishId = id
                        });
                    return NotFound();
                }

                _logger.LogInformation(
                    "Opdatering af ret påbegyndt: {Details}", 
                    new { 
                        Operation = "PUT",
                        EntityType = "Dish",
                        UserId = User.FindFirstValue(ClaimTypes.NameIdentifier),
                        UserName = User.Identity?.Name,
                        Timestamp = DateTime.UtcNow,
                        DishId = id,
                        OldName = existingDish.Name,
                        NewName = updatedDish.Name,
                        OldPrice = existingDish.Price,
                        NewPrice = updatedDish.Price,
                        OldCookId = existingDish.CookID,
                        NewCookId = updatedDish.CookID,
                        OldStartTime = existingDish.StartTime,
                        NewStartTime = updatedDish.StartTime,
                        OldEndTime = existingDish.EndTime,
                        NewEndTime = updatedDish.EndTime
                    });

                existingDish.Name = updatedDish.Name;
                existingDish.Quantity = updatedDish.Quantity;
                existingDish.Price = updatedDish.Price;
                existingDish.StartTime = updatedDish.StartTime;
                existingDish.EndTime = updatedDish.EndTime;
                existingDish.CookID = updatedDish.CookID;

                await _context.SaveChangesAsync();

                _logger.LogInformation(
                    "Ret opdateret succesfuldt: {Result}", 
                    new { 
                        Operation = "PUT",
                        EntityType = "Dish",
                        UserId = User.FindFirstValue(ClaimTypes.NameIdentifier),
                        UserName = User.Identity?.Name,
                        Timestamp = DateTime.UtcNow,
                        DishId = id,
                        DishName = updatedDish.Name
                    });

                return NoContent();
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, 
                    "Fejl ved opdatering af ret: {ErrorDetails}", 
                    new { 
                        Operation = "PUT",
                        EntityType = "Dish",
                        UserId = User.FindFirstValue(ClaimTypes.NameIdentifier),
                        UserName = User.Identity?.Name,
                        Timestamp = DateTime.UtcNow,
                        DishId = id,
                        ErrorMessage = ex.Message
                    });
                throw;
            }
        }

        // Delete a dish
        [HttpDelete("{id}")]
        [Authorize(Roles = "Admin")] // Only Admin can access this endpoint
        public async Task<IActionResult> DeleteDish(int id)
        {
            try
            {
                var dish = await _context.Dishes
                    .Include(d => d.DishOrders)
                    .FirstOrDefaultAsync(d => d.DishID == id);

                if (dish == null)
                {
                    _logger.LogWarning(
                        "Forsøg på at slette ikke-eksisterende ret: {Details}", 
                        new { 
                            Operation = "DELETE",
                            EntityType = "Dish",
                            UserId = User.FindFirstValue(ClaimTypes.NameIdentifier),
                            UserName = User.Identity?.Name,
                            Timestamp = DateTime.UtcNow,
                            DishId = id
                        });
                    return NotFound();
                }

                _logger.LogInformation(
                    "Sletning af ret påbegyndt: {Details}", 
                    new { 
                        Operation = "DELETE",
                        EntityType = "Dish",
                        UserId = User.FindFirstValue(ClaimTypes.NameIdentifier),
                        UserName = User.Identity?.Name,
                        Timestamp = DateTime.UtcNow,
                        DishId = id,
                        DishName = dish.Name,
                        AssociatedOrders = dish.DishOrders?.Count ?? 0
                    });

                _context.Dishes.Remove(dish);
                await _context.SaveChangesAsync();

                _logger.LogInformation(
                    "Ret slettet succesfuldt: {Result}", 
                    new { 
                        Operation = "DELETE",
                        EntityType = "Dish",
                        UserId = User.FindFirstValue(ClaimTypes.NameIdentifier),
                        UserName = User.Identity?.Name,
                        Timestamp = DateTime.UtcNow,
                        DishId = id,
                        DishName = dish.Name
                    });

                return NoContent();
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, 
                    "Fejl ved sletning af ret: {ErrorDetails}", 
                    new { 
                        Operation = "DELETE",
                        EntityType = "Dish",
                        UserId = User.FindFirstValue(ClaimTypes.NameIdentifier),
                        UserName = User.Identity?.Name,
                        Timestamp = DateTime.UtcNow,
                        DishId = id,
                        ErrorMessage = ex.Message
                    });
                throw;
            }
        }
    }
}
