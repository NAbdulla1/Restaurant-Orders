using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Restaurant_Orders.Authorizations;
using Restaurant_Orders.Services;
using RestaurantOrder.Core.DTOs;
using RestaurantOrder.Core.Exceptions;
using System.Net.Mime;

namespace Restaurant_Orders.Controllers
{
    [ApiController]
    [Route("api/user")]
    public class UserController : Controller
    {
        private readonly IUserService _userService;

        public UserController(IUserService userService)
        {
            _userService = userService;
        }

        [HttpGet("{id:long}")]
        [Authorize(Policy = OwnProfileModifyRequirement.Name)]
        [Consumes(MediaTypeNames.Application.Json)]
        [Produces(MediaTypeNames.Application.Json)]
        [ProducesResponseType(StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        [ProducesResponseType(StatusCodes.Status403Forbidden)]
        public async Task<ActionResult<UserDTO>> GetUser(long id)
        {
            try
            {
                var user = await _userService.GetUser(id);
                return Ok(user);
            }
            catch (UserNotFoundException)
            {
                return NotFound();
            }
        }

        [HttpPut("{id:long}")]
        [Authorize(Policy = OwnProfileModifyRequirement.Name)]
        [Consumes(MediaTypeNames.Application.Json)]
        [Produces(MediaTypeNames.Application.Json)]
        [ProducesResponseType(StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        [ProducesResponseType(StatusCodes.Status403Forbidden)]
        public async Task<ActionResult<UserDTO>> UpdateUser(long id, UserUpdateDTO userUpdateDTO)
        {
            try
            {
                var user = await _userService.UpdateUser(id, userUpdateDTO);
                return Ok(user);
            }
            catch(UserNotFoundException)
            {
                return NotFound();
            }
        }

        [HttpPost("register")]
        [Consumes(MediaTypeNames.Application.Json)]
        [Produces(MediaTypeNames.Application.Json)]
        [ProducesResponseType(StatusCodes.Status201Created)]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        public async Task<ActionResult<UserDTO>> RegisterCustomer(CustomerRegisterDTO customer)
        {
            try
            {
                var newCustomer = await _userService.CreateCustomer(customer);

                return CreatedAtAction(nameof(GetUser), new { id = newCustomer.Id }, newCustomer);
            }
            catch (CustomerAlreadyExistsException ex)
            {
                ModelState.AddModelError(nameof(customer.Email), ex.Message);
                return ValidationProblem();
            }
        }

        [HttpPost("login")]
        [Consumes(MediaTypeNames.Application.Json)]
        [Produces(MediaTypeNames.Application.Json)]
        [ProducesResponseType(StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status401Unauthorized)]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        public async Task<ActionResult<AccessTokenDTO>> LoginUser(LoginPayloadDTO loginPayload)
        {
            try
            {
                var accessToken = await _userService.SignInUser(loginPayload);
                return Ok(accessToken);
            }
            catch (UnauthenticatedException)
            {
                return Unauthorized();
            }
        }
    }
}
