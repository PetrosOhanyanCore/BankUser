using Application.Interfaces;
using Application.Services;
using Domain.Entities;
using Moq;

namespace Tests;

public class ServiceTests
{

    private readonly Mock<IBankRepository> _mockRepo;
    private readonly BankService _service;
    private readonly User _user;
    private readonly Account _account;

    public ServiceTests()
    {
        _mockRepo = new Mock<IBankRepository>();
        _service = new BankService(_mockRepo.Object);

        _account = new Account(500);
        _user = new User { Name = "Some name", Accounts = new List<Account> { _account } };

        _mockRepo.Setup(r => r.GetUser(It.IsAny<Guid>())).Returns(_user);
    }

    [Fact]
    public void Deposit_ShouldIncreaseBalance_WhenValid()
    {
        // Arrange
        var initial = _account.Balance;
        var amount = 1000;

        // Act
        _service.Deposit(_user.Id, _account.Id, amount);

        // Assert
        Assert.Equal(initial + amount, _account.Balance);
        _mockRepo.Verify(r => r.Save(), Times.Once);
    }

    [Fact]
    public void Deposit_ShouldThrow_WhenAmountTooHigh()
    {
        Assert.Throws<InvalidOperationException>(() =>
            _service.Deposit(_user.Id, _account.Id, 20000));
    }

    [Fact]
    public void Withdraw_ShouldDecreaseBalance_WhenValid()
    {
        var initial = _account.Balance;
        var amount = 200;

        _service.Withdraw(_user.Id, _account.Id, amount);

        Assert.Equal(initial - amount, _account.Balance);
        _mockRepo.Verify(r => r.Save(), Times.Once);
    }

    [Fact]
    public void Withdraw_ShouldThrow_WhenExceeds90Percent()
    {
        var amount = _account.Balance * 0.95m;
        Assert.Throws<InvalidOperationException>(() =>
            _service.Withdraw(_user.Id, _account.Id, amount));
    }

    [Fact]
    public void Withdraw_ShouldThrow_WhenBelow100Min()
    {
        // Arrange
        var amount = _account.Balance - 80; // leaves only $80 (below 100)

        // Act + Assert
        Assert.Throws<InvalidOperationException>(() =>
            _service.Withdraw(_user.Id, _account.Id, amount));
    }
}