using TimePlanner.Services.DTOs.Categories;
using TimePlanner.Services.Validators;

namespace TimePlanner.Tests.Services.Validators;

public class CategoryValidatorTests
{
    [Fact]
    public void ValidateAndNormalize_NameWithWhitespace_TrimsName()
    {
        var request = new CategoryRequest
        {
            CategoryName = "  Work  ",
        };

        CategoryValidator.ValidateAndNormalize(request);

        Assert.Equal("Work", request.CategoryName);
    }
}