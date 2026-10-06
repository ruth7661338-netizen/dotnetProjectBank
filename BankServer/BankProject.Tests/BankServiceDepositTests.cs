using BankProject.Core.Enums;
using BankProject.Core.Interfaces;
using BankProject.Core.Models;
using Microsoft.Extensions.Logging.Abstractions;
using Moq;
using Xunit;

namespace BankProject.Tests;

public class BankServiceDepositTests
{
    private static Account CreateTestAccount(int id = 1, decimal balance = 1000m) => new()
    {
        Id = id,
        AccountNumber = "1000-0001",
        Type = AccountType.Checking,
        Balance = balance,
        CustomerId = 1
    };

    private static (BankProject.Service.BankService Service, Mock<IAccountRepository> Accounts, Mock<ITransactionRepository> Transactions, Mock<IUnitOfWork> UnitOfWork) CreateSut(Account account)
    {
        var accountsMock = new Mock<IAccountRepository>();
        accountsMock.Setup(r => r.GetByIdAsync(account.Id, It.IsAny<CancellationToken>())).ReturnsAsync(account);

        var transactionsMock = new Mock<ITransactionRepository>();

        var unitOfWorkMock = new Mock<IUnitOfWork>();
        unitOfWorkMock.SetupGet(u => u.Accounts).Returns(accountsMock.Object);
        unitOfWorkMock.SetupGet(u => u.Transactions).Returns(transactionsMock.Object);
        unitOfWorkMock.Setup(u => u.SaveChangesAsync(It.IsAny<CancellationToken>())).ReturnsAsync(1);

        var service = new BankProject.Service.BankService(unitOfWorkMock.Object, NullLogger<BankProject.Service.BankService>.Instance);
        return (service, accountsMock, transactionsMock, unitOfWorkMock);
    }

    [Fact]
    public async Task DepositAsync_HappyPath_IncreasesBalanceAndRecordsTransaction()
    {
        var account = CreateTestAccount(balance: 1000m);
        var (service, _, transactionsMock, unitOfWorkMock) = CreateSut(account);

        await service.DepositAsync(account.Id, 250m, "הפקדה לדוגמה");

        Assert.Equal(1250m, account.Balance);
        transactionsMock.Verify(r => r.Add(It.Is<Transaction>(t =>
            t.Type == TransactionType.Deposit && t.Amount == 250m && t.BalanceAfter == 1250m)), Times.Once);
        unitOfWorkMock.Verify(u => u.SaveChangesAsync(It.IsAny<CancellationToken>()), Times.Once);
    }

    [Theory]
    [InlineData(0)]
    [InlineData(-50)]
    public async Task DepositAsync_NonPositiveAmount_ThrowsWithoutSaving(decimal amount)
    {
        var account = CreateTestAccount(balance: 1000m);
        var (service, _, _, unitOfWorkMock) = CreateSut(account);

        await Assert.ThrowsAsync<InvalidOperationException>(() => service.DepositAsync(account.Id, amount));

        unitOfWorkMock.Verify(u => u.SaveChangesAsync(It.IsAny<CancellationToken>()), Times.Never);
    }

    [Fact]
    public async Task DepositAsync_AccountNotFound_ThrowsInvalidOperationException()
    {
        var accountsMock = new Mock<IAccountRepository>();
        accountsMock.Setup(r => r.GetByIdAsync(999, It.IsAny<CancellationToken>())).ReturnsAsync((Account?)null);

        var unitOfWorkMock = new Mock<IUnitOfWork>();
        unitOfWorkMock.SetupGet(u => u.Accounts).Returns(accountsMock.Object);

        var service = new BankProject.Service.BankService(unitOfWorkMock.Object, NullLogger<BankProject.Service.BankService>.Instance);

        await Assert.ThrowsAsync<InvalidOperationException>(() => service.DepositAsync(999, 100m));
    }
}
