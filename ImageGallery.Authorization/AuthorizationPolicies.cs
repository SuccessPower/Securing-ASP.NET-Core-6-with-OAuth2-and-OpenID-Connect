using Microsoft.AspNetCore.Authorization;
using System.Net.NetworkInformation;

namespace ImageGallery.Authorization
{
    public static class AuthorizationPolicies
    {
        public static AuthorizationPolicy CanAddImage()
        {
            return new AuthorizationPolicyBuilder()
                .RequireAuthenticatedUser()
                .RequireClaim("country", "be")
                .RequireRole("PayingUser")
                .Build();
        }
    }
}