using Application.Services;
using Domain.Entities;
using Microsoft.AspNetCore.Mvc;

namespace API.Controllers;

[ApiController]
[Route("api/[controller]")]
public class UsersController : ControllerBase
{
    private readonly BankService _service;

    public UsersController(BankService service)
    {
        _service = service;
    }

    [HttpGet]
    public ActionResult<IEnumerable<User>> GetUsers() => Ok(_service.GetAllUsers());

    [HttpGet("{id}")]
    public ActionResult<User> GetUser(Guid id)
    {
        var user = _service.GetUser(id);
        return user is null ? NotFound() : Ok(user);
    }

    [HttpPost]
    public IActionResult CreateUser([FromBody] string name)
    {
        var user = new User { Name = name };
        _service.AddUser(user);
        return CreatedAtAction(nameof(GetUser), new { id = user.Id }, user);
    }
}
