using Application.Interfaces;
using Domain.Entities;
using Infrastructure.Data;
using Microsoft.EntityFrameworkCore;

namespace Infrastructure.Repositories;

public class EfBankRepository : IBankRepository
{
    private readonly BankDbContext _context;
    public EfBankRepository(BankDbContext context) => _context = context;

    public IEnumerable<User> GetAllUsers() => _context.Users.Include(u => u.Accounts).ToList();

    public User? GetUser(Guid id) =>
        _context.Users.Include(u => u.Accounts).FirstOrDefault(u => u.Id == id);

    public void AddUser(User user)
    {
        _context.Users.Add(user);
        Save();
    }

    public void Save() => _context.SaveChanges();
}