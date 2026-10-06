using BankProject.Core.Dtos.Requests;
using BankProject.Core.Dtos.Responses;
using BankProject.Core.Enums;
using BankProject.Core.Exceptions;
using BankProject.Core.Interfaces;
using BankProject.Core.Models;

namespace BankProject.Service.Auth;

public class AuthService : IAuthService
{
    private readonly IUnitOfWork _unitOfWork;
    private readonly IPasswordHasher _passwordHasher;
    private readonly IJwtTokenGenerator _tokenGenerator;

    public AuthService(IUnitOfWork unitOfWork, IPasswordHasher passwordHasher, IJwtTokenGenerator tokenGenerator)
    {
        _unitOfWork = unitOfWork;
        _passwordHasher = passwordHasher;
        _tokenGenerator = tokenGenerator;
    }

    /// <summary>
    /// הרשמה יוצרת תמיד משתמש בתפקיד Customer, יחד עם רשומת Customer חדשה המקושרת אליו.
    /// משתמשי Clerk (פקידי בנק) לא נרשמים בעצמם - הם קיימים רק כ-seed data.
    /// </summary>
    public async Task<AuthResponseDto> RegisterAsync(RegisterRequestDto request, CancellationToken ct = default)
    {
        var existing = await _unitOfWork.Users.GetByEmailAsync(request.Email, ct);
        if (existing is not null)
            throw new InvalidOperationException("כתובת האימייל כבר רשומה במערכת");

        var customer = new Customer { FullName = request.FullName, Email = request.Email };
        _unitOfWork.Customers.Add(customer);
        await _unitOfWork.SaveChangesAsync(ct);

        var user = new User
        {
            FullName = request.FullName,
            Email = request.Email,
            PasswordHash = _passwordHasher.Hash(request.Password),
            Role = UserRole.Customer,
            CustomerId = customer.Id
        };
        _unitOfWork.Users.Add(user);
        await _unitOfWork.SaveChangesAsync(ct);

        return BuildAuthResponse(user);
    }

    public async Task<AuthResponseDto> LoginAsync(LoginRequestDto request, CancellationToken ct = default)
    {
        var user = await _unitOfWork.Users.GetByEmailAsync(request.Email, ct);
        if (user is null || !_passwordHasher.Verify(request.Password, user.PasswordHash))
            throw new AuthenticationFailedException("אימייל או סיסמה שגויים");

        return BuildAuthResponse(user);
    }

    private AuthResponseDto BuildAuthResponse(User user)
    {
        var (token, expiresAtUtc) = _tokenGenerator.GenerateToken(user);
        return new AuthResponseDto
        {
            Token = token,
            ExpiresAtUtc = expiresAtUtc,
            FullName = user.FullName,
            Role = user.Role,
            CustomerId = user.CustomerId
        };
    }
}
