using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Restaurant_Orders.Data;
using Restaurant_Orders.Data.Entities;
using Restaurant_Orders.Models.DTOs;
using Restaurant_Orders.Services;

namespace Restaurant_Orders.Controllers
{
    [ApiController]
    [Route("api/user")]
    public class UserController : Controller
    {
        private readonly RestaurantContext _dbContext;
        private readonly IUserService _userService;

        public UserController(RestaurantContext dbContext, IUserService userService)
        {
            _dbContext = dbContext;
            _userService = userService;
        }

        [HttpPost("register")]
        [ProducesResponseType(StatusCodes.Status201Created, Type = typeof(UserDTO))]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        public async Task<ActionResult> RegisterUser([Bind("FirstName,LastName,Email,Password")] User customer)
        {
            customer = _userService.PrepareCustomer(customer);

            _dbContext.Users.Add(customer);
            await _dbContext.SaveChangesAsync();

            return CreatedAtAction(nameof(RegisterUser), ItemToDTO(customer));
        }

        [HttpPost("login")]
        [ProducesResponseType(StatusCodes.Status200OK, Type = typeof(AccessTokenDTO))]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        [ProducesResponseType(StatusCodes.Status401Unauthorized)]
        public async Task<ActionResult> LoginUser(LoginPayloadDTO loginPayload)
        {
            var savedUser = await _dbContext.Users.Where(u => u.Email == loginPayload.Email).FirstOrDefaultAsync();

            if (savedUser == null)
            {
                return NotFound();
            }

            if (_userService.LoginUser(savedUser, loginPayload.Password))
                return Ok(new AccessTokenDTO { AccessToken = "Correct Credentials" }); //TODO return actual accesstoken
            else
                return Unauthorized();
        }

        private static UserDTO ItemToDTO(User item)
        {
            return new UserDTO()
            {
                Id = item.Id,
                FirstName = item.FirstName,
                LastName = item.LastName,
                Email = item.Email,
            };
        }
    }
}
