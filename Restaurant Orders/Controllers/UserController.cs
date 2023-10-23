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

        [HttpGet]
        [Authorize(Roles = "RestaurantOwner")]
        public ActionResult<IEnumerable<UserDTO>> GetUsers()
        {
            return Ok(_dbContext.Users.Select(user => ToUserDTO(user)).AsEnumerable());
        }

        [HttpPost("register")]
        [Consumes("application/json")]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        public async Task<ActionResult<UserDTO>> RegisterUser([Bind("FirstName,LastName,Email,Password")] User customer)
        {
            customer = _userService.PrepareCustomer(customer);

            _dbContext.Users.Add(customer);
            await _dbContext.SaveChangesAsync();

            return CreatedAtAction(nameof(RegisterUser), ToUserDTO(customer));
        }

        [HttpPost("login")]
        [Consumes("application/json")]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        [ProducesResponseType(StatusCodes.Status401Unauthorized)]
        public async Task<ActionResult<AccessTokenDTO>> LoginUser(LoginPayloadDTO loginPayload)
        {
            var savedUser = await _dbContext.Users.Where(u => u.Email == loginPayload.Email).FirstOrDefaultAsync();

            if (savedUser == null)
            {
                return Unauthorized();
            }

            try
            {
                var token = _userService.SignInUser(savedUser, loginPayload.Password);
                return Ok(new AccessTokenDTO { AccessToken = token });
            }
            catch (UnauthenticatedException)
            {
                return Unauthorized();
            }
        }

        private static UserDTO ToUserDTO(User item)
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
