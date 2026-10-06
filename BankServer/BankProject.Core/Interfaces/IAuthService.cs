using BankProject.Core.Dtos.Requests;
using BankProject.Core.Dtos.Responses;

namespace BankProject.Core.Interfaces;

public interface IAuthService
{
    Task<AuthResponseDto> RegisterAsync(RegisterRequestDto request, CancellationToken ct = default);
    Task<AuthResponseDto> LoginAsync(LoginRequestDto request, CancellationToken ct = default);
}
