using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Restaurant_Orders.Data;
using Restaurant_Orders.Data.Entities;
using Restaurant_Orders.Exceptions;
using Restaurant_Orders.Models.DTOs;
using Restaurant_Orders.Services;

namespace Restaurant_Orders.Controllers
{
    [Route("api/orders")]
    [ApiController]
    public class OrdersController : ControllerBase
    {
        private readonly RestaurantContext _context;
        private readonly IUserService _userService;
        private readonly IOrderService _orderService;

        public OrdersController(RestaurantContext context, IUserService userService, IOrderService orderService)
        {
            _context = context;
            _userService = userService;
            _orderService = orderService;
        }

        [HttpGet]
        [Authorize(Roles = "RestaurantOwner,Customer")]
        public async Task<ActionResult<IEnumerable<Order>>> GetOrder()
        {
            if (_context.Orders == null)
            {
                return NotFound();
            }
            return await _context.Orders.Include("OrderItems").ToListAsync();
        }

        [HttpGet("{id}")]
        [Authorize(Roles = "RestaurantOwner,Customer")]
        public async Task<ActionResult<Order>> GetOrder(long id)
        {
            var order = await _context.Orders.Include("OrderItems").FirstOrDefaultAsync(o => o.Id == id);

            if (order == null)
            {
                return NotFound();
            }

            return order;
        }

        [HttpPatch("{id}")]
        [Authorize(Roles = "Customer")]
        public async Task<IActionResult> UpdateOrder(long id, OrderUpdateDTO orderData)
        {
            bool validationProblem = CustomValidation(orderData);

            if (validationProblem)
            {
                return ValidationProblem();
            }

            var order = await _context.Orders.Include("OrderItems").FirstOrDefaultAsync(o => o.Id == id);
            if (order == null)
            {
                return Problem(
                    title: "Unable to save changes. The order was deleted by someone else.",
                    statusCode: 404
                );
            }

            try
            {
                List<OrderItem> deleteExistingOrderItems = _orderService.RemoveExistingOrderItems(orderData.RemoveMenuItemIds, order);
                _context.OrderItems.RemoveRange(deleteExistingOrderItems);

                await _orderService.AddOrderItems(orderData.AddMenuItemIds, order);
                order.Version = Guid.NewGuid();

                _context.Entry(order).State = EntityState.Modified;

#pragma warning disable 8629
                _context.Entry(order).Property("Version").OriginalValue = orderData.Version.Value;
#pragma warning restore 8629

                await _context.SaveChangesAsync();
            }
            catch (MenuItemDoesNotExists ex)
            {
                ModelState.AddModelError(nameof(orderData.AddMenuItemIds), ex.Message);
                return ValidationProblem();
            }
            catch (DbUpdateConcurrencyException)
            {
                if (!OrderExists(id))
                {
                    return Problem(
                        title: "Unable to save changes. The order was deleted by someone else.",
                        statusCode: 404
                    );
                }
                else
                {
                    return Problem(
                        title: "The record you attempted to edit was modified by another user after you got the original value."
                    );
                }
            }

            return Ok(order);
        }

        private bool CustomValidation(OrderUpdateDTO orderData)
        {
            var validationProblem = false;
            if (!orderData.AddMenuItemIds.Any() && !orderData.RemoveMenuItemIds.Any())
            {
                ModelState.AddModelError(nameof(orderData.AddMenuItemIds), $"Either of {nameof(orderData.AddMenuItemIds)} or {nameof(orderData.RemoveMenuItemIds)} field is required.");
                ModelState.AddModelError(nameof(orderData.RemoveMenuItemIds), $"Either of {nameof(orderData.RemoveMenuItemIds)} or {nameof(orderData.AddMenuItemIds)} field is required.");
                validationProblem = true;
            }

            if (orderData.Version == null)
            {
                ModelState.AddModelError(nameof(orderData.Version), $"The {nameof(orderData.Version)} field is required.");
                validationProblem = true;
            }

            return validationProblem;
        }

        [HttpPost]
        [Authorize(Roles = "Customer")]
        public async Task<ActionResult<Order>> CreateOrder(NewOrderDTO newOrderDTO)
        {
            try
            {
                var order = await _orderService.BuildOrder(newOrderDTO.menuItemIds, _userService.GetCurrentUser(HttpContext).Id);

                _context.Orders.Add(order);
                await _context.SaveChangesAsync();

                return CreatedAtAction(nameof(GetOrder), new { id = order.Id }, order);
            }
            catch (MenuItemDoesNotExists ex)
            {
                ModelState.AddModelError(nameof(newOrderDTO.menuItemIds), ex.Message);
                return ValidationProblem();
            }
        }

        [HttpDelete("{id}/{version:Guid?}")]
        [Authorize(Roles = "RestaurantOwner")]
        public async Task<IActionResult> DeleteOrder(long id, Guid? version)
        {
            var order = await _context.Orders.FindAsync(id);
            if (order == null)
            {
                return Problem(
                    title: "Unable to save changes. The order was deleted by someone else.",
                    statusCode: 404
                );
            }

            try
            {
                _context.Entry(order).State = EntityState.Deleted;
                if (version != null)
                {
                    _context.Entry(order).Property("Version").OriginalValue = version;
                }
                await _context.SaveChangesAsync();

                return NoContent();
            }
            catch (DbUpdateConcurrencyException)
            {
                return Problem(
                    title: "The record you attempted to delete was modified by another user after you got the original value. Please fetch the order again to get new version."
                );
            }
        }

        private bool OrderExists(long id)
        {
            return (_context.Orders?.Any(e => e.Id == id)).GetValueOrDefault();
        }
    }
}
