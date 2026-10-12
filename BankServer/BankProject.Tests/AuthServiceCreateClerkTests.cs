using BankProject.Core.Dtos.Requests;
using BankProject.Core.Enums;
using BankProject.Core.Interfaces;
using BankProject.Core.Models;
using BankProject.Service.Auth;
using Moq;
using Xunit;

namespace BankProject.Tests;

/// <summary>
/// בדיקות ל-CreateClerkAsync: פקידה מוסיפה פקיד/ה נוספ/ת - מקרה תקין ומקרה של אימייל תפוס.
/// </summary>
public class AuthServiceCreateClerkTests
{
    private static RegisterRequestDto CreateRequest() => new()
    {
        FullName = "פקיד חדש",
        IdNumber = "000000042",
        Email = "new.clerk@bank.local",
        Password = "Secret123!"
    };

    [Fact]
    public async Task CreateClerkAsync_NewEmail_CreatesClerkWithoutCustomer()
    {
        // Arrange
        var usersMock = new Mock<IUserRepository>();
        usersMock.Setup(r => r.GetByEmailAsync(It.IsAny<string>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync((User?)null);

        var customersMock = new Mock<ICustomerRepository>();

        var unitOfWorkMock = new Mock<IUnitOfWork>();
        unitOfWorkMock.SetupGet(u => u.Users).Returns(usersMock.Object);
        unitOfWorkMock.SetupGet(u => u.Customers).Returns(customersMock.Object);
        unitOfWorkMock.Setup(u => u.SaveChangesAsync(It.IsAny<CancellationToken>())).ReturnsAsync(1);

        var hasherMock = new Mock<IPasswordHasher>();
        hasherMock.Setup(h => h.Hash("Secret123!")).Returns("hashed");

        var service = new AuthService(unitOfWorkMock.Object, hasherMock.Object, Mock.Of<IJwtTokenGenerator>());

        // Act
        var result = await service.CreateClerkAsync(CreateRequest());

        // Assert
        Assert.Equal(UserRole.Clerk, result.Role);
        Assert.Equal("new.clerk@bank.local", result.Email);
        usersMock.Verify(r => r.Add(It.Is<User>(u =>
            u.Role == UserRole.Clerk && u.CustomerId == null && u.PasswordHash == "hashed" && u.IdNumber == "000000042")), Times.Once);
        customersMock.Verify(r => r.Add(It.IsAny<Customer>()), Times.Never);
        unitOfWorkMock.Verify(u => u.SaveChangesAsync(It.IsAny<CancellationToken>()), Times.Once);
    }

    [Fact]
    public async Task CreateClerkAsync_EmailAlreadyExists_ThrowsAndSavesNothing()
    {
        // Arrange
        var usersMock = new Mock<IUserRepository>();
        usersMock.Setup(r => r.GetByEmailAsync("new.clerk@bank.local", It.IsAny<CancellationToken>()))
            .ReturnsAsync(new User { Id = 1, Email = "new.clerk@bank.local", Role = UserRole.Customer });

        var unitOfWorkMock = new Mock<IUnitOfWork>();
        unitOfWorkMock.SetupGet(u => u.Users).Returns(usersMock.Object);

        var service = new AuthService(unitOfWorkMock.Object, Mock.Of<IPasswordHasher>(), Mock.Of<IJwtTokenGenerator>());

        // Act + Assert
        await Assert.ThrowsAsync<InvalidOperationException>(() => service.CreateClerkAsync(CreateRequest()));
        usersMock.Verify(r => r.Add(It.IsAny<User>()), Times.Never);
        unitOfWorkMock.Verify(u => u.SaveChangesAsync(It.IsAny<CancellationToken>()), Times.Never);
    }

    [Fact]
    public async Task CreateClerkAsync_IdNumberAlreadyExists_ThrowsAndSavesNothing()
    {
        // Arrange
        var usersMock = new Mock<IUserRepository>();
        usersMock.Setup(r => r.GetByIdNumberAsync("000000042", It.IsAny<CancellationToken>()))
            .ReturnsAsync(new User { Id = 1, IdNumber = "000000042", Role = UserRole.Customer });

        var unitOfWorkMock = new Mock<IUnitOfWork>();
        unitOfWorkMock.SetupGet(u => u.Users).Returns(usersMock.Object);

        var service = new AuthService(unitOfWorkMock.Object, Mock.Of<IPasswordHasher>(), Mock.Of<IJwtTokenGenerator>());

        // Act + Assert
        var ex = await Assert.ThrowsAsync<InvalidOperationException>(() => service.CreateClerkAsync(CreateRequest()));
        Assert.Contains("תעודת הזהות", ex.Message);
        usersMock.Verify(r => r.Add(It.IsAny<User>()), Times.Never);
    }
}
