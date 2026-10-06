using BankProject.Core.Enums;
using BankProject.Core.Exceptions;
using BankProject.Core.Interfaces;
using BankProject.Core.Models;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging.Abstractions;
using Moq;
using Xunit;

namespace BankProject.Tests;

/// <summary>
/// בדיקות ברמת ה-Service (עם repositories מדומים ב-Moq, כנדרש) על לוגיקת המשאב המוגבל:
/// גם המקרה התקין (משיכה שמצליחה) וגם המקרה שנדחה (התנגשות concurrency -> 409).
/// </summary>
public class BankServiceConcurrencyTests
{
    private static Account CreateTestAccount(int id = 1, decimal balance = 1000m) => new()
    {
        Id = id,
        AccountNumber = "1000-0001",
        Type = AccountType.Checking,
        Balance = balance,
        CustomerId = 1
    };

    [Fact]
    public async Task WithdrawAsync_HappyPath_SavesSuccessfully()
    {
        // Arrange
        var account = CreateTestAccount(balance: 1000m);

        var accountsMock = new Mock<IAccountRepository>();
        accountsMock.Setup(r => r.GetByIdAsync(account.Id, It.IsAny<CancellationToken>()))
            .ReturnsAsync(account);

        var transactionsMock = new Mock<ITransactionRepository>();

        var unitOfWorkMock = new Mock<IUnitOfWork>();
        unitOfWorkMock.SetupGet(u => u.Accounts).Returns(accountsMock.Object);
        unitOfWorkMock.SetupGet(u => u.Transactions).Returns(transactionsMock.Object);
        unitOfWorkMock.Setup(u => u.SaveChangesAsync(It.IsAny<CancellationToken>())).ReturnsAsync(1);

        var service = new BankProject.Service.BankService(unitOfWorkMock.Object, NullLogger<BankProject.Service.BankService>.Instance);

        // Act
        await service.WithdrawAsync(account.Id, 300m);

        // Assert
        Assert.Equal(700m, account.Balance);
        transactionsMock.Verify(r => r.Add(It.Is<Transaction>(t => t.Type == TransactionType.Withdraw && t.Amount == 300m)), Times.Once);
        unitOfWorkMock.Verify(u => u.SaveChangesAsync(It.IsAny<CancellationToken>()), Times.Once);
    }

    [Fact]
    public async Task WithdrawAsync_ConcurrencyConflictOnSave_ThrowsConcurrencyConflictException()
    {
        // Arrange: מדמים בדיוק את המצב שקורה כשמישהו אחר "תפס" את החשבון בין הקריאה לשמירה -
        // ה-repository מדומה, וה-SaveChangesAsync זורק DbUpdateConcurrencyException כמו ש-EF Core
        // היה עושה במצב אמיתי.
        var account = CreateTestAccount(balance: 1000m);

        var accountsMock = new Mock<IAccountRepository>();
        accountsMock.Setup(r => r.GetByIdAsync(account.Id, It.IsAny<CancellationToken>()))
            .ReturnsAsync(account);

        var transactionsMock = new Mock<ITransactionRepository>();

        var unitOfWorkMock = new Mock<IUnitOfWork>();
        unitOfWorkMock.SetupGet(u => u.Accounts).Returns(accountsMock.Object);
        unitOfWorkMock.SetupGet(u => u.Transactions).Returns(transactionsMock.Object);
        unitOfWorkMock.Setup(u => u.SaveChangesAsync(It.IsAny<CancellationToken>()))
            .ThrowsAsync(new DbUpdateConcurrencyException("simulated conflict"));

        var service = new BankProject.Service.BankService(unitOfWorkMock.Object, NullLogger<BankProject.Service.BankService>.Instance);

        // Act + Assert: השירות חייב לתרגם את החריגה הטכנית לחריגה עסקית שה-API יודע למפות ל-409,
        // ולא להעביר הלאה DbUpdateConcurrencyException גולמי.
        await Assert.ThrowsAsync<ConcurrencyConflictException>(() => service.WithdrawAsync(account.Id, 300m));
    }

    [Fact]
    public async Task WithdrawAsync_InsufficientBalance_ThrowsInvalidOperationException_WithoutTouchingSave()
    {
        // מקרה שנדחה על בסיס לוגיקה עסקית (לא concurrency) - חייב להיכשל *לפני* SaveChangesAsync.
        var account = CreateTestAccount(balance: 50m);

        var accountsMock = new Mock<IAccountRepository>();
        accountsMock.Setup(r => r.GetByIdAsync(account.Id, It.IsAny<CancellationToken>()))
            .ReturnsAsync(account);

        var unitOfWorkMock = new Mock<IUnitOfWork>();
        unitOfWorkMock.SetupGet(u => u.Accounts).Returns(accountsMock.Object);
        unitOfWorkMock.SetupGet(u => u.Transactions).Returns(new Mock<ITransactionRepository>().Object);

        var service = new BankProject.Service.BankService(unitOfWorkMock.Object, NullLogger<BankProject.Service.BankService>.Instance);

        await Assert.ThrowsAsync<InvalidOperationException>(() => service.WithdrawAsync(account.Id, 1000m));

        unitOfWorkMock.Verify(u => u.SaveChangesAsync(It.IsAny<CancellationToken>()), Times.Never);
    }
}
