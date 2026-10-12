using BankProject.Core.Validation;
using Xunit;

namespace BankProject.Tests;

/// <summary>
/// בדיקות לולידציית תעודת זהות: 9 ספרות + ספרת ביקורת תקינה.
/// </summary>
public class IsraeliIdAttributeTests
{
    [Theory]
    [InlineData("000000018")]
    [InlineData("123456782")]
    public void IsValidId_ValidIds_ReturnsTrue(string id) =>
        Assert.True(IsraeliIdAttribute.IsValidId(id));

    [Theory]
    [InlineData("123456789")]   // ספרת ביקורת שגויה
    [InlineData("12345678")]    // 8 ספרות
    [InlineData("1234567890")]  // 10 ספרות
    [InlineData("12345678a")]   // תו שאינו ספרה
    public void IsValidId_InvalidIds_ReturnsFalse(string id) =>
        Assert.False(IsraeliIdAttribute.IsValidId(id));
}
