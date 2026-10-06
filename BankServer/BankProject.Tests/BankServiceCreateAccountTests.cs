using BankProject.Core.Enums;
using BankProject.Core.Interfaces;
using BankProject.Core.Models;
using Microsoft.Extensions.Logging.Abstractions;
using Moq;
using Xunit;

namespace BankProject.Tests;

public class BankServiceCreateAccountTests
{
    private static Customer CreateTestCustomer(int id = 1) => new()
    {
        Id = id,
        FullName = "לקוחת בדיקה",
        Email = "test@example.com"
    };

    [Fact]
    public async Task CreateAccountAsync_HappyPath_CreatesAccountAndOpeningDeposit()
    {
        var customer = CreateTestCustomer();

        var customersMock = new Mock<ICustomerRepository>();
        customersMock.Setup(r => r.GetByIdAsync(customer.Id, It.IsAny<CancellationToken>())).ReturnsAsync(customer);

        var accountsMock = new Mock<IAccountRepository>();
        Account? addedAccount = null;
        accountsMock.Setup(r => r.Add(It.IsAny<Account>()))
            .Callback<Account>(a =>
            {
                a.Id = 42; // מדמה את מה ש-EF Core היה עושה אחרי SaveChanges על insert
                addedAccount = a;
            });

        var transactionsMock = new Mock<ITransactionRepository>();

        var unitOfWorkMock = new Mock<IUnitOfWork>();
        unitOfWorkMock.SetupGet(u => u.Customers).Returns(customersMock.Object);
        unitOfWorkMock.SetupGet(u => u.Accounts).Returns(accountsMock.Object);
        unitOfWorkMock.SetupGet(u => u.Transactions).Returns(transactionsMock.Object);
        unitOfWorkMock.Setup(u => u.SaveChangesAsync(It.IsAny<CancellationToken>())).ReturnsAsync(1);

        var service = new BankProject.Service.BankService(unitOfWorkMock.Object, NullLogger<BankProject.Service.BankService>.Instance);

        var result = await service.CreateAccountAsync(customer.Id, AccountType.Savings, 500m);

        Assert.Equal(500m, result.Balance);
        Assert.Equal(customer.Id, result.CustomerId);
        accountsMock.Verify(r => r.Add(It.IsAny<Account>()), Times.Once);
        transactionsMock.Verify(r => r.Add(It.Is<Transaction>(t => t.Type == TransactionType.Deposit && t.Amount == 500m)), Times.Once);
        unitOfWorkMock.Verify(u => u.SaveChangesAsync(It.IsAny<CancellationToken>()), Times.Exactly(2)); // פעם ליצירת החשבון, פעם לתנועת הפתיחה
    }

    [Fact]
    public async Task CreateAccountAsync_ZeroOpeningBalance_DoesNotRecordTransaction()
    {
        var customer = CreateTestCustomer();

        var customersMock = new Mock<ICustomerRepository>();
        customersMock.Setup(r => r.GetByIdAsync(customer.Id, It.IsAny<CancellationToken>())).ReturnsAsync(customer);

        var accountsMock = new Mock<IAccountRepository>();
        var transactionsMock = new Mock<ITransactionRepository>();

        var unitOfWorkMock = new Mock<IUnitOfWork>();
        unitOfWorkMock.SetupGet(u => u.Customers).Returns(customersMock.Object);
        unitOfWorkMock.SetupGet(u => u.Accounts).Returns(accountsMock.Object);
        unitOfWorkMock.SetupGet(u => u.Transactions).Returns(transactionsMock.Object);
        unitOfWorkMock.Setup(u => u.SaveChangesAsync(It.IsAny<CancellationToken>())).ReturnsAsync(1);

        var service = new BankProject.Service.BankService(unitOfWorkMock.Object, NullLogger<BankProject.Service.BankService>.Instance);

        await service.CreateAccountAsync(customer.Id, AccountType.Checking, 0m);

        transactionsMock.Verify(r => r.Add(It.IsAny<Transaction>()), Times.Never);
        unitOfWorkMock.Verify(u => u.SaveChangesAsync(It.IsAny<CancellationToken>()), Times.Once); // רק יצירת החשבון
    }

    [Fact]
    public async Task CreateAccountAsync_NegativeOpeningBalance_ThrowsWithoutTouchingRepositories()
    {
        var unitOfWorkMock = new Mock<IUnitOfWork>();
        var service = new BankProject.Service.BankService(unitOfWorkMock.Object, NullLogger<BankProject.Service.BankService>.Instance);

        await Assert.ThrowsAsync<InvalidOperationException>(() => service.CreateAccountAsync(1, AccountType.Checking, -100m));

        unitOfWorkMock.Verify(u => u.SaveChangesAsync(It.IsAny<CancellationToken>()), Times.Never);
    }

    [Fact]
    public async Task CreateAccountAsync_CustomerNotFound_ThrowsInvalidOperationException()
    {
        var customersMock = new Mock<ICustomerRepository>();
        customersMock.Setup(r => r.GetByIdAsync(999, It.IsAny<CancellationToken>())).ReturnsAsync((Customer?)null);

        var unitOfWorkMock = new Mock<IUnitOfWork>();
        unitOfWorkMock.SetupGet(u => u.Customers).Returns(customersMock.Object);

        var service = new BankProject.Service.BankService(unitOfWorkMock.Object, NullLogger<BankProject.Service.BankService>.Instance);

        await Assert.ThrowsAsync<InvalidOperationException>(() => service.CreateAccountAsync(999, AccountType.Checking, 100m));
    }
}
