using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using IdentityModel.Client;

namespace MyApp.Namespace
{
    public class SignoutModel : PageModel
    {
        public async Task<IActionResult> OnGet()
        {
            var accessToken = await HttpContext.GetTokenAsync("access_token");
            var refreshToken = await HttpContext.GetTokenAsync("refresh_token");
            var client = new HttpClient();
            var result = await client.RevokeTokenAsync(new TokenRevocationRequest
            {
                Address = "https://localhost:5001/connect/revocation",
                ClientId = "web",
                ClientSecret = "secret",

                Token = accessToken,
                TokenTypeHint = "access_token"
            });
            if (refreshToken != null)
            {
                var result1 = await client.RevokeTokenAsync(new TokenRevocationRequest
                {
                    Address = "https://localhost:5001/connect/revocation",
                    ClientId = "web",
                    ClientSecret = "secret",

                    Token = refreshToken,
                    TokenTypeHint = "refresh_token"
                });

            }

            return SignOut("Cookies", "oidc");
        }
    }
}
