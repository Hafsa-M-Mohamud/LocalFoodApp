using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Assignment3BAD.Database;
using Assignment3BAD.Models;
using Microsoft.Extensions.Logging;
using System.Security.Claims;
using System;
using Microsoft.AspNetCore.Authorization; // Import for [Authorize]
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace Assignment3BAD.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    [Authorize] // Protect all actions in the controller by default
    public class CooksController : ControllerBase
    {
        private readonly MyDBContext _context;
        private readonly ILogger<CooksController> _logger;

        public CooksController(MyDBContext context, ILogger<CooksController> logger)
        {
            _context = context;
            _logger = logger;
        }

        // Get all cooks - Accessible to Admins and Managers query 1
        [HttpGet]
        [Authorize(Roles = "Manager,Admin")] // Kun Manager og Admin kan tilgå
        public async Task<ActionResult> GetCooks()
        {
            var cooks = await _context.Cooks
                .Select(c => new 
                {
                    c.CookID,
                    c.Name,
                    c.PhoneNumber,
                    c.PhysicalAddress,
                    c.PassedCourse
                })
                .ToListAsync();

            return Ok(cooks);
        }

        // Get basic info (Name, PhoneNumber, PhysicalAddress) for a specific cook by ID
        [HttpGet("{id}/basic-info")]
        [Authorize(Roles = "Manager,Admin")] // Kun Manager og Admin kan tilgå
        public async Task<ActionResult> GetCookBasicInfo(int id)
        {
            var Cooks = await _context.Cooks.Select(c => new
            {
                c.CookID,
                c.PhysicalAddress,
                c.PhoneNumber,
                c.PassedCourse,
            }).ToListAsync();
            return Ok(Cooks);
        }

        // Get cook by ID - Accessible to Admins and Cooks query 1 
        [HttpGet("{id}")]
        [Authorize(Roles = "Admin,Manager")]
        public async Task<ActionResult<Cook>> GetCook(int id)
        {
            var cook = await _context.Cooks.FindAsync(id);
            if (cook == null)
                return NotFound();

            return cook;
        }

        // Get all dishes for a specific cook
        [HttpGet("{id}/dishes")]
        [Authorize(Roles = "Admin")] // Only Admins can view dishes for a cook
        public async Task<ActionResult> GetDishesByCook(int id)
        {
            var dishes = await _context
                .Dishes.Where(d => d.CookID == id)
                .Select(d => new
                {
                    d.Name,
                    d.Price,
                    StartTime = d.StartTime.TimeOfDay,
                    EndTime = d.EndTime.TimeOfDay,
                })
                .ToListAsync();

            if (!dishes.Any())
                return NotFound("No dishes found for this cook.");

            return Ok(dishes);
        }

        // Post a new dish for a specific cook
        [HttpPost("{id}/dishes")]
        [Authorize(Roles = "Admin")] // Only Admins can add dishes
        public async Task<ActionResult<Dish>> AddDishToCook(int id, Dish dish)
        {
            var cook = await _context.Cooks.FindAsync(id);
            if (cook == null)
                return NotFound("Cook not found.");

            dish.CookID = id; // Associate the new dish with the specified cook
            _context.Dishes.Add(dish);
            await _context.SaveChangesAsync();

            return CreatedAtAction(nameof(GetDishesByCook), new { id = id }, dish);
        }

        // Post a new cook
        [HttpPost]
        [Authorize(Policy = "AdminOnly")] // Only Admins can add new cooks
        public async Task<ActionResult<Cook>> AddCook(Cook cook)
        {
            try
            {
                if (!ModelState.IsValid)
                {
                    _logger.LogWarning(
                        "Ugyldig cook data modtaget: {ValidationDetails}", 
                        new { 
                            Operation = "POST",
                            EntityType = "Cook",
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
                    "Oprettelse af ny cook påbegyndt: {CookDetails}", 
                    new { 
                        Operation = "POST",
                        EntityType = "Cook",
                        UserId = User.FindFirstValue(ClaimTypes.NameIdentifier),
                        UserName = User.Identity?.Name,
                        Timestamp = DateTime.UtcNow,
                        CookID = cook.CookID,
                        CookName = cook.Name,
                        CookPhone = cook.PhoneNumber,
                        CookAddress = cook.PhysicalAddress
                    });

                _context.Cooks.Add(cook);
                await _context.SaveChangesAsync();

                _logger.LogInformation(
                    "Cook oprettet succesfuldt: {Result}", 
                    new { 
                        Operation = "POST",
                        EntityType = "Cook",
                        UserId = User.FindFirstValue(ClaimTypes.NameIdentifier),
                        UserName = User.Identity?.Name,
                        Timestamp = DateTime.UtcNow,
                        CookID = cook.CookID,
                        CookName = cook.Name
                    });

                return CreatedAtAction(nameof(GetCook), new { id = cook.CookID }, cook);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, 
                    "Fejl ved oprettelse af cook: {ErrorDetails}", 
                    new { 
                        Operation = "POST",
                        EntityType = "Cook",
                        UserId = User.FindFirstValue(ClaimTypes.NameIdentifier),
                        UserName = User.Identity?.Name,
                        Timestamp = DateTime.UtcNow,
                        CookName = cook.Name,
                        ErrorMessage = ex.Message
                    });
                throw;
            }
        }

        // Update a cook's information - Accessible to Admins only
        [HttpPut("{id}")]
        [Authorize(Policy = "AdminOnly")] // Only Admins can update cook details
        public async Task<IActionResult> UpdateCook(int id, Cook cook)
        {
            try
            {
                if (id != cook.CookID)
                {
                    _logger.LogWarning(
                        "Mismatch i cook ID ved opdatering: {Details}", 
                        new { 
                            Operation = "PUT",
                            EntityType = "Cook",
                            UserId = User.FindFirstValue(ClaimTypes.NameIdentifier),
                            UserName = User.Identity?.Name,
                            Timestamp = DateTime.UtcNow,
                            RequestedId = id,
                            ProvidedId = cook.CookID
                        });
                    return BadRequest("Cook ID mismatch.");
                }

                var existingCook = await _context.Cooks.FindAsync(id);
                if (existingCook == null)
                {
                    _logger.LogWarning(
                        "Forsøg på at opdatere ikke-eksisterende cook: {Details}", 
                        new { 
                            Operation = "PUT",
                            EntityType = "Cook",
                            UserId = User.FindFirstValue(ClaimTypes.NameIdentifier),
                            UserName = User.Identity?.Name,
                            Timestamp = DateTime.UtcNow,
                            CookID = id
                        });
                    return NotFound();
                }

                _logger.LogInformation(
                    "Opdatering af cook påbegyndt: {Details}", 
                    new { 
                        Operation = "PUT",
                        EntityType = "Cook",
                        UserId = User.FindFirstValue(ClaimTypes.NameIdentifier),
                        UserName = User.Identity?.Name,
                        Timestamp = DateTime.UtcNow,
                        CookID = id,
                        OldName = existingCook.Name,
                        NewName = cook.Name
                    });

                _context.Entry(existingCook).CurrentValues.SetValues(cook);
                await _context.SaveChangesAsync();

                _logger.LogInformation(
                    "Cook opdateret succesfuldt: {Result}", 
                    new { 
                        Operation = "PUT",
                        EntityType = "Cook",
                        UserId = User.FindFirstValue(ClaimTypes.NameIdentifier),
                        UserName = User.Identity?.Name,
                        Timestamp = DateTime.UtcNow,
                        CookID = id
                    });

                return NoContent();
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, 
                    "Fejl ved opdatering af cook: {ErrorDetails}", 
                    new { 
                        Operation = "PUT",
                        EntityType = "Cook",
                        UserId = User.FindFirstValue(ClaimTypes.NameIdentifier),
                        UserName = User.Identity?.Name,
                        Timestamp = DateTime.UtcNow,
                        CookID = id,
                        ErrorMessage = ex.Message
                    });
                throw;
            }
        }

        // Delete a cook
        [HttpDelete("{id}")]
        [Authorize(Policy = "AdminOnly")] // Only Admins can delete cooks
        public async Task<IActionResult> DeleteCook(int id)
        {
            try
            {
                var cook = await _context.Cooks
                    .Include(c => c.Dishes)
                    .FirstOrDefaultAsync(c => c.CookID == id);

                if (cook == null)
                {
                    _logger.LogWarning(
                        "Forsøg på at slette ikke-eksisterende cook: {Details}", 
                        new { 
                            Operation = "DELETE",
                            EntityType = "Cook",
                            UserId = User.FindFirstValue(ClaimTypes.NameIdentifier),
                            UserName = User.Identity?.Name,
                            Timestamp = DateTime.UtcNow,
                            CookID = id
                        });
                    return NotFound();
                }

                _logger.LogInformation(
                    "Sletning af cook påbegyndt: {Details}", 
                    new { 
                        Operation = "DELETE",
                        EntityType = "Cook",
                        UserId = User.FindFirstValue(ClaimTypes.NameIdentifier),
                        UserName = User.Identity?.Name,
                        Timestamp = DateTime.UtcNow,
                        CookID = id,
                        CookName = cook.Name,
                        AssociatedDishes = cook.Dishes?.Count ?? 0
                    });

                _context.Cooks.Remove(cook);
                await _context.SaveChangesAsync();

                _logger.LogInformation(
                    "Cook slettet succesfuldt: {Result}", 
                    new { 
                        Operation = "DELETE",
                        EntityType = "Cook",
                        UserId = User.FindFirstValue(ClaimTypes.NameIdentifier),
                        UserName = User.Identity?.Name,
                        Timestamp = DateTime.UtcNow,
                        CookID = id
                    });

                return NoContent();
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, 
                    "Fejl ved sletning af cook: {ErrorDetails}", 
                    new { 
                        Operation = "DELETE",
                        EntityType = "Cook",
                        UserId = User.FindFirstValue(ClaimTypes.NameIdentifier),
                        UserName = User.Identity?.Name,
                        Timestamp = DateTime.UtcNow,
                        CookID = id,
                        ErrorMessage = ex.Message
                    });
                throw;
            }
        }
    }
}
