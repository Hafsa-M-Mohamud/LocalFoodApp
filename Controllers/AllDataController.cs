using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Assignment3BAD.Database;
using Microsoft.AspNetCore.Authorization;
namespace Assignment3BAD.Controllers 
{
    [ApiController]
    [Route("api/[controller]")]
    [Authorize] // Gør at alle endpoints kræver autorisation - dette kan dog overskrives.
    public class AllDataController : ControllerBase 
    {
        private readonly MyDBContext _context;

        public AllDataController(MyDBContext context)
        {
            _context = context;
        }

        // // ***** COOK ENDPOINTS *****
        // [HttpGet("cooks")]
        // public async Task<ActionResult<IEnumerable<Cook>>> GetCooks()
        // {
        //     return await _context.Cooks.ToListAsync();
        // }

        // [HttpPost("cooks")]
        // public async Task<ActionResult<Cook>> AddCook(Cook cook)
        // {
        //     _context.Cooks.Add(cook);
        //     await _context.SaveChangesAsync();
        //     return CreatedAtAction(nameof(GetCooks), new { id = cook.CookID }, cook);
        // }

        // // ***** CUSTOMER ENDPOINTS *****
        // [HttpGet("customers")]
        // public async Task<ActionResult<IEnumerable<Customer>>> GetCustomers()
        // {
        //     return await _context.Customers.ToListAsync();
        // }

        // [HttpPost("customers")]
        // public async Task<ActionResult<Customer>> AddCustomer(Customer customer)
        // {
        //     _context.Customers.Add(customer);
        //     await _context.SaveChangesAsync();
        //     return CreatedAtAction(nameof(GetCustomers), new { id = customer.CustomerID }, customer);
        // }

        // // ***** CYCLIST ENDPOINTS *****
        // [HttpGet("cyclists")]
        // public async Task<ActionResult<IEnumerable<Cyclist>>> GetCyclists()
        // {
        //     return await _context.Cyclists.ToListAsync();
        // }

        // [HttpPost("cyclists")]
        // public async Task<ActionResult<Cyclist>> AddCyclist(Cyclist cyclist)
        // {
        //     _context.Cyclists.Add(cyclist);
        //     await _context.SaveChangesAsync();
        //     return CreatedAtAction(nameof(GetCyclists), new { id = cyclist.CyclistID }, cyclist);
        // }

        // // ***** DISH ENDPOINTS *****

        // // Get all dishes
        // [HttpGet("dishes")]
        // public async Task<ActionResult<IEnumerable<Dish>>> GetDishes()
        // {
        //     return await _context.Dishes.ToListAsync();
        // }

        // // Add a new dish
        // [HttpPost("dishes")]
        // public async Task<ActionResult<Dish>> AddDish(Dish newDish)
        // {
        //     if (!ModelState.IsValid)
        //     {
        //         return BadRequest(ModelState);
        //     }

        //     _context.Dishes.Add(newDish);
        //     await _context.SaveChangesAsync();
        //     return CreatedAtAction(nameof(GetDishes), new { id = newDish.DishID }, newDish);
        // }

        // // Update an existing dish
        // [HttpPut("dishes/{id}")]
        // public async Task<ActionResult<Dish>> UpdateDish(int id, Dish updatedDish) 
        // {
        //     if (id != updatedDish.DishID)
        //     {
        //         return BadRequest("Dish ID mismatch.");
        //     }

        //     if (!ModelState.IsValid)
        //     {
        //         return BadRequest(ModelState);
        //     }

        //     var existingDish = await _context.Dishes.FindAsync(id);
        //     if (existingDish == null)
        //     {
        //         return NotFound();
        //     }

        //     // Update properties
        //     existingDish.Name = updatedDish.Name;
        //     existingDish.Price = updatedDish.Price;
        //     existingDish.StartTime = updatedDish.StartTime;
        //     existingDish.EndTime = updatedDish.EndTime;
        //     existingDish.CookID = updatedDish.CookID;

        //     await _context.SaveChangesAsync();
        //     return NoContent(); // Return 204 No Content
        // }

        // // Delete a dish
        // [HttpDelete("dishes/{id}")]
        // public async Task<ActionResult> DeleteDish(int id)
        // {
        //     var existingDish = await _context.Dishes.FindAsync(id);
        //     if (existingDish == null)
        //     {
        //         return NotFound();
        //     }

        //     _context.Dishes.Remove(existingDish);
        //     await _context.SaveChangesAsync();
        //     return NoContent(); // Return 204 No Content
        // }

        // // ***** ORDER ENDPOINTS *****
        // [HttpGet("orders")]
        // public async Task<ActionResult<IEnumerable<Order>>> GetOrders()
        // {
        //     return await _context.Orders.ToListAsync();
        // }

        // [HttpPost("orders")]
        // public async Task<ActionResult<Order>> AddOrder(Order order)
        // {
        //     _context.Orders.Add(order);
        //     await _context.SaveChangesAsync();
        //     return CreatedAtAction(nameof(GetOrders), new { id = order.OrderID }, order);
        // }

        // // ***** TRIP ENDPOINTS *****
        // [HttpGet("trips")]
        // public async Task<ActionResult<IEnumerable<Trip>>> GetTrips()
        // {
        //     return await _context.Trips.ToListAsync();
        // }

        // [HttpPost("trips")]
        // public async Task<ActionResult<Trip>> AddTrip(Trip trip)
        // {
        //     _context.Trips.Add(trip);
        //     await _context.SaveChangesAsync();
        //     return CreatedAtAction(nameof(GetTrips), new { id = trip.TripID }, trip);
        // }

        // // ***** RATING ENDPOINTS *****
        // [HttpGet("ratings")]
        // public async Task<ActionResult<IEnumerable<RatingSystem>>> GetRatings()
        // {
        //     return await _context.Ratings.ToListAsync();
        // }

        // [HttpPost("ratings")]
        // public async Task<ActionResult<RatingSystem>> AddRating(RatingSystem rating)
        // {
        //     _context.Ratings.Add(rating);
        //     await _context.SaveChangesAsync();
        //     return CreatedAtAction(nameof(GetRatings), new { id = rating.RatingID }, rating);
        // }

        // ***** GET ALL DATA ENDPOINT *****
        [HttpGet]
        public async Task<ActionResult> GetAllData()
        {
            var allData = new
            {
                Cooks = await _context.Cooks.ToListAsync(),
                Cyclists = await _context.Cyclists.ToListAsync(),
                Customers = await _context.Customers.ToListAsync(),
                DishOrders = await _context.DishOrders.ToListAsync(),
                Dishes = await _context.Dishes.ToListAsync(),
                Orders = await _context.Orders.ToListAsync(),
                Trips = await _context.Trips.ToListAsync(),
                Ratings = await _context.Ratings.ToListAsync()
            };

            return Ok(allData);
        }

    }
}
