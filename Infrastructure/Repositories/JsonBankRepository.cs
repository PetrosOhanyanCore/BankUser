using Application.Interfaces;
using Domain.Entities;
using System.Text.Json;

namespace Infrastructure.Repositories;

public class JsonBankRepository : IBankRepository
{
    private const string FilePath = "users.json";
    private List<User> _users = new();

    public JsonBankRepository() => Load();

    public IEnumerable<User> GetAllUsers() => _users;
    public User? GetUser(Guid id) => _users.FirstOrDefault(x => x.Id == id);

    public void AddUser(User user)
    {
        _users.Add(user);
        Save();
    }

    public void Save() => File.WriteAllText(FilePath,
        JsonSerializer.Serialize(_users, new JsonSerializerOptions { WriteIndented = true }));

    private void Load()
    {
        if (!File.Exists(FilePath)) return;
        var json = File.ReadAllText(FilePath);
        var loaded = JsonSerializer.Deserialize<List<User>>(json);
        if (loaded != null) _users = loaded;
    }
}