using Library.Api.Models.Dto;
using Library.Api.Repositories;
using Microsoft.AspNetCore.Identity;

namespace Library.Api.Services
{
    public class AuthService : IAuthService
    {
        private readonly UserManager<IdentityUser> _userManager;
        private readonly ITokenRepository _tokenRepository;
        private readonly ILogger<AuthService> _logger;

        public AuthService(
            UserManager<IdentityUser> userManager,
            ITokenRepository tokenRepository,
            ILogger<AuthService> logger)
        {
            _userManager = userManager;
            _tokenRepository = tokenRepository;
            _logger = logger;
        }

        public async Task<ServiceResult<string>> RegisterAsync(
            RegisterRequestDto dto,
            CancellationToken cancellationToken = default)
        {
            _logger.LogInformation(
                "Registering user {Username}",
                dto.Username);

            var user = new IdentityUser
            {
                UserName = dto.Username,
                Email = dto.Username
            };

            var result = await _userManager.CreateAsync(
                user,
                dto.Password);

            if (!result.Succeeded)
            {
                var errors = string.Join(
                    "; ",
                    result.Errors.Select(error => error.Description));

                _logger.LogWarning(
                    "Registration failed for {Username}: {Errors}",
                    dto.Username,
                    errors);

                return ServiceResult<string>.BadRequest(errors);
            }

            _logger.LogInformation(
                "User {Username} registered successfully",
                dto.Username);

            return ServiceResult<string>.Ok(
                "User registered successfully.");
        }

        public async Task<ServiceResult<LoginResponseDto>> LoginAsync(
            LoginRequestDto dto,
            CancellationToken cancellationToken = default)
        {
            _logger.LogInformation(
                "Login attempt for {Username}",
                dto.Username);

            var user = await _userManager.FindByEmailAsync(
                dto.Username);

            if (user is null)
            {
                _logger.LogWarning(
                    "Login failed: user {Username} not found",
                    dto.Username);

                return ServiceResult<LoginResponseDto>.Unauthorized(
                    "Invalid login attempt.");
            }

            var passwordValid = await _userManager.CheckPasswordAsync(
                user,
                dto.Password);

            if (!passwordValid)
            {
                _logger.LogWarning(
                    "Login failed: invalid password for {Username}",
                    dto.Username);

                return ServiceResult<LoginResponseDto>.Unauthorized(
                    "Invalid login attempt.");
            }

            var roles = await _userManager.GetRolesAsync(user);

            if (roles.Count == 0)
            {
                _logger.LogWarning(
                    "Login failed: user {Username} has no roles",
                    dto.Username);

                return ServiceResult<LoginResponseDto>.Unauthorized(
                    "Invalid login attempt.");
            }

            try
            {
                var jwtToken = _tokenRepository.createJWTToken(
                    user,
                    roles.ToList());

                var response = new LoginResponseDto
                {
                    JwtToken = jwtToken
                };

                _logger.LogInformation(
                    "User {Username} logged in successfully",
                    dto.Username);

                return ServiceResult<LoginResponseDto>.Ok(response);
            }
            catch (InvalidOperationException ex)
            {
                _logger.LogWarning(
                    ex,
                    "Login failed because user {Username} has no valid membership",
                    dto.Username);

                return ServiceResult<LoginResponseDto>.Unauthorized(
                    "User does not have an active organization membership.");
            }
        }
    }
}
