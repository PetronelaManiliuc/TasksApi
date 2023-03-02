using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using System.Security.Claims;

namespace TasksApi.Controllers
{
    public class BaseApiController : ControllerBase
    {
        public int UserId
        {
            get
            {
               return Convert.ToInt32(FindClaim(ClaimTypes.NameIdentifier));
            }
        }

        private string FindClaim(string claimName)
        {
            var claim = (HttpContext.User.Identity as ClaimsIdentity).FindFirst(claimName);

            if(claim == null)
            {
                return null;
            }

            return claim.Value;
        }
    }
}
