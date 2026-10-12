using BankProject.Core.Enums;

namespace BankProject.Core.Dtos.Responses;

// פרטי משתמש שנוצר ע"י פקידה - בלי token, כדי שהיוצרת לא תתחבר בטעות כמשתמש החדש.
public class UserResponseDto
{
    public int Id { get; set; }
    public string FullName { get; set; } = string.Empty;
    public string Email { get; set; } = string.Empty;
    public UserRole Role { get; set; }
}
