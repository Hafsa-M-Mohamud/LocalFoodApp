using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Assignment3BAD.Database;
using Assignment3BAD.Models;
using Microsoft.Extensions.Logging;
using System.Security.Claims;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace Assignment3BAD.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    [Authorize]
    public class OrdersController : ControllerBase
    {
        private readonly MyDBContext _context;
        private readonly ILogger<OrdersController> _logger;

        public OrdersController(MyDBContext context, ILogger<OrdersController> logger)
        {
            _context = context;
            _logger = logger;
        }

        // Get all orders
        [HttpGet]
        [Authorize(Roles = "Admin")] // Only Admin can access this endpoint
        public async Task<ActionResult<IEnumerable<Order>>> GetAllOrders()
        {
            return await _context.Orders.ToListAsync();
        }

        // Get order by ID
        [HttpGet("{id}")]
        [Authorize(Roles = "Admin")] // Only Admin can access this endpoint
        public async Task<ActionResult<Order>> GetOrderById(int id)
        {
            var order = await _context.Orders.FindAsync(id);

            if (order == null)
            {
                return NotFound();
            }

            return Ok(order);
        }

        // Get all dishes in an order, along with the cooks who made them
        [HttpGet("{orderId}/dishes-with-cooks")]
        [Authorize(Roles = "Admin")] // Only Admin can access this endpoint
        public async Task<ActionResult<IEnumerable<object>>> GetDishesAndCooksInOrder(int orderId)
        {
            var orderDetails = await _context
                .DishOrders.Where(dishOrder => dishOrder.OrderID == orderId)
                .Include(dishOrder => dishOrder.Dish)
                .ThenInclude(dish => dish.Cook)
                .Select(dishOrder => new
                {
                    DishName = dishOrder.Dish.Name,
                    CookName = dishOrder.Dish.Cook.Name,
                    Quantity = dishOrder.Quantity,
                })
                .ToListAsync();

            if (!orderDetails.Any())
            {
                return NotFound("No dishes or cooks found for this order.");
            }

            return Ok(orderDetails);
        }

        // Create a new order
        [HttpPost]
        public async Task<ActionResult<Order>> CreateOrder(Order order)
        {
            try 
            {
                if (order.OrderTime == default)
                {
                    order.OrderTime = DateTime.Now;
                }
                
                _logger.LogInformation("Oprettelse af ny ordre påbegyndt: {OrderDetails}", 
                    new { CustomerID = order.CustomerID, DishID = order.DishID, OrderTime = order.OrderTimeFormatted });
                
                _context.Orders.Add(order);
                await _context.SaveChangesAsync();
                
                _logger.LogInformation("Ordre oprettet succesfuldt med ID: {OrderID}", order.OrderID);
                
                return CreatedAtAction(nameof(GetOrderById), new { id = order.OrderID }, order);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Fejl ved oprettelse af ordre");
                throw;
            }
        }

        // Update an existing order
        [HttpPut("{id}")]
        [Authorize(Roles = "Admin")] // Only Admin can access this endpoint
        public async Task<IActionResult> UpdateOrder(int id, Order updatedOrder)
        {
            try
            {
                if (id != updatedOrder.OrderID)
                {
                    _logger.LogWarning(
                        "Mismatch i order ID ved opdatering: {Details}", 
                        new { 
                            Operation = "PUT",
                            EntityType = "Order",
                            UserId = User.FindFirstValue(ClaimTypes.NameIdentifier),
                            UserName = User.Identity?.Name,
                            Timestamp = DateTime.UtcNow,
                            RequestedId = id,
                            ProvidedId = updatedOrder.OrderID
                        });
                    return BadRequest("Order ID mismatch.");
                }

                var existingOrder = await _context.Orders
                    .Include(o => o.Customer)
                    .Include(o => o.Dish)
                    .FirstOrDefaultAsync(o => o.OrderID == id);
                    
                if (existingOrder == null)
                {
                    _logger.LogWarning(
                        "Forsøg på at opdatere ikke-eksisterende order: {Details}", 
                        new { 
                            Operation = "PUT",
                            EntityType = "Order",
                            UserId = User.FindFirstValue(ClaimTypes.NameIdentifier),
                            UserName = User.Identity?.Name,
                            Timestamp = DateTime.UtcNow,
                            OrderID = id
                        });
                    return NotFound();
                }

                _logger.LogInformation(
                    "Opdatering af order påbegyndt: {Details}", 
                    new { 
                        Operation = "PUT",
                        EntityType = "Order",
                        UserId = User.FindFirstValue(ClaimTypes.NameIdentifier),
                        UserName = User.Identity?.Name,
                        Timestamp = DateTime.UtcNow,
                        OrderID = id,
                        OldOrderTime = existingOrder.OrderTime,
                        NewOrderTime = updatedOrder.OrderTime,
                        OldCustomerId = existingOrder.CustomerID,
                        NewCustomerId = updatedOrder.CustomerID,
                        OldDishId = existingOrder.DishID,
                        NewDishId = updatedOrder.DishID
                    });

                _context.Entry(existingOrder).CurrentValues.SetValues(updatedOrder);
                await _context.SaveChangesAsync();

                _logger.LogInformation(
                    "Order opdateret succesfuldt: {Result}", 
                    new { 
                        Operation = "PUT",
                        EntityType = "Order",
                        UserId = User.FindFirstValue(ClaimTypes.NameIdentifier),
                        UserName = User.Identity?.Name,
                        Timestamp = DateTime.UtcNow,
                        OrderID = id
                    });

                return NoContent();
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, 
                    "Fejl ved opdatering af order: {ErrorDetails}", 
                    new { 
                        Operation = "PUT",
                        EntityType = "Order",
                        UserId = User.FindFirstValue(ClaimTypes.NameIdentifier),
                        UserName = User.Identity?.Name,
                        Timestamp = DateTime.UtcNow,
                        OrderID = id,
                        ErrorMessage = ex.Message
                    });
                throw;
            }
        }

        // Delete an order
        [HttpDelete("{id}")]
        [Authorize(Roles = "Admin")] // Only Admin can access this endpoint
        public async Task<IActionResult> DeleteOrder(int id)
        {
            try
            {
                var order = await _context.Orders
                    .Include(o => o.DishOrders)
                    .FirstOrDefaultAsync(o => o.OrderID == id);

                if (order == null)
                {
                    _logger.LogWarning("Forsøg på at slette ikke-eksisterende ordre med ID: {OrderID}", id);
                    return NotFound();
                }

                _logger.LogInformation("Sletning af ordre {OrderID} påbegyndt", id);
                
                if (order.DishOrders != null && order.DishOrders.Any())
                {
                    _logger.LogInformation("Sletter {Count} DishOrders for ordre {OrderID}", 
                        order.DishOrders.Count, id);
                    _context.DishOrders.RemoveRange(order.DishOrders);
                }
                
                _context.Orders.Remove(order);
                await _context.SaveChangesAsync();
                
                _logger.LogInformation("Ordre {OrderID} blev slettet succesfuldt", id);
                return NoContent();
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Fejl ved sletning af ordre {OrderID}", id);
                throw;
            }
        }

    }

}
