using TimePlanner.Services.Validators;

namespace TimePlanner.Tests.Services.Validators;

public class TextInputTests
{
    [Theory]
    [InlineData(null)]
    [InlineData("")]
    [InlineData("   ")]
    public void Required_MissingValue_ThrowsArgumentException(string? value)
    {
        var exception = Assert.Throws<ArgumentException>(() => TextInput.Required(value!, "Field", 10));

        Assert.Equal("Field is required", exception.Message);
    }

    [Fact]
    public void Required_ValueAtMaximumLength_ReturnsValue()
    {
        var value = new string('a', 10);

        var result = TextInput.Required(value, "Field", 10);

        Assert.Equal(value, result);
    }

    [Fact]
    public void Required_ValueLongerThanMaximum_ThrowsArgumentException()
    {
        var value = new string('a', 11);

        var exception = Assert.Throws<ArgumentException>(() => TextInput.Required(value, "Field", 10));

        Assert.Equal("Field must be 10 characters or fewer", exception.Message);
    }

    [Fact]
    public void Required_TrimsWhitespaceBeforeCheckingLength()
    {
        var expected = new string('a', 10);
        var value = $"  {expected}  ";

        var result = TextInput.Required(value, "Field", 10);

        Assert.Equal(expected, result);
    }
}