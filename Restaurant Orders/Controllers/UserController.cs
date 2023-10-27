using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Restaurant_Orders.Authorizations;
using Restaurant_Orders.Data;
using Restaurant_Orders.Exceptions;
using Restaurant_Orders.Extensions;
using Restaurant_Orders.Models;
using Restaurant_Orders.Models.DTOs;
using Restaurant_Orders.Services;
using System.Net.Mime;

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
        [Produces(MediaTypeNames.Application.Json)]
        public ActionResult<IAsyncEnumerable<UserDTO>> GetUsers()
        {
            return Ok(_dbContext.Users.Select(user => user.ToUserDTO()).AsAsyncEnumerable());
        }

        [HttpGet("{id:long}")]
        [Authorize(Policy = OwnProfileModifyRequirement.Name)]
        [Consumes(MediaTypeNames.Application.Json)]
        [Produces(MediaTypeNames.Application.Json)]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        public ActionResult<UserDTO> GetUser(long id) {
            var user = _dbContext.Users.FirstOrDefault(x => x.Id == id);
            if(user == null)
            {
                return NotFound();
            }

            return Ok(user.ToUserDTO());
        }

        [HttpPut("{id:long}")]
        [Authorize(Policy = OwnProfileModifyRequirement.Name)]
        [Consumes(MediaTypeNames.Application.Json)]
        [Produces(MediaTypeNames.Application.Json)]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        public async Task<ActionResult<UserDTO>> UpdateUser(long id, UserUpdateDTO userUpdate)
        {
            var user = _dbContext.Users.FirstOrDefault(x => x.Id == id);
            if (user == null)
            {
                return NotFound();
            }

            _userService.PrepareUserUpdate(user, userUpdate);

            await _dbContext.SaveChangesAsync();

            return Ok(user.ToUserDTO());
        }

        [HttpPost("register")]
        [Consumes(MediaTypeNames.Application.Json)]
        [Produces(MediaTypeNames.Application.Json)]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        public async Task<ActionResult<UserDTO>> RegisterUser([Bind("FirstName,LastName,Email,Password")] User customer)
        {
            var existingUser = _dbContext.Users.FirstOrDefault(user => user.Email == customer.Email);
            if(existingUser != null)
            {
                ModelState.AddModelError(nameof(customer.Email), $"Another user exists with the given email: '{customer.Email}'.");
                return ValidationProblem();
            }

            customer = _userService.PrepareCustomer(customer);

            _dbContext.Users.Add(customer);
            await _dbContext.SaveChangesAsync();

            return CreatedAtAction(nameof(GetUser), new { id = customer.Id }, customer.ToUserDTO());
        }

        [HttpPost("login")]
        [Consumes(MediaTypeNames.Application.Json)]
        [Produces(MediaTypeNames.Application.Json)]
        [ProducesResponseType(StatusCodes.Status401Unauthorized)]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
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
    }
}
