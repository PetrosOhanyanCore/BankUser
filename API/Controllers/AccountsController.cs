using Application.Services;
using Microsoft.AspNetCore.Mvc;

namespace API.Controllers;

[ApiController]
[Route("api/[controller]")]
public class AccountsController : ControllerBase
{
    private readonly BankService _service;
    public AccountsController(BankService service) => _service = service;

    [HttpPost("{userId}/deposit/{accountId}")]
    public IActionResult Deposit(Guid userId, Guid accountId, [FromQuery] decimal amount)
    {
        try
        {
            _service.Deposit(userId, accountId, amount);
            return Ok(new { Message = $"Deposited {amount:C} successfully." });
        }
        catch (Exception ex)
        {
            return BadRequest(ex.Message);
        }
    }

    [HttpPost("{userId}/withdraw/{accountId}")]
    public IActionResult Withdraw(Guid userId, Guid accountId, [FromQuery] decimal amount)
    {
        try
        {
            _service.Withdraw(userId, accountId, amount);
            return Ok(new { Message = $"Withdrew {amount:C} successfully." });
        }
        catch (Exception ex)
        {
            return BadRequest(ex.Message);
        }
    }
}

