using AutoMapper;
using Library.Api.CustomActionFilters;
using Library.Api.Models.Dto;
using Library.Api.Repositories;
using Library.Api.Services;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;

namespace Library.Api.Controllers
{
    [Route("api/[controller]")]
    [ApiController]

    public class AuthController : ControllerBase
    {
        private readonly IAuthService _authService;

        public AuthController(IAuthService authService)
        {
            _authService = authService;
        }

        [HttpPost("register")]
        public async Task<IActionResult> Register(
            [FromBody] RegisterRequestDto dto,
            CancellationToken cancellationToken)
        {
            var result = await _authService.RegisterAsync(
                dto, cancellationToken);

            if (!result.Success)
                return StatusCode(result.StatusCode, result.Error);

            return Ok(result.Data);
        }

        [HttpPost("login")]
        public async Task<IActionResult> Login(
            [FromBody] LoginRequestDto dto,
            CancellationToken cancellationToken)
        {
            var result = await _authService.LoginAsync(
                dto, cancellationToken);

            if (!result.Success)
                return StatusCode(result.StatusCode, result.Error);

            return Ok(result.Data);
        }
    }
}
