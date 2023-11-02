using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Restaurant_Orders.Extensions;
using Restaurant_Orders.Services;
using RestaurantOrder.Core.DTOs;
using RestaurantOrder.Core.Exceptions;
using RestaurantOrder.Data.Models;
using System.Net.Mime;

namespace Restaurant_Orders.Controllers
{
    [Route("api/orders")]
    [ApiController]
    public class OrdersController : ControllerBase
    {
        private readonly IUserService _userService;
        private readonly IOrderService _orderService;

        public OrdersController(
            IUserService userService,
            IOrderService orderService)
        {
            _userService = userService;
            _orderService = orderService;
        }

        [HttpGet]
        [Authorize(Roles = "RestaurantOwner")]
        [Produces(MediaTypeNames.Application.Json)]
        public async Task<ActionResult<PagedData<OrderDTO>>> GetOrder([FromQuery] IndexingDTO indexData, [FromQuery] OrderFilterDTO orderFilters)
        {
            if (indexData.SortBy != null && !typeof(OrderDTO).FieldExists(indexData.SortBy))
            {
                ModelState.AddModelError(nameof(indexData.SortBy), $"Can't find the provided sort property: '{indexData.SortBy}'.");
                return ValidationProblem();
            }

            var page = await _orderService.Get(indexData, orderFilters);

            return Ok(page);
        }

        [HttpGet("customer")]
        [Authorize(Roles = "Customer")]
        [Produces(MediaTypeNames.Application.Json)]
        public async Task<ActionResult<PagedData<OrderDTO>>> GetOrder([FromQuery] IndexingDTO indexData)
        {
            if (indexData.SortBy != null && !typeof(OrderDTO).FieldExists(indexData.SortBy))
            {
                ModelState.AddModelError(nameof(indexData.SortBy), $"Can't find the provided sort property: '{indexData.SortBy}'.");
                return ValidationProblem();
            }

            var orderFilters = new OrderFilterDTO { CustomerId = _userService.GetCurrentAuthenticatedUser(HttpContext).Id };

            var page = await _orderService.Get(indexData, orderFilters);

            return Ok(page);
        }

        [HttpGet("{id}")]
        [Authorize(Roles = "RestaurantOwner,Customer")]
        [Produces(MediaTypeNames.Application.Json)]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        [ProducesResponseType(StatusCodes.Status200OK)]
        public async Task<ActionResult<OrderDTO>> GetOrder(long id)
        {
            var user = _userService.GetCurrentAuthenticatedUser(HttpContext);

            OrderDTO? order;
            if (user != null && user.UserType == UserType.Customer.ToString())
            {
                order = await _orderService.GetById(id, user.Id);
            }
            else
            {
                order = await _orderService.GetById(id);
            }

            if (order == null)
            {
                return NotFound();
            }

            return order;
        }

