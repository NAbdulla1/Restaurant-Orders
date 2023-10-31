using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Restaurant_Orders.Exceptions;
using Restaurant_Orders.Extensions;
using Restaurant_Orders.Models.DTOs;
using Restaurant_Orders.Services;
using RestaurantOrder.Data.Models;
using RestaurantOrder.Data.Repositories;
using System.Net.Mime;

namespace Restaurant_Orders.Controllers
{
    [Route("api/orders")]
    [ApiController]
    public class OrdersController : ControllerBase
    {
        private readonly IOrderRepository _orderRepository;
        private readonly IUserRepository _userRepository;
        private readonly IOrderItemRepository _orderItemRepository;
        private readonly IUserService _userService;
        private readonly IOrderService _orderService;
        private readonly IPaginationService<Order> _paginationService;

        public OrdersController(
            IOrderRepository orderRepository,
            IUserRepository userRepository,
            IOrderItemRepository orderItemRepository,
            IUserService userService,
            IOrderService orderService,
            IPaginationService<Order> paginationService)
        {
            _orderRepository = orderRepository;
            _userRepository = userRepository;
            _orderItemRepository = orderItemRepository;
            _userService = userService;
            _orderService = orderService;
            _paginationService = paginationService;
        }

        [HttpGet]
        [Authorize(Roles = "RestaurantOwner")]
        [Produces(MediaTypeNames.Application.Json)]
        public async Task<ActionResult<PagedData<Order>>> GetOrder([FromQuery] IndexingDTO indexData, [FromQuery] OrderFilterDTO orderFilters)
        {
            if (indexData.SortBy != null && !typeof(Order).FieldExists(indexData.SortBy))
            {
                ModelState.AddModelError(nameof(indexData.SortBy), "Can't find the provided sort property.");
                return ValidationProblem();
            }

            var query = _orderService.PrepareIndexQuery(_orderRepository.GetAll(), indexData, orderFilters);
            var page = await _paginationService.Paginate(query, indexData);

            return Ok(page);
        }

        [HttpGet("customer")]
        [Authorize(Roles = "Customer")]
        [Produces(MediaTypeNames.Application.Json)]
        public async Task<ActionResult<PagedData<Order>>> GetOrder([FromQuery] IndexingDTO indexData)
        {
            if (indexData.SortBy != null && !typeof(Order).FieldExists(indexData.SortBy))
            {
                ModelState.AddModelError(nameof(indexData.SortBy), "Can't find the provided sort property.");
                return ValidationProblem();
            }

            var orderFilters = new OrderFilterDTO { CustomerId = _userService.GetCurrentUser(HttpContext).Id };

            var query = _orderService.PrepareIndexQuery(_orderRepository.GetAll(), indexData, orderFilters);
            var page = await _paginationService.Paginate(query, indexData);

            return Ok(page);
        }

