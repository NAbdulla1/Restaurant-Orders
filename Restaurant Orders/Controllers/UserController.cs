using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Restaurant_Orders.Authorizations;
using Restaurant_Orders.Exceptions;
using Restaurant_Orders.Extensions;
using Restaurant_Orders.Models.DTOs;
using Restaurant_Orders.Services;
using RestaurantOrder.Data.Models;
using RestaurantOrder.Data.Repositories;
using System.Net.Mime;

namespace Restaurant_Orders.Controllers
{
    [ApiController]
    [Route("api/user")]
    public class UserController : Controller
    {
        private readonly IUserRepository _userRepository;
        private readonly IUserService _userService;

        public UserController(IUserRepository userRepository, IUserService userService)
        {
            _userRepository = userRepository;
            _userService = userService;
        }

        [HttpGet]
        [Authorize(Roles = "RestaurantOwner")]
        [Produces(MediaTypeNames.Application.Json)]
        public async Task<ActionResult<IAsyncEnumerable<UserDTO>>> GetUsers()
        {
            return Ok((await _userRepository.GetAll()).Select(user => user.ToUserDTO()));
        }

        [HttpGet("{id:long}")]
        [Authorize(Policy = OwnProfileModifyRequirement.Name)]
        [Consumes(MediaTypeNames.Application.Json)]
        [Produces(MediaTypeNames.Application.Json)]
        [ProducesResponseType(StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        [ProducesResponseType(StatusCodes.Status403Forbidden)]
        public async Task<ActionResult<UserDTO>> GetUser(long id) {
            var user = await _userRepository.GetById(id);
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
        [ProducesResponseType(StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        [ProducesResponseType(StatusCodes.Status403Forbidden)]
        public async Task<ActionResult<UserDTO>> UpdateUser(long id, UserUpdateDTO userUpdate)
        {
            var user = await _userRepository.GetById(id);
            if (user == null)
            {
                return NotFound();
            }

            _userService.PrepareUserUpdate(user, userUpdate);

            await _userRepository.Commit();

            return Ok(user.ToUserDTO());
        }

        [HttpPost("register")]
        [Consumes(MediaTypeNames.Application.Json)]
        [Produces(MediaTypeNames.Application.Json)]
        [ProducesResponseType(StatusCodes.Status201Created)]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        public async Task<ActionResult<UserDTO>> RegisterUser([Bind("FirstName,LastName,Email,Password")] User customer)
        {
            var existingUser = _userRepository.GetByEmail(customer.Email);
            if(existingUser != null)
            {
                ModelState.AddModelError(nameof(customer.Email), $"Another user exists with the given email: '{customer.Email}'.");
                return ValidationProblem();
            }

            customer = _userService.PrepareCustomer(customer);

            _userRepository.Add(customer);
            await _userRepository.Commit();

            return CreatedAtAction(nameof(GetUser), new { id = customer.Id }, customer.ToUserDTO());
        }

        [HttpPost("login")]
        [Consumes(MediaTypeNames.Application.Json)]
        [Produces(MediaTypeNames.Application.Json)]
        [ProducesResponseType(StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status401Unauthorized)]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        public async Task<ActionResult<AccessTokenDTO>> LoginUser(LoginPayloadDTO loginPayload)
        {
            var savedUser = await _userRepository.GetByEmail(loginPayload.Email);

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