        [HttpPatch("{id}/status")]
        [Authorize(Roles = "RestaurantOwner")]
        [Consumes(MediaTypeNames.Application.Json)]
        [Produces(MediaTypeNames.Application.Json)]
        [ProducesResponseType(StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        public async Task<ActionResult<OrderDTO>> UpdateOrderStatus(long id, OrderStatusDTO orderStatusDTO)
        {
            try
            {
                var order = await _orderService.UpdateStatus(id, orderStatusDTO.Status, orderStatusDTO.Version);
                return Ok(order);
            }
            catch (OrderNotFoundException)
            {
                return Problem(
                    title: "Unable to save changes. The order is not found.",
                    statusCode: 404
                );
            }
            catch (DbUpdateConcurrencyException)
            {
                return await ConcurrentErrorResponse(id);
            }
        }

        [HttpPatch("{id}")]
        [Authorize(Roles = "Customer")]
        [Consumes(MediaTypeNames.Application.Json)]
        [Produces(MediaTypeNames.Application.Json)]
        [ProducesResponseType(StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        [ProducesResponseType(StatusCodes.Status403Forbidden)]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        public async Task<ActionResult<OrderDTO>> UpdateOrder(long id, OrderUpdateDTO orderData)
        {
            var order = await _orderService.GetById(id);
            if (order == null)
            {
                return Problem(
                    title: "Unable to save changes. The order is not found.",
                    statusCode: 404
                );
            }

            if (order.CustomerId != _userService.GetCurrentAuthenticatedUser(HttpContext).Id)
            {
                return Problem(title: "Not allowed to update other users order.", statusCode: 403);
            }

            if (order.Status != OrderStatus.CREATED.ToString() && order.Status != OrderStatus.PROCESSING.ToString())
            {
                ModelState.AddModelError(string.Empty, $"Unable to modify order because the order is not in '{OrderStatus.CREATED}' or '{OrderStatus.PROCESSING}' state.");
                return ValidationProblem();
            }

            try
            {
                order = await _orderService.UpdateOrderItems(order, orderData);

                return Ok(order);
            }
            catch (MenuItemNotFountException ex)
            {
                ModelState.AddModelError(nameof(orderData.AddMenuItemIds), ex.Message);
                return ValidationProblem();
            }
            catch (DbUpdateConcurrencyException)
            {
                return await ConcurrentErrorResponse(id);
            }
        }

        [HttpPost]
        [Authorize(Roles = "Customer")]
        [Consumes(MediaTypeNames.Application.Json)]
        [Produces(MediaTypeNames.Application.Json)]
        [ProducesResponseType(StatusCodes.Status201Created)]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        public async Task<ActionResult<OrderDTO>> CreateOrder(NewOrderDTO newOrderDTO)
        {
            try
            {
                var order = await _orderService.Create(newOrderDTO.menuItemIds, _userService.GetCurrentAuthenticatedUser(HttpContext).Id);

                return CreatedAtAction(nameof(GetOrder), new { id = order.Id }, order);
            }
            catch (MenuItemNotFountException ex)
            {
                ModelState.AddModelError(nameof(newOrderDTO.menuItemIds), ex.Message);
                return ValidationProblem();
            }
        }

        [HttpDelete("{id}/{version:Guid}")]
        [Authorize(Roles = "RestaurantOwner")]
        [Produces(MediaTypeNames.Application.Json)]
        [ProducesResponseType(StatusCodes.Status204NoContent)]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        public async Task<ActionResult> DeleteOrder(long id, Guid version)
        {
            try
            {
                await _orderService.Delete(id, version);

                return NoContent();
            }
            catch (Exception ex) when (ex is OrderNotFoundException || ex is DbUpdateConcurrencyException)
            {
                return await ConcurrentErrorResponse(id);
            }
        }

        [HttpPost("{id}/cancel")]
        [Authorize(Roles = "Customer")]
        [Consumes(MediaTypeNames.Application.Json)]
        [Produces(MediaTypeNames.Application.Json)]
        [ProducesResponseType(StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        [ProducesResponseType(StatusCodes.Status403Forbidden)]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        public async Task<ActionResult<OrderDTO>> CancelOrder(long id, VersionDTO versionDTO)
        {
            var order = await _orderService.GetById(id);
            if (order == null)
            {
                return Problem(
                    title: "Unable to save changes. The order was deleted by someone else.",
                    statusCode: 404
                );
            }

            if (order.CustomerId != _userService.GetCurrentAuthenticatedUser(HttpContext).Id)
            {
                return Problem(title: "Not allowed to cancel other users order.", statusCode: 403);
            }

            if (order.Status != OrderStatus.CREATED.ToString() && order.Status != OrderStatus.PROCESSING.ToString())
            {
                ModelState.AddModelError(string.Empty, $"Unable to modify order because the order is not in '{OrderStatus.CREATED}' or '{OrderStatus.PROCESSING}' state.");
                return ValidationProblem();
            }

            try
            {
                order = await _orderService.UpdateStatus(order, OrderStatus.CANCELED, versionDTO.Version);

                return Ok(order);
            }
            catch (DbUpdateConcurrencyException)
            {
                return await ConcurrentErrorResponse(id);
            }
        }

        [HttpPost("{id}/pay")]
        [Authorize(Roles = "Customer")]
        [Consumes(MediaTypeNames.Application.Json)]
        [Produces(MediaTypeNames.Application.Json)]
        [ProducesResponseType(StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        [ProducesResponseType(StatusCodes.Status403Forbidden)]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        public async Task<ActionResult<OrderDTO>> PayOrder(long id, VersionDTO versionDTO)
        {
            var order = await _orderService.GetById(id);
            if (order == null)
            {
                return Problem(
                    title: "Unable to save changes. The order was deleted by someone else.",
                    statusCode: 404
                );
            }

            if (order.CustomerId != _userService.GetCurrentAuthenticatedUser(HttpContext).Id)
            {
                return Problem(title: "Not allowed to pay other users order.", statusCode: 403);
            }

            if (order.Status != OrderStatus.PROCESSING.ToString())
            {
                ModelState.AddModelError(string.Empty, $"Unable to modify order because the order is not in '{OrderStatus.PROCESSING}' state.");
                return ValidationProblem();
            }

            try
            {
                order = await _orderService.UpdateStatus(order, OrderStatus.BILLED, versionDTO.Version);

                return Ok(order);
            }
            catch (DbUpdateConcurrencyException)
            {
                return await ConcurrentErrorResponse(id);
            }
        }

        private async Task<ActionResult> ConcurrentErrorResponse(long id)
        {
            if (!await _orderService.IsOrderExists(id))
            {
                return Problem(
                    title: "Unable to save changes. The order was deleted by someone else.",
                    statusCode: 404
                );
            }
            else
            {
                return Problem(
                    title: "The record you attempted to edit was modified by another user after you got the original value.",
                    statusCode: 400
                );
            }
        }
    }
}
