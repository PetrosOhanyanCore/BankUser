using Application.Interfaces;
using Domain.Entities;

namespace Application.Services;

public class BankService
{
    private readonly IBankRepository _repo;
    public BankService(IBankRepository repo) => _repo = repo;

    public IEnumerable<User> GetAllUsers() => _repo.GetAllUsers();
    public User? GetUser(Guid id) => _repo.GetUser(id);
    public void AddUser(User user) => _repo.AddUser(user);

    public void Deposit(Guid userId, Guid accountId, decimal amount)
    {
        var user = _repo.GetUser(userId) ?? throw new InvalidOperationException("User not found.");
        var account = user.Accounts.FirstOrDefault(a => a.Id == accountId)
            ?? throw new InvalidOperationException("Account not found.");

        if (amount <= 0) throw new InvalidOperationException("Deposit amount must be positive.");
        if (amount > 10000) throw new InvalidOperationException("Cannot deposit more than $10,000.");

        account.Balance += amount;
        _repo.Save();
    }

    public void Withdraw(Guid userId, Guid accountId, decimal amount)
    {
        var user = _repo.GetUser(userId) ?? throw new InvalidOperationException("User not found.");
        var account = user.Accounts.FirstOrDefault(a => a.Id == accountId)
            ?? throw new InvalidOperationException("Account not found.");

        if (amount <= 0) throw new InvalidOperationException("Withdrawal must be positive.");
        if (amount > account.Balance * 0.9m) throw new InvalidOperationException("Cannot withdraw more than 90% of balance.");
        if (account.Balance - amount < 100) throw new InvalidOperationException("Balance cannot drop below $100.");

        account.Balance -= amount;
        _repo.Save();
    }
}