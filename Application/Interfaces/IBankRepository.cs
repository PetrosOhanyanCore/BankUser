using Domain.Entities;

namespace Application.Interfaces;

public interface IBankRepository
{
    IEnumerable<User> GetAllUsers();
    User? GetUser(Guid id);
    void AddUser(User user);
    void Save();
}