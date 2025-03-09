using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Assignment3BAD.Database;
using Assignment3BAD.Models;
using Microsoft.Extensions.Logging;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace Assignment3BAD.Controllers
{
[Route("api/[controller]")]
[ApiController]
public class TripsController : ControllerBase
{
    private readonly MyDBContext _context;
    private readonly ILogger<TripsController> _logger;

    public TripsController(MyDBContext context, ILogger<TripsController> logger)
    {
        _context = context;
        _logger = logger;
    }

        // GET all trips
        [HttpGet]
        [Authorize(Roles = "Admin")] // Only Admin can access this endpoint
        public async Task<ActionResult<IEnumerable<Trip>>> GetTrips()
        {
            var trips = await _context
                .Trips.Include(t => t.Stops) // Load related stops
                .ToListAsync();

            return Ok(trips); // Return the trips with their stops
        }

        // GET a specific trip by ID
        [HttpGet("{id}")]
        [Authorize(Roles = "Admin")] // Only Admin can access this endpoint
        public async Task<ActionResult<Trip>> GetTrip(int id)
        {
            var trip = await _context
                .Trips.Include(t => t.Stops) // Load related stops
                .FirstOrDefaultAsync(t => t.TripID == id);

            if (trip == null)
            {
                return NotFound(); // Return 404 if trip is not found
            }

            return Ok(trip); // Return the trip with its stops
        }

    // POST: Create a new trip
    [HttpPost]
    public async Task<ActionResult<Trip>> CreateTrip(Trip trip)
    {
        try
        {
            if (trip == null)
            {
                _logger.LogWarning("Forsøg på at oprette tur med null data");
                return BadRequest("Trip data is required.");
            }

            _logger.LogInformation("Oprettelse af ny tur påbegyndt med {StopCount} stop", 
                trip.Stops?.Count ?? 0);

            // Step 1: Add the trip to the database
            _context.Trips.Add(trip);
            await _context.SaveChangesAsync();

            _logger.LogInformation("Tur oprettet med ID: {TripID}", trip.TripID);

            // Step 2: Set the TripID and Trip navigation property for each stop
            if (trip.Stops != null)
            {
                foreach (var stop in trip.Stops)
                {
                    stop.TripID = trip.TripID;
                    stop.Trip = trip;
                    _context.TripStops.Add(stop);
                    _logger.LogInformation("Stop tilføjet til tur {TripID}: {StopDetails}", 
                        trip.TripID, 
                        new { stop.Location, stop.ArrivalTime, stop.DepartureTime });
                }

                await _context.SaveChangesAsync();
                _logger.LogInformation("Alle {StopCount} stop gemt for tur {TripID}", 
                    trip.Stops.Count, trip.TripID);
            }

            return CreatedAtAction(nameof(GetTrip), new { id = trip.TripID }, trip);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Fejl ved oprettelse af tur");
            throw;
        }
    }

    // PUT: Update an existing trip
    [HttpPut("{id}")]
    public async Task<IActionResult> UpdateTrip(int id, Trip trip)
    {
        try
        {
            if (id != trip.TripID)
            {
                _logger.LogWarning("Forsøg på at opdatere tur med uoverensstemmende ID'er: {RequestID} vs {TripID}", 
                    id, trip.TripID);
                return BadRequest("Trip ID mismatch.");
            }

            var existingTrip = await _context.Trips
                .Include(t => t.Stops)
                .FirstOrDefaultAsync(t => t.TripID == id);

            if (existingTrip == null)
            {
                _logger.LogWarning("Forsøg på at opdatere ikke-eksisterende tur med ID: {TripID}", id);
                return NotFound();
            }

            _logger.LogInformation("Opdatering af tur {TripID} påbegyndt", id);

            // Update trip details
            _context.Entry(existingTrip).CurrentValues.SetValues(trip);

            // Handle updating the stops
            if (trip.Stops != null)
            {
                foreach (var stop in trip.Stops)
                {
                    if (stop.TripStopID == 0)
                    {
                        // New stop
                        stop.TripID = trip.TripID;
                        _context.TripStops.Add(stop);
                        _logger.LogInformation("Nyt stop tilføjet til tur {TripID}: {StopDetails}", 
                            id, new { stop.Location, stop.ArrivalTime, stop.DepartureTime });
                    }
                    else
                    {
                        // Existing stop update
                        var existingStop = existingTrip.Stops.FirstOrDefault(s => s.TripStopID == stop.TripStopID);
                        if (existingStop != null)
                        {
                            _logger.LogInformation("Stop {StopID} opdateret for tur {TripID}: {StopDetails}", 
                                stop.TripStopID, 
                                id,
                                new { 
                                    OldLocation = existingStop.Location, 
                                    NewLocation = stop.Location,
                                    OldArrival = existingStop.ArrivalTime,
                                    NewArrival = stop.ArrivalTime
                                });
                        }
                        _context.Entry(stop).State = EntityState.Modified;
                    }
                }
            }

            await _context.SaveChangesAsync();
            _logger.LogInformation("Tur {TripID} blev opdateret succesfuldt", id);
            return NoContent();
        }
        catch (DbUpdateConcurrencyException ex)
        {
            _logger.LogError(ex, "Concurrent opdateringsfejl for tur {TripID}", id);
            if (!_context.Trips.Any(e => e.TripID == id))
            {
                return NotFound();
            }
            throw;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Fejl ved opdatering af tur {TripID}", id);
            throw;
        }
    }

    // DELETE: Remove a trip by ID
    [HttpDelete("{id}")]
    public async Task<IActionResult> DeleteTrip(int id)
    {
        try
        {
            var trip = await _context.Trips
                .Include(t => t.Stops)
                .FirstOrDefaultAsync(t => t.TripID == id);

            if (trip == null)
            {
                _logger.LogWarning("Forsøg på at slette ikke-eksisterende tur med ID: {TripID}", id);
                return NotFound();
            }

            _logger.LogInformation("Sletning af tur {TripID} påbegyndt med {StopCount} tilknyttede stop", 
                id, trip.Stops?.Count ?? 0);

            if (trip.Stops != null && trip.Stops.Any())
            {
                foreach (var stop in trip.Stops)
                {
                    _context.TripStops.Remove(stop);
                    _logger.LogInformation("Stop {StopID} slettet fra tur {TripID}: {Location}", 
                        stop.TripStopID, id, stop.Location);
                }
            }

            _context.Trips.Remove(trip);
            await _context.SaveChangesAsync();

            _logger.LogInformation("Tur {TripID} blev slettet succesfuldt", id);
            return NoContent();
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Fejl ved sletning af tur {TripID}", id);
            throw;
        }
    }
}
}
