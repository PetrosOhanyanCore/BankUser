namespace Domain.Entities;

public class User
{
    public Guid Id { get; set; } = Guid.NewGuid();
    public string Name { get; set; } = default!;
    public List<Account> Accounts { get; set; } = new();

    public Account CreateAccount(decimal initialDeposit)
    {
        var acc = new Account(initialDeposit);
        Accounts.Add(acc);
        return acc;
    }
}