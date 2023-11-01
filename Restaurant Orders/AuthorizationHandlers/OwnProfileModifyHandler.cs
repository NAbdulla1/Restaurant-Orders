using Microsoft.AspNetCore.Authorization;
using Restaurant_Orders.Services;
using System.Security.Claims;

namespace Restaurant_Orders.Authorizations
{
    public class OwnProfileModifyRequirement : IAuthorizationRequirement
    {
        public const string Name = "GetOrUpdateOwnProfileInfo";
    }

    public class OwnProfileModifyHandler : AuthorizationHandler<OwnProfileModifyRequirement>
    {
        private readonly IUserService _userService;

        public OwnProfileModifyHandler(IUserService userService)
        {
            _userService = userService;
        }

        protected override Task HandleRequirementAsync(AuthorizationHandlerContext context, OwnProfileModifyRequirement requirement)
        {
            if (!context.User.HasClaim(c => c.Type == ClaimTypes.UserData))
            {
                return Task.CompletedTask;
            }

            if (context.Resource is not HttpContext httpContext)
            {
                return Task.CompletedTask;
            }

            var userData = _userService.GetCurrentAuthenticatedUser(httpContext);
            var idPathParamValue = httpContext.Request.RouteValues["id"] as string;
            if (idPathParamValue == null)
            {
                return Task.CompletedTask;
            }

            if (int.TryParse(idPathParamValue, out int idInRoute) && idInRoute == userData?.Id)
            {
                context.Succeed(requirement);
            }
            else
            {
                context.Fail();
            }

            return Task.CompletedTask;
        }
    }
}
