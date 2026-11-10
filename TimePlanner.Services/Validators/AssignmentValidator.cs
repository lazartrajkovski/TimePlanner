using TimePlanner.Domain.Enums;
using TimePlanner.Services.DTOs;

namespace TimePlanner.Services.Validators;

public static class AssignmentValidator
{
    public static void ValidateAndNormalize(AssignmentRequest request)
    {
        ValidateCategoryId(request.CategoryId);
        request.AssignmentName = TextInput.Required(request.AssignmentName, "Assignment name", 50);
        request.Description = NormalizeDescription(request.Description);
    }

    public static void ValidateStatus(AssignmentStatus? status)
    {
        if (status.HasValue && !Enum.IsDefined(status.Value))
        {
            throw new ArgumentException("Invalid assignment status");
        }
    }

    private static void ValidateCategoryId(int categoryId)
    {
        if (categoryId <= 0)
        {
            throw new ArgumentException("Category id must be greater than zero");
        }
    }

    private static string? NormalizeDescription(string? assignmentDescription)
    {
        if (string.IsNullOrWhiteSpace(assignmentDescription))
        {
            return null;
        }

        return TextInput.Required(assignmentDescription, "Assignment description", 256);
    }
}