using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using PaymentsService.Application.Dtos;
using PaymentsService.Domain;
using PaymentsService.Infrastructure;

namespace PaymentsService.Controllers;

[ApiController]
[Route("api/payments")]
public class PaymentsController : ControllerBase
{
    private readonly PaymentsDbContext _dbContext;

    public PaymentsController(PaymentsDbContext dbContext)
    {
        _dbContext = dbContext;
    }
    [HttpPost("accounts")]
    public async Task<IActionResult> CreateAccount([FromBody] CreateAccountRequest request)
    {
        var exists = await _dbContext.Accounts
            .AnyAsync(a => a.UserId == request.UserId);

        if (exists)
        {
            return Conflict("Account already exists for this user");
        }

        var account = new Account
        {
            Id = Guid.NewGuid(),
            UserId = request.UserId,
            Balance = 0m,
            CreatedAt = DateTime.UtcNow
        };

        _dbContext.Accounts.Add(account);
        await _dbContext.SaveChangesAsync();

        return Ok();
    }
    [HttpPost("accounts/{userId:guid}/top-up")]
    public async Task<IActionResult> TopUpAccount(Guid userId, [FromBody] TopUpAccountRequest request)
    {
        if (request.Amount <= 0)
        {
            return BadRequest("Amount must be positive");
        }

        var account = await _dbContext.Accounts
            .SingleOrDefaultAsync(a => a.UserId == userId);

        if (account == null)
        {
            return NotFound("Account not found");
        }

        account.Balance += request.Amount;
        await _dbContext.SaveChangesAsync();

        return Ok();
    }
    [HttpGet("accounts/{userId:guid}")]
    public async Task<ActionResult<AccountResponse>> GetBalance(Guid userId)
    {
        var account = await _dbContext.Accounts
            .AsNoTracking()
            .SingleOrDefaultAsync(a => a.UserId == userId);

        if (account == null)
        {
            return NotFound();
        }

        return new AccountResponse
        {
            UserId = account.UserId,
            Balance = account.Balance
        };
    }
}
