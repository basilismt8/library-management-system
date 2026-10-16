using Library.Api.Models.Dto;

namespace Library.Api.Services
{
    public interface IAuthService
    {
        Task<ServiceResult<string>> RegisterAsync(
            RegisterRequestDto dto,
            CancellationToken cancellationToken = default);

        Task<ServiceResult<LoginResponseDto>> LoginAsync(
            LoginRequestDto dto,
            CancellationToken cancellationToken = default);
    }
}