        [HttpGet("{id}")]
        [Authorize(Roles = "RestaurantOwner,Customer")]
        [Produces(MediaTypeNames.Application.Json)]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        [ProducesResponseType(StatusCodes.Status200OK)]
        public async Task<ActionResult<Order>> GetOrder(long id)
        {
            var user = _userService.GetCurrentUser(HttpContext);
            Order? order;
            if (user != null && (await _userRepository.GetById(user.Id))?.UserType == UserType.Customer)
            {
                order = await _orderRepository.GetById(id);
            }
            else
            {
                order = await _orderRepository.GetById(id);
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
        public async Task<ActionResult<Order>> UpdateOrderStatus(long id, OrderStatusDTO orderStatusDTO)
        {
            var order = await _orderRepository.GetById(id);
            if (order == null)
            {
                return Problem(
                    title: "Unable to save changes. The order was deleted by someone else.",
                    statusCode: 404
                );
            }

            return await UpdateStatus(orderStatusDTO, order);
        }

        private async Task<ActionResult<Order>> UpdateStatus(OrderStatusDTO orderStatusDTO, Order order)
        {
            order.Status = orderStatusDTO.Status;
            order.Version = Guid.NewGuid();

            _orderRepository.UpdateOrder(order, orderStatusDTO.Version);
            try
            {
                await _orderRepository.Commit();
                return Ok(order);
            }
            catch (DbUpdateConcurrencyException)
            {
                return await ConcurrentErrorResponse(order.Id);
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
        public async Task<ActionResult<Order>> UpdateOrder(long id, OrderUpdateDTO orderData)
        {
            bool validationProblem = CustomValidation(orderData);

            if (validationProblem)
            {
                return ValidationProblem();
            }

            var order = await _orderRepository.GetById(id);
            if (order == null)
            {
                return Problem(
                    title: "Unable to save changes. The order was deleted by someone else.",
                    statusCode: 404
                );
            }

            if (order.CustomerId != _userService.GetCurrentUser(HttpContext).Id)
            {
                return Problem(title: "Not allowed to update other users order.", statusCode: 403);
            }

            if (order.Status != OrderStatus.CREATED && order.Status != OrderStatus.PROCESSING)
            {
                ModelState.AddModelError(string.Empty, $"Unable to modify order because the order is not in '{OrderStatus.CREATED}' or '{OrderStatus.PROCESSING}' state.");
                return ValidationProblem();
            }

            try
            {
                List<OrderItem> deleteExistingOrderItems = _orderService.RemoveExistingOrderItems(orderData.RemoveMenuItemIds, order);
                _orderItemRepository.RemoveMany(deleteExistingOrderItems);

                await _orderService.AddOrderItems(orderData.AddMenuItemIds, order);
                order.Version = Guid.NewGuid();

                _orderRepository.UpdateOrder(order, orderData.Version.Value);
                await _orderRepository.Commit();
            }
            catch (MenuItemDoesNotExists ex)
            {
                ModelState.AddModelError(nameof(orderData.AddMenuItemIds), ex.Message);
                return ValidationProblem();
            }
            catch (DbUpdateConcurrencyException)
            {
                return await ConcurrentErrorResponse(id);
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
        [Consumes(MediaTypeNames.Application.Json)]
        [Produces(MediaTypeNames.Application.Json)]
        [ProducesResponseType(StatusCodes.Status201Created)]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        public async Task<ActionResult<Order>> CreateOrder(NewOrderDTO newOrderDTO)
        {
            try
            {
                var order = await _orderService.BuildOrder(newOrderDTO.menuItemIds, _userService.GetCurrentUser(HttpContext).Id);

                _orderRepository.Add(order);
                await _orderRepository.Commit();

                return CreatedAtAction(nameof(GetOrder), new { id = order.Id }, order);
            }
            catch (MenuItemDoesNotExists ex)
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
        public async Task<IActionResult> DeleteOrder(long id, Guid version)
        {
            var order = await _orderRepository.GetById(id);
            if (order == null)
            {
                return Problem(
                    title: "Unable to save changes. The order was deleted by someone else.",
                    statusCode: 404
                );
            }

            try
            {
                _orderRepository.Delete(order, version);
                await _orderRepository.Commit();

                return NoContent();
            }
            catch (DbUpdateConcurrencyException)
            {
                return Problem(
                    title: "The record you attempted to delete was modified by another user after you got the original value. Please fetch the order again to get new version.",
                    statusCode: 404
                );
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
        public async Task<ActionResult<Order>> CancelOrder(long id, VersionDTO versionDTO)
        {
            var order = await _orderRepository.GetById(id);
            if (order == null)
            {
                return Problem(
                    title: "Unable to save changes. The order was deleted by someone else.",
                    statusCode: 404
                );
            }

            if (order.CustomerId != _userService.GetCurrentUser(HttpContext).Id)
            {
                return Problem(title: "Not allowed to cancel other users order.", statusCode: 403);
            }

            if (order.Status != OrderStatus.CREATED && order.Status != OrderStatus.PROCESSING)
            {
                ModelState.AddModelError(string.Empty, $"Unable to modify order because the order is not in '{OrderStatus.CREATED}' or '{OrderStatus.PROCESSING}' state.");
                return ValidationProblem();
            }

            return await UpdateStatus(new OrderStatusDTO { Status = OrderStatus.CANCELED, Version = versionDTO.Version }, order);
        }

        [HttpPost("{id}/pay")]
        [Authorize(Roles = "Customer")]
        [Consumes(MediaTypeNames.Application.Json)]
        [Produces(MediaTypeNames.Application.Json)]
        [ProducesResponseType(StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        [ProducesResponseType(StatusCodes.Status403Forbidden)]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        public async Task<ActionResult<Order>> PayOrder(long id, VersionDTO versionDTO)
        {
            var order = await _orderRepository.GetById(id);
            if (order == null)
            {
                return Problem(
                    title: "Unable to save changes. The order was deleted by someone else.",
                    statusCode: 404
                );
            }

            if (order.CustomerId != _userService.GetCurrentUser(HttpContext).Id)
            {
                return Problem(title: "Not allowed to pay other users order.", statusCode: 403);
            }

            if (order.Status != OrderStatus.PROCESSING)
            {
                ModelState.AddModelError(string.Empty, $"Unable to modify order because the order is not in '{OrderStatus.PROCESSING}' state.");
                return ValidationProblem();
            }

            return await UpdateStatus(new OrderStatusDTO { Status = OrderStatus.BILLED, Version = versionDTO.Version }, order);
        }

        private async Task<ActionResult<Order>> ConcurrentErrorResponse(long id)
        {
            if (!(await _orderRepository.OrderExists(id)))
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
