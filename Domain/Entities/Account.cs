namespace Domain.Entities;

public class Account
{
    public Guid Id { get; set; } = Guid.NewGuid();
    public decimal Balance { get; set; }

    public Account() { }

    public Account(decimal initialDeposit)
    {
        Balance = initialDeposit;
    }
}