using AutoMapper;
using BankProject.API.Extensions;
using BankProject.Core.Dtos.Requests;
using BankProject.Core.Dtos.Responses;
using BankProject.Core.Enums;
using BankProject.Core.Exceptions;
using BankProject.Core.Interfaces;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace BankProject.API.Controllers;

[ApiController]
[Route("api/[controller]")]
[Authorize]
public class AccountsController : ControllerBase
{
    private readonly IBankService _bankService;
    private readonly IMapper _mapper;

    public AccountsController(IBankService bankService, IMapper mapper)
    {
        _bankService = bankService;
        _mapper = mapper;
    }

    /// <summary>מחזיר חשבונות בעימוד אמיתי (Skip/Take מול ה-DB), עם סינון אופציונלי לפי סוג. פקידות בנק בלבד.</summary>
    [HttpGet]
    [Authorize(Roles = "Clerk")]
    public async Task<IActionResult> GetAll(
        [FromQuery] int pageNumber = 1,
        [FromQuery] int pageSize = 20,
        [FromQuery] AccountType? type = null,
        CancellationToken ct = default)
    {
        if (pageNumber < 1) pageNumber = 1;
        if (pageSize is < 1 or > 100) pageSize = 20;

        var (items, totalCount) = await _bankService.GetAllAccountsAsync(pageNumber, pageSize, type, ct);

        return Ok(new PagedResultDto<AccountResponseDto>
        {
            Items = _mapper.Map<List<AccountResponseDto>>(items),
            PageNumber = pageNumber,
            PageSize = pageSize,
            TotalCount = totalCount
        });
    }

    /// <summary>מחזיר חשבון לפי מזהה. פקידות בנק רואות כל חשבון; לקוחה רואה רק את החשבון שלה.</summary>
    [HttpGet("{id:int}")]
    public async Task<IActionResult> GetById([FromRoute] int id, CancellationToken ct)
    {
        var account = await _bankService.GetAccountAsync(id, ct);
        if (account is null)
            return NotFound();

        if (User.IsForbiddenCustomerAccess(account.CustomerId))
            return Forbid();

        return Ok(_mapper.Map<AccountResponseDto>(account));
    }

    /// <summary>יוצר חשבון חדש ללקוח קיים. פקידות בנק בלבד.</summary>
    [HttpPost]
    [Authorize(Roles = "Clerk")]
    public async Task<IActionResult> Create([FromBody] CreateAccountRequestDto request, CancellationToken ct)
    {
        if (!ModelState.IsValid)
            return BadRequest(ModelState);

        try
        {
            var account = await _bankService.CreateAccountAsync(request.CustomerId, request.Type, request.OpeningBalance, ct);
            var dto = _mapper.Map<AccountResponseDto>(account);
            return CreatedAtAction(nameof(GetById), new { id = account.Id }, dto);
        }
        catch (InvalidOperationException ex)
        {
            return BadRequest(new { message = ex.Message });
        }
    }

    /// <summary>מחזיר את היסטוריית התנועות של חשבון.</summary>
    [HttpGet("{id:int}/transactions")]
    public async Task<IActionResult> GetTransactions([FromRoute] int id, CancellationToken ct)
    {
        var account = await _bankService.GetAccountAsync(id, ct);
        if (account is null)
            return NotFound();

        if (User.IsForbiddenCustomerAccess(account.CustomerId))
            return Forbid();

        var transactions = await _bankService.GetTransactionsAsync(id, ct);
        return Ok(_mapper.Map<List<TransactionResponseDto>>(transactions));
    }

    /// <summary>מבצע הפקדה או משיכה על חשבון (הפעולה שמתחרים עליה בו-זמנית).</summary>
    [HttpPost("{id:int}/transactions")]
    public async Task<IActionResult> PostTransaction([FromRoute] int id, [FromBody] AccountTransactionRequestDto request, CancellationToken ct)
    {
        if (!ModelState.IsValid)
            return BadRequest(ModelState);

        if (request.Type != TransactionType.Deposit && request.Type != TransactionType.Withdraw)
            return BadRequest(new { message = "סוג תנועה לא נתמך בנתיב זה. השתמשי ב-Deposit או Withdraw." });

        var existingAccount = await _bankService.GetAccountAsync(id, ct);
        if (existingAccount is null)
            return NotFound();

        if (User.IsForbiddenCustomerAccess(existingAccount.CustomerId))
            return Forbid();

        try
        {
            if (request.Type == TransactionType.Deposit)
                await _bankService.DepositAsync(id, request.Amount, request.Note, ct);
            else
                await _bankService.WithdrawAsync(id, request.Amount, request.Note, ct);

            var account = await _bankService.GetAccountAsync(id, ct);
            return Ok(_mapper.Map<AccountResponseDto>(account));
        }
        catch (ConcurrencyConflictException ex)
        {
            return Conflict(new { message = ex.Message });
        }
        catch (InvalidOperationException ex)
        {
            return BadRequest(new { message = ex.Message });
        }
    }

    /// <summary>מעביר כסף מהחשבון הזה לחשבון אחר. בעלות נבדקת רק על חשבון המקור.</summary>
    [HttpPost("{id:int}/transfers")]
    public async Task<IActionResult> Transfer([FromRoute] int id, [FromBody] TransferRequestDto request, CancellationToken ct)
    {
        if (!ModelState.IsValid)
            return BadRequest(ModelState);

        var sourceAccount = await _bankService.GetAccountAsync(id, ct);
        if (sourceAccount is null)
            return NotFound();

        if (User.IsForbiddenCustomerAccess(sourceAccount.CustomerId))
            return Forbid();

        try
        {
            await _bankService.TransferAsync(id, request.ToAccountId, request.Amount, request.Note, ct);

            var account = await _bankService.GetAccountAsync(id, ct);
            return Ok(_mapper.Map<AccountResponseDto>(account));
        }
        catch (ConcurrencyConflictException ex)
        {
            return Conflict(new { message = ex.Message });
        }
        catch (InvalidOperationException ex)
        {
            return BadRequest(new { message = ex.Message });
        }
    }
}
