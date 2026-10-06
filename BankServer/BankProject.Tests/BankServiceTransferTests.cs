using BankProject.Core.Enums;
using BankProject.Core.Interfaces;
using BankProject.Core.Models;
using Microsoft.Extensions.Logging.Abstractions;
using Moq;
using Xunit;

namespace BankProject.Tests;

public class BankServiceTransferTests
{
    private static Account CreateTestAccount(int id, decimal balance) => new()
    {
        Id = id,
        AccountNumber = $"1000-{id:D4}",
        Type = AccountType.Checking,
        Balance = balance,
        CustomerId = 1
    };

    private static (BankProject.Service.BankService Service, Mock<ITransactionRepository> Transactions, Mock<IUnitOfWork> UnitOfWork) CreateSut(params Account[] accounts)
    {
        var accountsMock = new Mock<IAccountRepository>();
        foreach (var account in accounts)
        {
            accountsMock.Setup(r => r.GetByIdAsync(account.Id, It.IsAny<CancellationToken>())).ReturnsAsync(account);
        }

        var transactionsMock = new Mock<ITransactionRepository>();

        var unitOfWorkMock = new Mock<IUnitOfWork>();
        unitOfWorkMock.SetupGet(u => u.Accounts).Returns(accountsMock.Object);
        unitOfWorkMock.SetupGet(u => u.Transactions).Returns(transactionsMock.Object);
        unitOfWorkMock.Setup(u => u.SaveChangesAsync(It.IsAny<CancellationToken>())).ReturnsAsync(1);

        var service = new BankProject.Service.BankService(unitOfWorkMock.Object, NullLogger<BankProject.Service.BankService>.Instance);
        return (service, transactionsMock, unitOfWorkMock);
    }

    [Fact]
    public async Task TransferAsync_HappyPath_MovesMoneyAndRecordsTwoTransactions()
    {
        var from = CreateTestAccount(1, 1000m);
        var to = CreateTestAccount(2, 200m);
        var (service, transactionsMock, unitOfWorkMock) = CreateSut(from, to);

        await service.TransferAsync(from.Id, to.Id, 300m);

        Assert.Equal(700m, from.Balance);
        Assert.Equal(500m, to.Balance);
        transactionsMock.Verify(r => r.Add(It.Is<Transaction>(t => t.AccountId == from.Id && t.Type == TransactionType.TransferOut && t.Amount == 300m)), Times.Once);
        transactionsMock.Verify(r => r.Add(It.Is<Transaction>(t => t.AccountId == to.Id && t.Type == TransactionType.TransferIn && t.Amount == 300m)), Times.Once);
        unitOfWorkMock.Verify(u => u.SaveChangesAsync(It.IsAny<CancellationToken>()), Times.Once);
    }

    [Fact]
    public async Task TransferAsync_InsufficientSourceBalance_ThrowsWithoutSaving()
    {
        var from = CreateTestAccount(1, 100m);
        var to = CreateTestAccount(2, 200m);
        var (service, _, unitOfWorkMock) = CreateSut(from, to);

        await Assert.ThrowsAsync<InvalidOperationException>(() => service.TransferAsync(from.Id, to.Id, 500m));

        unitOfWorkMock.Verify(u => u.SaveChangesAsync(It.IsAny<CancellationToken>()), Times.Never);
    }

    [Fact]
    public async Task TransferAsync_SameSourceAndTarget_ThrowsInvalidOperationException()
    {
        var account = CreateTestAccount(1, 1000m);
        var (service, _, unitOfWorkMock) = CreateSut(account);

        await Assert.ThrowsAsync<InvalidOperationException>(() => service.TransferAsync(account.Id, account.Id, 100m));

        unitOfWorkMock.Verify(u => u.SaveChangesAsync(It.IsAny<CancellationToken>()), Times.Never);
    }

    [Fact]
    public async Task TransferAsync_TargetAccountNotFound_ThrowsInvalidOperationException()
    {
        var from = CreateTestAccount(1, 1000m);
        var (service, _, unitOfWorkMock) = CreateSut(from); // רק חשבון המקור מוגדר - היעד "לא קיים"

        await Assert.ThrowsAsync<InvalidOperationException>(() => service.TransferAsync(from.Id, 999, 100m));

        unitOfWorkMock.Verify(u => u.SaveChangesAsync(It.IsAny<CancellationToken>()), Times.Never);
    }
}
