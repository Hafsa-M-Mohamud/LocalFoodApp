using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Assignment3BAD.Database;
using Assignment3BAD.Models;
using System.Security.Claims;
using Microsoft.Extensions.Logging;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace Assignment3BAD.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    [Authorize]
    public class CyclistsController : ControllerBase
    {
        private readonly MyDBContext _context;
        private readonly ILogger<CyclistsController> _logger;

        public CyclistsController(MyDBContext context, ILogger<CyclistsController> logger)
        {
            _context = context;
            _logger = logger;
        }

        // Get all cyclists
        [HttpGet]
        [Authorize(Roles = "Admin")] // Only Admin can access this endpoint
        public async Task<ActionResult<IEnumerable<Cyclist>>> GetCyclists()
        {
            return await _context.Cyclists.ToListAsync();
        }

        // Get cyclist by ID
        [HttpGet("{id}")]
        [Authorize(Roles = "Admin")] // Only Admin can access this endpoint
        public async Task<ActionResult<Cyclist>> GetCyclist(int id)
        {
            var cyclist = await _context.Cyclists.FindAsync(id);
            if (cyclist == null)
                return NotFound();
            return cyclist;
        }

        // Get all trips for a specific cyclist
        [HttpGet("{id}/trips")]
        [Authorize(Roles = "Admin")] // Only Admin can access this endpoint
        public async Task<ActionResult> GetTripsByCyclist(int id)
        {
            var trips = await _context
                .Trips.Where(t => t.CyclistID == id)
                .Select(t => new { t.TripID, t.Stops })
                .ToListAsync();

            if (!trips.Any())
                return NotFound("No trips found for this cyclist.");
            return Ok(trips);
        }

        [HttpGet("{id}/monthly-info")] //query 6
        [Authorize(Roles="Admin,Cyclist")] //only Admin and Cyclist can access this endpoint
        public async Task<ActionResult<IEnumerable<object>>> GetMonthlyInfo(int id)
        {
            var stats = await _context
                .CyclistStats.Where(cs => cs.CyclistID == id)
                .Select(cs => new
                {
                    cs.Month,
                    cs.MonthlyHours,
                    cs.MonthlyEarning,
                })
                .ToListAsync();

            if (!stats.Any())
                return NotFound("Cyclist's monthly info not found.");
            return Ok(stats);
        }

        // Add a new cyclist
        [HttpPost]
        [Authorize(Roles = "Admin")] // Only Admin can access this endpoint
        public async Task<ActionResult<Cyclist>> AddCyclist(Cyclist cyclist)
        {
            try
            {
                if (!ModelState.IsValid)
                {
                    _logger.LogWarning(
                        "Ugyldig cyclist data modtaget: {ValidationDetails}", 
                        new { 
                            Operation = "POST",
                            EntityType = "Cyclist",
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
                    "Oprettelse af ny cyclist påbegyndt: {CyclistDetails}", 
                    new { 
                        Operation = "POST",
                        EntityType = "Cyclist",
                        UserId = User.FindFirstValue(ClaimTypes.NameIdentifier),
                        UserName = User.Identity?.Name,
                        Timestamp = DateTime.UtcNow,
                        BikeType = cyclist.BikeType,
                        PhoneNumber = cyclist.PhoneNumber
                    });

                _context.Cyclists.Add(cyclist);
                await _context.SaveChangesAsync();

                _logger.LogInformation(
                    "Cyclist oprettet succesfuldt: {CyclistResult}", 
                    new { 
                        Operation = "POST",
                        EntityType = "Cyclist",
                        UserId = User.FindFirstValue(ClaimTypes.NameIdentifier),
                        UserName = User.Identity?.Name,
                        Timestamp = DateTime.UtcNow,
                        CyclistId = cyclist.CyclistID
                    });

                return CreatedAtAction(nameof(GetCyclist), new { id = cyclist.CyclistID }, cyclist);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, 
                    "Fejl ved oprettelse af cyclist: {ErrorDetails}", 
                    new { 
                        Operation = "POST",
                        EntityType = "Cyclist",
                        UserId = User.FindFirstValue(ClaimTypes.NameIdentifier),
                        UserName = User.Identity?.Name,
                        Timestamp = DateTime.UtcNow,
                        ErrorMessage = ex.Message
                    });
                throw;
            }
        }

        // Update cyclist information
        [HttpPut("{id}")]
        [Authorize(Roles = "Admin")] // Only Admin can access this endpoint
        public async Task<IActionResult> UpdateCyclist(int id, Cyclist cyclist)
        {
            try 
            {
                if (id != cyclist.CyclistID)
                {
                    _logger.LogWarning(
                        "Mismatch i cyclist ID ved opdatering: {Details}", 
                        new { 
                            Operation = "PUT",
                            EntityType = "Cyclist",
                            UserId = User.FindFirstValue(ClaimTypes.NameIdentifier),
                            UserName = User.Identity?.Name,
                            Timestamp = DateTime.UtcNow,
                            RequestedId = id,
                            ProvidedId = cyclist.CyclistID
                        });
                    return BadRequest();
                }

                var existingCyclist = await _context.Cyclists.FindAsync(id);
                if (existingCyclist == null)
                {
                    _logger.LogWarning(
                        "Forsøg på at opdatere ikke-eksisterende cyclist: {Details}", 
                        new { 
                            Operation = "PUT",
                            EntityType = "Cyclist",
                            UserId = User.FindFirstValue(ClaimTypes.NameIdentifier),
                            UserName = User.Identity?.Name,
                            Timestamp = DateTime.UtcNow,
                            CyclistId = id
                        });
                    return NotFound();
                }

                _logger.LogInformation(
                    "Opdatering af cyclist påbegyndt: {Details}", 
                    new { 
                        Operation = "PUT",
                        EntityType = "Cyclist",
                        UserId = User.FindFirstValue(ClaimTypes.NameIdentifier),
                        UserName = User.Identity?.Name,
                        Timestamp = DateTime.UtcNow,
                        CyclistId = id,
                        OldBikeType = existingCyclist.BikeType,
                        NewBikeType = cyclist.BikeType,
                        OldPhoneNumber = existingCyclist.PhoneNumber,
                        NewPhoneNumber = cyclist.PhoneNumber
                    });

                _context.Entry(existingCyclist).CurrentValues.SetValues(cyclist);
                await _context.SaveChangesAsync();

                _logger.LogInformation(
                    "Cyclist opdateret succesfuldt: {Result}", 
                    new { 
                        Operation = "PUT",
                        EntityType = "Cyclist",
                        UserId = User.FindFirstValue(ClaimTypes.NameIdentifier),
                        UserName = User.Identity?.Name,
                        Timestamp = DateTime.UtcNow,
                        CyclistId = id
                    });

                return NoContent();
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, 
                    "Fejl ved opdatering af cyclist: {ErrorDetails}", 
                    new { 
                        Operation = "PUT",
                        EntityType = "Cyclist",
                        UserId = User.FindFirstValue(ClaimTypes.NameIdentifier),
                        UserName = User.Identity?.Name,
                        Timestamp = DateTime.UtcNow,
                        CyclistId = id,
                        ErrorMessage = ex.Message
                    });
                throw;
            }
        }

        // Delete a cyclist
        [HttpDelete("{id}")]
        [Authorize(Roles = "Admin")] // Only Admin can access this endpoint
        public async Task<IActionResult> DeleteCyclist(int id)
        {
            try
            {
                var cyclist = await _context.Cyclists
                    .Include(c => c.Trips)
                    .FirstOrDefaultAsync(c => c.CyclistID == id);

                if (cyclist == null)
                {
                    _logger.LogWarning(
                        "Forsøg på at slette ikke-eksisterende cyclist: {Details}", 
                        new { 
                            Operation = "DELETE",
                            EntityType = "Cyclist",
                            UserId = User.FindFirstValue(ClaimTypes.NameIdentifier),
                            UserName = User.Identity?.Name,
                            Timestamp = DateTime.UtcNow,
                            CyclistId = id
                        });
                    return NotFound();
                }

                _logger.LogInformation(
                    "Sletning af cyclist påbegyndt: {Details}", 
                    new { 
                        Operation = "DELETE",
                        EntityType = "Cyclist",
                        UserId = User.FindFirstValue(ClaimTypes.NameIdentifier),
                        UserName = User.Identity?.Name,
                        Timestamp = DateTime.UtcNow,
                        CyclistId = id,
                        AssociatedTrips = cyclist.Trips?.Count ?? 0
                    });

                _context.Cyclists.Remove(cyclist);
                await _context.SaveChangesAsync();

                _logger.LogInformation(
                    "Cyclist slettet succesfuldt: {Result}", 
                    new { 
                        Operation = "DELETE",
                        EntityType = "Cyclist",
                        UserId = User.FindFirstValue(ClaimTypes.NameIdentifier),
                        UserName = User.Identity?.Name,
                        Timestamp = DateTime.UtcNow,
                        CyclistId = id
                    });

                return NoContent();
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, 
                    "Fejl ved sletning af cyclist: {ErrorDetails}", 
                    new { 
                        Operation = "DELETE",
                        EntityType = "Cyclist",
                        UserId = User.FindFirstValue(ClaimTypes.NameIdentifier),
                        UserName = User.Identity?.Name,
                        Timestamp = DateTime.UtcNow,
                        CyclistId = id,
                        ErrorMessage = ex.Message
                    });
                throw;
            }
        }
    }
}
