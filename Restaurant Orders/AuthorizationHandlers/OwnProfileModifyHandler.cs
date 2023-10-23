using Microsoft.AspNetCore.Authorization;
using Restaurant_Orders.Models.DTOs;
using System.Security.Claims;
using System.Text.Json;

namespace Restaurant_Orders.Authorizations
{
    public class OwnProfileModifyRequirement : IAuthorizationRequirement
    {
        public const string OwnPMR = "GetOrUpdateOwnProfileInfo";
    }

    public class OwnProfileModifyHandler : AuthorizationHandler<OwnProfileModifyRequirement>
    {
        protected override Task HandleRequirementAsync(AuthorizationHandlerContext context, OwnProfileModifyRequirement requirement)
        {
            if (!context.User.HasClaim(c => c.Type == ClaimTypes.UserData))
            {
                context.Fail();
                return Task.CompletedTask;
            }

            if (context.Resource is not HttpContext httpContext)
            {
                return Task.CompletedTask;
            }

            var userData = JsonSerializer.Deserialize<UserDTO>(context.User.FindFirstValue(ClaimTypes.UserData));
            var idPathParamValue = httpContext.Request.RouteValues["id"] as string;
            if (idPathParamValue == null)
            {
                context.Fail();
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
