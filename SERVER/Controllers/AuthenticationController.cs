using System;
using System.Data;
using BaseLibrary.DTOs;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using ServerLibrary.Repositories.Contracts;

namespace SERVER.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
   
    public class AuthenticationController:ControllerBase
	{
		private IUserAccount _userAccount;
		public AuthenticationController(IUserAccount userAccount)
		{
			_userAccount = userAccount;
		}
		[HttpPost("register")]
		public async Task<IActionResult> CreateAsync(Register user)
		{
			if (user == null) return BadRequest("Model is empty");
			var result = await _userAccount.CreateAsync(user);
			return Ok(result);
		}


        [HttpPost("login")]
        public async Task<IActionResult> SignInAsync(Login user)
        {
            if (user == null) return BadRequest("Model is empty");
            var result = await _userAccount.SignInAsync(user);
            return Ok(result);
        }

		[HttpPost("refresh-token")]
		public async Task<IActionResult> RefreshTokenAsync(RefreshToken token)
		{
			if (token == null) return BadRequest("Model is empty");
			var result = await _userAccount.RefreshTokenAsync(token);
			return Ok(result);
		}

    }
}

