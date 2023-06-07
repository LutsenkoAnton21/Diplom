using Diplom.Services;
using Microsoft.AspNetCore.Http;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Diplom.Auth
{
    public class JwtMiddleware
    {
        private readonly RequestDelegate _next;

        public JwtMiddleware(RequestDelegate next)
        {
            _next = next;
        }

        public async Task Invoke(HttpContext context, UserService userService, ITokenService tokenService)
        {
            string token = context.Request.Headers["Authorization"].FirstOrDefault()?.Split(" ")?.Last();

            if (token != null)
                tokenService.AttachUserToContext(context, token);

            await _next(context);
        }
    }
}
