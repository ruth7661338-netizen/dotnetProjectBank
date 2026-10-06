namespace BankProject.Core.Dtos.Responses;

public class CustomerResponseDto
{
    public int Id { get; set; }
    public string FullName { get; set; } = string.Empty;
    public string Email { get; set; } = string.Empty;
    public List<AccountSummaryDto> Accounts { get; set; } = new();
}
