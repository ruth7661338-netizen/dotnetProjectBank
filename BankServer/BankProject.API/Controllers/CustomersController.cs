using AutoMapper;
using BankProject.API.Extensions;
using BankProject.Core.Dtos.Responses;
using BankProject.Core.Interfaces;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace BankProject.API.Controllers;

[ApiController]
[Route("api/[controller]")]
[Authorize]
public class CustomersController : ControllerBase
{
    private readonly IBankService _bankService;
    private readonly IMapper _mapper;

    public CustomersController(IBankService bankService, IMapper mapper)
    {
        _bankService = bankService;
        _mapper = mapper;
    }

    /// <summary>מחזיר את כל הלקוחות. פקידות בנק בלבד.</summary>
    [HttpGet]
    [Authorize(Roles = "Clerk")]
    public async Task<IActionResult> GetAll(CancellationToken ct)
    {
        var customers = await _bankService.GetAllCustomersAsync(ct);
        return Ok(_mapper.Map<List<CustomerResponseDto>>(customers));
    }

    /// <summary>מחזיר לקוח לפי מזהה, כולל החשבונות שלו. פקידות בנק רואות כל לקוח; לקוחה רואה רק את עצמה.</summary>
    [HttpGet("{id:int}")]
    public async Task<IActionResult> GetById([FromRoute] int id, CancellationToken ct)
    {
        if (User.IsForbiddenCustomerAccess(id))
            return Forbid();

        var customer = await _bankService.GetCustomerAsync(id, ct);
        return customer is null ? NotFound() : Ok(_mapper.Map<CustomerResponseDto>(customer));
    }
}
