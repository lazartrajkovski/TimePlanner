using TimePlanner.Domain.Entities;
using TimePlanner.Domain.Enums;
using TimePlanner.Services.DTOs.Assignments;
using TimePlanner.Services.Validators;

namespace TimePlanner.Tests.Services.Validators;

public class AssignmentValidatorTests
{
    [Theory]
    [InlineData(0)]
    [InlineData(-1)]
    public void ValidateAndNormalize_InvalidCategoryId_ThrowsArgumentException(
        int categoryId)
    {
        var request = new AssignmentRequest
        {
            CategoryId = categoryId,
            AssignmentName = "Test assignment",
        };

        var exception = Assert.Throws<ArgumentException>(() => AssignmentValidator.ValidateAndNormalize(request));

        Assert.Equal("Category id must be greater than zero", exception.Message);
    }

    [Fact]
    public void ValidateAndNormalize_ValidRequest_NormalizesTextFields()
    {
        var request = new AssignmentRequest
        {
            CategoryId = 1,
            AssignmentName = "  Test assignment  ",
            Description = "  Test description  ",
        };

        AssignmentValidator.ValidateAndNormalize(request);

        Assert.Equal("Test assignment", request.AssignmentName);
        Assert.Equal("Test description", request.Description);
    }

    [Theory]
    [InlineData(null)]
    [InlineData("")]
    [InlineData("   ")]
    public void ValidateAndNormalize_MissingDescription_SetsDescriptionToNull(string? description)
    {
        var request = new AssignmentRequest
        {
            CategoryId = 1,
            AssignmentName = "Test assignment",
            Description = description,
        };

        AssignmentValidator.ValidateAndNormalize(request);

        Assert.Null(request.Description);
    }

    [Fact]
    public void ValidateAndNormalize_DescriptionLongerThanMaximum_ThrowsArgumentException()
    {
        var request = new AssignmentRequest
        {
            CategoryId = 1,
            AssignmentName = "Test assignment",
            Description = new string('a', 257),
        };

        var exception = Assert.Throws<ArgumentException>(() => AssignmentValidator.ValidateAndNormalize(request));

        Assert.Equal("Assignment description must be 256 characters or fewer", exception.Message);
    }

    [Fact]
    public void ValidateStatus_InvalidStatus_ThrowsArgumentException()
    {
        var status = (AssignmentStatus)999;

        var exception = Assert.Throws<ArgumentException>(() => AssignmentValidator.ValidateStatus(status));

        Assert.Equal("Invalid assignment status", exception.Message);
    }

    [Theory]
    [InlineData(null)]
    [InlineData(AssignmentStatus.ToDo)]
    [InlineData(AssignmentStatus.InProgress)]
    [InlineData(AssignmentStatus.Done)]
    public void ValidateStatus_ValidOrMissingStatus_DoesNotThrowException(AssignmentStatus? status)
    {
        var exception = Record.Exception(() => AssignmentValidator.ValidateStatus(status));

        Assert.Null(exception);
    }

    [Fact]
    public void EnsureNotArchived_ArchivedAssignment_ThrowsInvalidOperationException()
    {
        var assignment = new Assignment
        {
            ArchivedAt = new DateTime(2026, 11, 23, 10, 0, 0, DateTimeKind.Utc),
        };

        var exception = Assert.Throws<InvalidOperationException>(() => AssignmentValidator.EnsureNotArchived(assignment));

        Assert.Equal("Assignment is archived", exception.Message);
    }

    [Fact]
    public void EnsureNotArchived_ActiveAssignment_DoesNotThrowException()
    {
        var assignment = new Assignment
        {
            ArchivedAt = null,
        };

        var exception = Record.Exception(() => AssignmentValidator.EnsureNotArchived(assignment));

        Assert.Null(exception);
    }

    [Fact]
    public void EnsureArchived_ActiveAssignment_ThrowsInvalidOperationException()
    {
        var assignment = new Assignment
        {
            ArchivedAt = null,
        };

        var exception = Assert.Throws<InvalidOperationException>(() => AssignmentValidator.EnsureArchived(assignment));

        Assert.Equal("Assignment is not archived", exception.Message);
    }

    [Fact]
    public void EnsureArchived_ArchivedAssignment_DoesNotThrowException()
    {
        var assignment = new Assignment
        {
            ArchivedAt = new DateTime(2026, 11, 23, 10, 0, 0, DateTimeKind.Utc),
        };

        var exception = Record.Exception(() => AssignmentValidator.EnsureArchived(assignment));

        Assert.Null(exception);
    }
}