using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Security.Claims;
using System.Text;
using System.Threading.Tasks;

namespace Application.Utilities
{
    public static class ClaimsPrincipalExtensions
    {
        public static int GetCurrentUserId(this ClaimsPrincipal user)
        {
            return int.Parse(user.FindFirst("UserId")?.Value ?? throw new UnauthorizedAccessException("User ID not found"));
        }
    }
}
