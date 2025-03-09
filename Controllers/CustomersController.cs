using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Assignment3BAD.Database;
using Assignment3BAD.Models;
using System;
using Microsoft.Extensions.Logging;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.IdentityModel.Tokens;
using System.Security.Claims;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace Assignment3BAD.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    [Authorize]
    public class CustomersController : ControllerBase
    {
        private readonly MyDBContext _context;
        private readonly ILogger<CustomersController> _logger;

        public CustomersController(MyDBContext context, ILogger<CustomersController> logger)
        {
            _context = context;
            _logger = logger;
        }

        // Get all customers
        [HttpGet]
        public async Task<ActionResult<IEnumerable<Customer>>> GetCustomers()
        {
            return await _context.Customers.ToListAsync();
        }

        // Get customer by ID
        [HttpGet("{id}")]
        [Authorize(Roles = "Admin")] // Only Admin can access this endpoint
        public async Task<ActionResult<Customer>> GetCustomer(int id)
        {
            var customer = await _context.Customers.FindAsync(id);
            if (customer == null)
                return NotFound();
            return customer;
        }

        // Get all orders placed by a specific customer
        [HttpGet("{id}/orders")]
        [Authorize(Roles = "Admin")] // Only Admin can access this endpoint
        public async Task<ActionResult> GetOrdersByCustomer(int id)
        {
            var orders = await _context
                .Orders.Where(o => o.CustomerID == id)
                .Select(o => new
                {
                    o.OrderID,
                    o.OrderTime,
                    o.Dish,
                })
                .ToListAsync();

            if (!orders.Any())
                return NotFound("No orders found for this customer.");
            return Ok(orders);
        }

        // Post a new customer
        [HttpPost]
        [Authorize(Roles = "Admin")] // Only Admin can access this endpoint
        public async Task<ActionResult<Customer>> AddCustomer(Customer customer)
        {
            try
            {
                if (!ModelState.IsValid)
                {
                    _logger.LogWarning(
                        "Ugyldig customer data modtaget: {ValidationDetails}", 
                        new { 
                            Operation = "POST",
                            EntityType = "Customer",
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
                    "Oprettelse af ny customer påbegyndt: {CustomerDetails}", 
                    new { 
                        Operation = "POST",
                        EntityType = "Customer",
                        UserId = User.FindFirstValue(ClaimTypes.NameIdentifier),
                        UserName = User.Identity?.Name,
                        Timestamp = DateTime.UtcNow,
                        CustomerID = customer.CustomerID
                    });

                _context.Customers.Add(customer);
                await _context.SaveChangesAsync();

                _logger.LogInformation(
                    "Customer oprettet succesfuldt: {Result}", 
                    new { 
                        Operation = "POST",
                        EntityType = "Customer",
                        UserId = User.FindFirstValue(ClaimTypes.NameIdentifier),
                        UserName = User.Identity?.Name,
                        Timestamp = DateTime.UtcNow,
                        CustomerID = customer.CustomerID
                    });

                return CreatedAtAction(nameof(GetCustomer), new { id = customer.CustomerID }, customer);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, 
                    "Fejl ved oprettelse af customer: {ErrorDetails}", 
                    new { 
                        Operation = "POST",
                        EntityType = "Customer",
                        UserId = User.FindFirstValue(ClaimTypes.NameIdentifier),
                        UserName = User.Identity?.Name,
                        Timestamp = DateTime.UtcNow,
                        ErrorMessage = ex.Message
                    });
                throw;
            }
        }
        

        // Update a customer's information
        [HttpPut("{id}")]
        [Authorize(Roles = "Admin")] // Only Admin can access this endpoint
        public async Task<IActionResult> UpdateCustomer(int id, Customer customer)
        {
            try
            {
                if (id != customer.CustomerID)
                {
                    _logger.LogWarning(
                        "Mismatch i customer ID ved opdatering: {Details}", 
                        new { 
                            Operation = "PUT",
                            EntityType = "Customer",
                            UserId = User.FindFirstValue(ClaimTypes.NameIdentifier),
                            UserName = User.Identity?.Name,
                            Timestamp = DateTime.UtcNow,
                            RequestedId = id,
                            ProvidedId = customer.CustomerID
                        });
                    return BadRequest("Customer ID mismatch.");
                }

                var existingCustomer = await _context.Customers.FindAsync(id);
                if (existingCustomer == null)
                {
                    _logger.LogWarning(
                        "Forsøg på at opdatere ikke-eksisterende customer: {Details}", 
                        new { 
                            Operation = "PUT",
                            EntityType = "Customer",
                            UserId = User.FindFirstValue(ClaimTypes.NameIdentifier),
                            UserName = User.Identity?.Name,
                            Timestamp = DateTime.UtcNow,
                            CustomerID = id
                        });
                    return NotFound();
                }

                _context.Entry(existingCustomer).CurrentValues.SetValues(customer);
                await _context.SaveChangesAsync();

                _logger.LogInformation(
                    "Customer opdateret succesfuldt: {Result}", 
                    new { 
                        Operation = "PUT",
                        EntityType = "Customer",
                        UserId = User.FindFirstValue(ClaimTypes.NameIdentifier),
                        UserName = User.Identity?.Name,
                        Timestamp = DateTime.UtcNow,
                        CustomerID = id
                    });

                return NoContent();
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, 
                    "Fejl ved opdatering af customer: {ErrorDetails}", 
                    new { 
                        Operation = "PUT",
                        EntityType = "Customer",
                        UserId = User.FindFirstValue(ClaimTypes.NameIdentifier),
                        UserName = User.Identity?.Name,
                        Timestamp = DateTime.UtcNow,
                        CustomerID = id,
                        ErrorMessage = ex.Message
                    });
                throw;
            }
        }

        // Delete a customer
        [HttpDelete("{id}")]
        [Authorize(Roles = "Admin")] // Only Admin can access this endpoint
        public async Task<IActionResult> DeleteCustomer(int id)
        {
            try
            {
                var customer = await _context.Customers
                    .Include(c => c.Orders)
                    .FirstOrDefaultAsync(c => c.CustomerID == id);

                if (customer == null)
                {
                    _logger.LogWarning(
                        "Forsøg på at slette ikke-eksisterende customer: {Details}", 
                        new { 
                            Operation = "DELETE",
                            EntityType = "Customer",
                            UserId = User.FindFirstValue(ClaimTypes.NameIdentifier),
                            UserName = User.Identity?.Name,
                            Timestamp = DateTime.UtcNow,
                            CustomerID = id
                        });
                    return NotFound();
                }

                _logger.LogInformation(
                    "Sletning af customer påbegyndt: {Details}", 
                    new { 
                        Operation = "DELETE",
                        EntityType = "Customer",
                        UserId = User.FindFirstValue(ClaimTypes.NameIdentifier),
                        UserName = User.Identity?.Name,
                        Timestamp = DateTime.UtcNow,
                        CustomerID = id,
                        AssociatedOrders = customer.Orders?.Count ?? 0
                    });

                _context.Customers.Remove(customer);
                await _context.SaveChangesAsync();

                _logger.LogInformation(
                    "Customer slettet succesfuldt: {Result}", 
                    new { 
                        Operation = "DELETE",
                        EntityType = "Customer",
                        UserId = User.FindFirstValue(ClaimTypes.NameIdentifier),
                        UserName = User.Identity?.Name,
                        Timestamp = DateTime.UtcNow,
                        CustomerID = id
                    });

                return NoContent();
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, 
                    "Fejl ved sletning af customer: {ErrorDetails}", 
                    new { 
                        Operation = "DELETE",
                        EntityType = "Customer",
                        UserId = User.FindFirstValue(ClaimTypes.NameIdentifier),
                        UserName = User.Identity?.Name,
                        Timestamp = DateTime.UtcNow,
                        CustomerID = id,
                        ErrorMessage = ex.Message
                    });
                throw;
            }
        }
    }
}
