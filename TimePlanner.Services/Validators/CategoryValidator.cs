using TimePlanner.Services.DTOs;

namespace TimePlanner.Services.Validators;

public static class CategoryValidator
{
    public static void ValidateAndNormalize(CategoryRequest request)
    {
        request.CategoryName = TextInput.Required(request.CategoryName, "Category name", 50);
    }
}