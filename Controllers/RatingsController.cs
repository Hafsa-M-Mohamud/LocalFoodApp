using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Assignment3BAD.Database;
using Assignment3BAD.Models;
using Microsoft.Extensions.Logging;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Microsoft.AspNetCore.Authorization; // Import for [Authorize]

namespace Assignment3BAD.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    [Authorize]
    public class RatingsController : ControllerBase
    {
        private readonly MyDBContext _context;
        private readonly ILogger<RatingsController> _logger;

        public RatingsController(MyDBContext context, ILogger<RatingsController> logger)
        {
            _context = context;
            _logger = logger;
        }

        // Get all ratings
        [HttpGet]
        [Authorize(Policy = "AdminOnly")]
        public async Task<ActionResult<IEnumerable<RatingSystem>>> GetRatings()
        {
            return await _context.Ratings.ToListAsync();
        }

        // GET: api/rating/5
        [HttpGet("{id}")]
        public async Task<ActionResult<RatingSystem>> GetRating(int id)
        {
            var rating = await _context.Ratings.FindAsync(id);
            if (rating == null)
                return NotFound();
            return rating;
        }

        // Get ratings done by a specific customer
        [HttpGet("customer/{customerId}")]
        public async Task<ActionResult<IEnumerable<RatingSystem>>> GetRatingsByCustomer(
            int customerId
        )
        {
            var ratings = await _context
                .Ratings.Where(r => r.CustomerID == customerId)
                .ToListAsync();

            return Ok(ratings);
        }

        // // Get average food rating for a cook query 5
        // [HttpGet("cook/{cookId}/average")]
        // [Authorize(Roles = "Admin,Cook")]
        // public async Task<ActionResult<double>> GetAverageFoodRating(int cookId)
        // {
        //     var averageRating = await _context.Ratings
        //         .Where(r => r.CookID == cookId)
        //         .AverageAsync(r => r.FoodRating);

        //     return Ok(averageRating);
        // }

        //query 5
        // Get average food rating for the logged-in cook
        [HttpGet("MyCook/average-rating")]
        [Authorize(Roles = "Admin,Cook")] // Kun Admin og Cook kan tilgå
        public async Task<ActionResult<double>> GetAverageFoodRating()
        {
            // Hent UserId fra JWT-token
            var userId = User.FindFirst(System.Security.Claims.ClaimTypes.NameIdentifier)?.Value;
            Console.WriteLine($"UserId from token: {userId}");

            // Inkluder User relationen og log alle kokke debugging
            var allCooks = await _context.Cooks
                .Include(c => c.User)
                .ToListAsync();
            
            Console.WriteLine("Alle kokke i databasen:");
            foreach (var c in allCooks)
            {
                Console.WriteLine($"CookID: {c.CookID}, UserId: {c.UserId}, Name: {c.Name}");
            }

            var cook = await _context.Cooks
                .Include(c => c.User)
                .FirstOrDefaultAsync(c => c.UserId == userId);

            if (cook == null)
            {
                Console.WriteLine($"Ingen kok fundet med UserId: {userId}");
                return NotFound($"Kok ikke fundet med UserId: {userId}");
            } //HTTP 404-fejl

            Console.WriteLine($"Fandt kok med ID: {cook.CookID}");

            var ratings = await _context.Ratings
                .Where(r => r.CookID == cook.CookID)
                .ToListAsync();

            Console.WriteLine($"Fandt {ratings.Count} ratings for kokken");

            if (!ratings.Any())
            {
                return Ok(0); // Returner 0 hvis der ikke er nogen ratings
            }

            var averageRating = ratings.Average(r => r.FoodRating);
            Console.WriteLine($"Gennemsnitlig rating: {averageRating}");

            return Ok(averageRating);
        }
        // Get average delivery rating for a cyclist query 6
        // Get average delivery rating for the logged-in cyclist
        [HttpGet("MyCyclist/average-delivery-rating")]
        [Authorize(Roles = "Cyclist,Admin")] // Kun Cyclist og Admin kan tilgå
        public async Task<ActionResult<double>> GetAverageDeliveryRating()
        {
            // Hent UserId fra JWT-token
            var userId = User.FindFirst(System.Security.Claims.ClaimTypes.NameIdentifier)?.Value;

            // Find Cyclist baseret på UserId
            var cyclist = await _context.Cyclists.FirstOrDefaultAsync(c => c.UserId == userId);

            if (cyclist == null)
                return NotFound("Cyclist not found.");

            // Beregn gennemsnitlig leveringsrating for den loggede Cyclist
            var averageRating = await _context.Ratings
                .Where(r => r.CyclistID == cyclist.CyclistID)
                .AverageAsync(r => r.DeliveryRating);

            return Ok(averageRating);
        }


        // Post a new rating
        [HttpPost]
        [Authorize(Roles = "Admin")] // Only Admin can access this endpoint
        public async Task<ActionResult<RatingSystem>> AddRating(RatingSystem rating)
        {
            try
            {
                if (!ModelState.IsValid)
                {
                    _logger.LogWarning("Forsøg på at oprette bedømmelse med ugyldig modelstate");
                    return BadRequest(ModelState);
                }

                _logger.LogInformation("Oprettelse af ny bedømmelse påbegyndt: {RatingDetails}", 
                    new { rating.CustomerID, rating.CookID, rating.CyclistID, 
                          rating.FoodRating, rating.DeliveryRating });

                _context.Ratings.Add(rating);
                await _context.SaveChangesAsync();

                _logger.LogInformation("Bedømmelse oprettet succesfuldt med ID: {RatingID}", 
                    rating.RatingID);

                return CreatedAtAction(nameof(GetRatings), new { id = rating.RatingID }, rating);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Fejl ved oprettelse af bedømmelse");
                throw;
            }
        }

        // Update a rating by ID
        [HttpPut("{id}")]
        [Authorize(Roles = "Admin")] // Only Admin can access this endpoint
        public async Task<IActionResult> UpdateRating(int id, RatingSystem rating)
        {
            try
            {
                if (id != rating.RatingID)
                {
                    _logger.LogWarning("Forsøg på at opdatere bedømmelse med uoverensstemmende ID'er: {RequestID} vs {RatingID}", 
                        id, rating.RatingID);
                    return BadRequest("Rating ID mismatch.");
                }

                var existingRating = await _context.Ratings.FindAsync(id);
                if (existingRating == null)
                {
                    _logger.LogWarning("Forsøg på at opdatere ikke-eksisterende bedømmelse med ID: {RatingID}", id);
                    return NotFound();
                }

                _logger.LogInformation("Opdatering af bedømmelse {RatingID} påbegyndt", id);

                // Log ændringer
                if (existingRating.FoodRating != rating.FoodRating)
                {
                    _logger.LogInformation("Bedømmelse {RatingID}: Mad-rating ændret fra {OldRating} til {NewRating}", 
                        id, existingRating.FoodRating, rating.FoodRating);
                }

                if (existingRating.DeliveryRating != rating.DeliveryRating)
                {
                    _logger.LogInformation("Bedømmelse {RatingID}: Leverings-rating ændret fra {OldRating} til {NewRating}", 
                        id, existingRating.DeliveryRating, rating.DeliveryRating);
                }

                _context.Entry(existingRating).CurrentValues.SetValues(rating);
                await _context.SaveChangesAsync();

                _logger.LogInformation("Bedømmelse {RatingID} blev opdateret succesfuldt", id);
                return NoContent();
            }
            catch (DbUpdateConcurrencyException ex)
            {
                _logger.LogError(ex, "Concurrent opdateringsfejl for bedømmelse {RatingID}", id);
                if (!_context.Ratings.Any(e => e.RatingID == id))
                {
                    return NotFound();
                }
                throw;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Fejl ved opdatering af bedømmelse {RatingID}", id);
                throw;
            }
        }

        // Delete a rating by ID
        [HttpDelete("{id}")]
        [Authorize(Roles = "Admin")] // Only Admin can access this endpoint
        public async Task<IActionResult> DeleteRating(int id)
        {
            try
            {
                var rating = await _context.Ratings.FindAsync(id);
                if (rating == null)
                {
                    _logger.LogWarning("Forsøg på at slette ikke-eksisterende bedømmelse med ID: {RatingID}", id);
                    return NotFound();
                }

                _logger.LogInformation("Sletning af bedømmelse {RatingID} påbegyndt. Detaljer: {RatingDetails}", 
                    id, new { rating.CustomerID, rating.CookID, rating.CyclistID, 
                             rating.FoodRating, rating.DeliveryRating });

                _context.Ratings.Remove(rating);
                await _context.SaveChangesAsync();

                _logger.LogInformation("Bedømmelse {RatingID} blev slettet succesfuldt", id);
                return NoContent();
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Fejl ved sletning af bedømmelse {RatingID}", id);
                throw;
            }
        }
    }
}
