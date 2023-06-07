using Diplom.Entities;
using Diplom.Models;
using Diplom.Services;
using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Authentication.Cookies;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Microsoft.IdentityModel.Tokens;
using System;
using System.Collections.Generic;
using System.IdentityModel.Tokens.Jwt;
using System.Linq;
using System.Security.Claims;
using System.Text;
using System.Threading.Tasks;

namespace Diplom.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class UserController : ControllerBase
    {
        private readonly UserService _userService;
        private readonly ApplicationContext _dbContext;
        private readonly UserManager<User> _userManager;
        private readonly ITokenService _tokenService;

        public UserController(UserService userService, ApplicationContext dbContext, UserManager<User> userManager, ITokenService tokenService)
        {
            _userService = userService;
            _dbContext = dbContext;
            _tokenService = tokenService;
            _userManager = userManager;
        }

        [HttpPost("Register")]
        public async Task<IActionResult> RegisterUser(RegisterUserModel model)
        {
            User user = new User
            {
                Email = model.Email,
                UserName = model.Email,
                FirstName = model.FirstName,
                SecondName = model.SecondName
            };

            if (model.Password != model.RepeatPassword)
                return BadRequest("Паролі не співпадають");

            var result = await _userManager.CreateAsync(user, model.Password);
            if (!result.Succeeded)
            {
                return BadRequest(result.Errors);
            }

            var validUser = _userService.GetUserByEmail(model.Email);
            string generatedToken = _tokenService.BuildToken(validUser);
            if (generatedToken != null)
            {
                return Ok(generatedToken);
            }

            return BadRequest("Виникла помилка реєстрації");
        }


        [HttpGet("GetAllUsers")]
        public IActionResult GetAllUsers()
        {
            var result = _userService.GetAllUsers();
            return Ok(result);
        }

        [HttpPost("Login")]
        public async Task<IActionResult> Login([FromBody] LoginViewModel model)
        {
            if (string.IsNullOrEmpty(model.Email) || string.IsNullOrEmpty(model.Password))
            {
                return BadRequest("Всі поля мають бути заповнені");
            }

            var validUser = _userService.GetUserByEmail(model.Email);
            if (validUser == null)
                return BadRequest("Користувача з таким email не існує");

            string generatedToken = _tokenService.BuildToken(validUser);
            if (generatedToken != null)
            {
                return Ok(generatedToken);
            }

            return BadRequest("Виникла помилка");
        }
    }
}
