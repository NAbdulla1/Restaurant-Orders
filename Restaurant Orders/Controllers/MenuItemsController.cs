using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Restaurant_Orders.Data;
using Restaurant_Orders.Data.Entities;
using Restaurant_Orders.Models.DTOs;

namespace Restaurant_Orders.Controllers
{
    [ApiController]
    [Route("api/menu-items")]
    public class MenuItemsController : ControllerBase
    {
        private readonly RestaurantContext _context;
        private readonly int _defaultPageSize;

        public MenuItemsController(RestaurantContext context, IConfiguration configuration)
        {
            _context = context;
            _defaultPageSize = configuration.GetValue<int>("DefaultPageSize");
        }

        [HttpGet]
        [Authorize(Roles = "RestaurantOwner,Customer")]
        public async Task<ActionResult<PagedData<MenuItem>>> GetMenuItems([FromQuery] IndexingDTO indexData)
        {
            int take = indexData.PageSize ?? _defaultPageSize;
            int skip = (indexData.Page - 1) * take;

            return Ok(new PagedData<MenuItem>
            {
                Page = indexData.Page,
                PageSize = take,
                Total = await _context.MenuItems.LongCountAsync(),
                PageData = _context.MenuItems.Skip(skip)
                .Take(take)
                .AsAsyncEnumerable()
            });
        }

        [HttpGet("{id:long}")]
        [Authorize(Roles = "RestaurantOwner,Customer")]
        public async Task<ActionResult<MenuItem>> GetMenuItem(long id)
        {
            var menuItem = await _context.MenuItems.FindAsync(id);

            if (menuItem == null)
            {
                return NotFound();
            }

            return menuItem;
        }

        [HttpPut("{id:long}")]
        [Authorize(Roles = "RestaurantOwner")]
        public async Task<IActionResult> UpdateMenuItem(long id, MenuItemDTO menuItemDto)
        {
            var menuItem = new MenuItem
            {
                Id = id,
                Name = menuItemDto.Name,
                Description = menuItemDto.Description,
                Price = menuItemDto.Price
            };

            _context.MenuItems.Attach(menuItem);

            try
            {
                await _context.SaveChangesAsync();
            }
            catch (DbUpdateConcurrencyException)
            {
                if (!MenuItemExists(id))
                {
                    return NotFound();
                }
                else
                {
                    throw;
                }
            }

            return Ok(menuItem);
        }

        [HttpPost]
        [Authorize(Roles = "RestaurantOwner")]
        public async Task<ActionResult<MenuItem>> CreateMenuItem(MenuItemDTO menuItemDto)
        {
            var menuItem = new MenuItem
            {
                Name = menuItemDto.Name,
                Description = menuItemDto.Description,
                Price = menuItemDto.Price
            };

            _context.MenuItems.Add(menuItem);
            await _context.SaveChangesAsync();

            return CreatedAtAction(nameof(GetMenuItem), new { id = menuItem.Id }, menuItem);
        }

        [HttpDelete("{id:long}")]
        [Authorize(Roles = "RestaurantOwner")]
        public async Task<IActionResult> DeleteMenuItem(long id)
        {
            var menuItem = await _context.MenuItems.FindAsync(id);
            if (menuItem == null)
            {
                return NotFound();
            }

            _context.MenuItems.Remove(menuItem);
            await _context.SaveChangesAsync();

            return NoContent();
        }

        private bool MenuItemExists(long id)
        {
            return (_context.MenuItems?.Any(e => e.Id == id)).GetValueOrDefault();
        }
    }
}
