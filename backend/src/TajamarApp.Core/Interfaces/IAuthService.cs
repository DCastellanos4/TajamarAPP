using TajamarApp.Core.DTOs.Auth;

namespace TajamarApp.Core.Interfaces;

public interface IAuthService
{
    Task<LoginResponseDto?> LoginAsync(LoginRequestDto request);
}
