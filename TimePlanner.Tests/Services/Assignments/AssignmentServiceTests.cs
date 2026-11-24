using MapsterMapper;
using Moq;
using TimePlanner.Domain.Entities;
using TimePlanner.Domain.Enums;
using TimePlanner.Repositories.Interfaces;
using TimePlanner.Services.DTOs.Assignments;
using TimePlanner.Services.Interfaces;
using TimePlanner.Services.Services;

namespace TimePlanner.Tests.Services.Assignments;

public class AssignmentServiceTests
{
    private readonly Mock<IAssignmentRepository> _assignmentRepositoryMock;
    private readonly Mock<ICategoryRepository> _categoryRepositoryMock;
    private readonly Mock<ICurrentUser> _currentUserMock;
    private readonly Mock<ITimeEntryRepository> _timeEntryRepositoryMock;
    private readonly Mock<IMapper> _mapperMock;
    private readonly AssignmentService _assignmentService;

    public AssignmentServiceTests()
    {
        _assignmentRepositoryMock = new Mock<IAssignmentRepository>();
        _categoryRepositoryMock = new Mock<ICategoryRepository>();
        _currentUserMock = new Mock<ICurrentUser>();
        _timeEntryRepositoryMock = new Mock<ITimeEntryRepository>();
        _mapperMock = new Mock<IMapper>();

        _assignmentService = new AssignmentService(
            _assignmentRepositoryMock.Object,
            _categoryRepositoryMock.Object,
            _currentUserMock.Object,
            _timeEntryRepositoryMock.Object,
            _mapperMock.Object);
    }

    [Fact]
    public async Task Add_ValidAssignmentRequest_CreatesAssignment()
    {
        var request = new AssignmentRequest
        {
            CategoryId = 1,
            AssignmentName = "  Study Mathematics  ",
            Description = "  Complete calculus exercises  ",
        };

        var category = new Category
        {
            Id = 1,
            UserId = 42,
            CategoryName = "Personal",
        };

        var newAssignment = new Assignment
        {
            CategoryId = 1,
            AssignmentName = "Study Mathematics",
            Description = "Complete calculus exercises"
        };

        var savedAssignment = new Assignment
        {
            Id = 5,
            CategoryId = 1,
            AssignmentName = "Study Mathematics",
            Description = "Complete calculus exercises"
        };

        var assignmentResponse = new AssignmentResponse
        {
            Id = 5,
            CategoryId = 1,
            AssignmentName = "Study Mathematics",
            Description = "Complete calculus exercises"
        };

        _currentUserMock
            .Setup(currentUser => currentUser.UserId)
            .Returns(42);

        _categoryRepositoryMock
            .Setup(repository => repository.GetById(1))
            .ReturnsAsync(category);

        _assignmentRepositoryMock
            .Setup(repository => repository.GetByNameAndCategoryId(
                "Study Mathematics",
                1))
            .ReturnsAsync((Assignment?)null);

        _mapperMock
            .Setup(mapper => mapper.Map<Assignment>(request))
            .Returns(newAssignment);

        _assignmentRepositoryMock
            .Setup(repository => repository.Add(newAssignment))
            .ReturnsAsync(savedAssignment);

        _mapperMock
            .Setup(mapper => mapper.Map<AssignmentResponse>(savedAssignment))
            .Returns(assignmentResponse);

        var result = await _assignmentService.Add(request);

        Assert.Same(assignmentResponse, result);
        Assert.Equal("Study Mathematics", request.AssignmentName);
        Assert.Equal("Complete calculus exercises", request.Description);

        _assignmentRepositoryMock.Verify(
            repository => repository.Add(newAssignment),
            Times.Once);
    }

    [Fact]
    public async Task Add_InvalidAssignmentRequest_ThrowsArgumentException()
    {
        var request = new AssignmentRequest
        {
            CategoryId = 1,
            AssignmentName = "   ",
        };

        var exception = await Assert.ThrowsAsync<ArgumentException>(
            () => _assignmentService.Add(request));

        Assert.Equal("Assignment name is required", exception.Message);

        _assignmentRepositoryMock.Verify(
            repository => repository.Add(It.IsAny<Assignment>()),
            Times.Never);
    }

    [Fact]
    public async Task Add_CategoryDoesNotExist_ThrowsKeyNotFoundException()
    {
        var request = new AssignmentRequest
        {
            CategoryId = 1,
            AssignmentName = "Study Mathematics",
        };

        _categoryRepositoryMock
            .Setup(repository => repository.GetById(1))
            .ReturnsAsync((Category?)null);

        var exception = await Assert.ThrowsAsync<KeyNotFoundException>(
            () => _assignmentService.Add(request));

        Assert.Equal("Category not found", exception.Message);

        _assignmentRepositoryMock.Verify(
            repository => repository.Add(It.IsAny<Assignment>()),
            Times.Never);
    }

    [Fact]
    public async Task Add_CategoryBelongsToAnotherUser_ThrowsKeyNotFoundException()
    {
        var request = new AssignmentRequest
        {
            CategoryId = 1,
            AssignmentName = "Study Mathematics",
        };

        var category = new Category
        {
            Id = 1,
            UserId = 99,
            CategoryName = "Personal",
        };

        _currentUserMock
            .Setup(currentUser => currentUser.UserId)
            .Returns(42);

        _categoryRepositoryMock
            .Setup(repository => repository.GetById(1))
            .ReturnsAsync(category);

        var exception = await Assert.ThrowsAsync<KeyNotFoundException>(
            () => _assignmentService.Add(request));

        Assert.Equal("Category not found", exception.Message);

        _assignmentRepositoryMock.Verify(
            repository => repository.GetByNameAndCategoryId(
                It.IsAny<string>(),
                It.IsAny<int>()),
            Times.Never);

        _assignmentRepositoryMock.Verify(
            repository => repository.Add(It.IsAny<Assignment>()),
            Times.Never);
    }

    [Fact]
    public async Task Add_DuplicateAssignment_ThrowsInvalidOperationException()
    {
        var request = new AssignmentRequest
        {
            CategoryId = 1,
            AssignmentName = "Study Mathematics"
        };

        var category = new Category
        {
            Id = 1,
            UserId = 42,
            CategoryName = "Personal",
        };

        var existingAssignment = new Assignment
        {
            Id = 5,
            CategoryId = 1,
            AssignmentName = "Study Mathematics",
        };

        _currentUserMock
            .Setup(currentUser => currentUser.UserId)
            .Returns(42);

        _categoryRepositoryMock
            .Setup(repository => repository.GetById(1))
            .ReturnsAsync(category);

        _assignmentRepositoryMock
            .Setup(repository => repository.GetByNameAndCategoryId(
                "Study Mathematics",
                1))
            .ReturnsAsync(existingAssignment);

        var exception = await Assert.ThrowsAsync<InvalidOperationException>(
            () => _assignmentService.Add(request));

        Assert.Equal("Assignment with this name already exists", exception.Message);

        _assignmentRepositoryMock.Verify(
            repository => repository.Add(It.IsAny<Assignment>()),
            Times.Never);
    }

    [Fact]
    public async Task GetAll_StatusFilter_ReturnsMappedAssignmentsForCurrentUser()
    {
        var assignments = new List<Assignment>
        {
            new()
            {
                Id = 5,
                CategoryId = 1,
                AssignmentName = "Study Mathematics",
                Status = AssignmentStatus.InProgress,
            },
            new()
            {
                Id = 6,
                CategoryId = 1,
                AssignmentName = "Practice Linux",
                Status = AssignmentStatus.InProgress,
            }
        };

        var mappedResponses = new List<AssignmentResponse>
        {
            new()
            {
                Id = 5,
                CategoryId = 1,
                AssignmentName = "Study Mathematics",
                Status = AssignmentStatus.InProgress,
            },
            new()
            {
                Id = 6,
                CategoryId = 1,
                AssignmentName = "Practice Linux",
                Status = AssignmentStatus.InProgress,
            }
        };

        _currentUserMock
            .Setup(currentUser => currentUser.UserId)
            .Returns(42);

        _assignmentRepositoryMock
            .Setup(repository => repository.GetByUserId(
                42,
                AssignmentStatus.InProgress))
            .ReturnsAsync(assignments);

        _mapperMock
            .Setup(mapper => mapper.Map<IEnumerable<AssignmentResponse>>(assignments))
            .Returns(mappedResponses);

        var result = await _assignmentService.GetAll(AssignmentStatus.InProgress);

        Assert.Same(mappedResponses, result);
    }

    [Fact]
    public async Task GetAll_InvalidStatus_ThrowsArgumentException()
    {
        var exception = await Assert.ThrowsAsync<ArgumentException>(
            () => _assignmentService.GetAll((AssignmentStatus)999));

        Assert.Equal("Invalid assignment status", exception.Message);
    }

    [Fact]
    public async Task GetById_OwnedAssignment_ReturnsMappedAssignment()
    {
        var assignment = new Assignment
        {
            Id = 5,
            CategoryId = 1,
            AssignmentName = "Study Mathematics",
            Category = new Category
            {
                Id = 1,
                UserId = 42,
                CategoryName = "Personal",
            },
        };

        var mappedResponse = new AssignmentResponse
        {
            Id = 5,
            CategoryId = 1,
            AssignmentName = "Study Mathematics",
        };

        _currentUserMock
            .Setup(currentUser => currentUser.UserId)
            .Returns(42);

        _assignmentRepositoryMock
            .Setup(repository => repository.GetByIdWithCategory(5))
            .ReturnsAsync(assignment);

        _mapperMock
            .Setup(mapper => mapper.Map<AssignmentResponse>(assignment))
            .Returns(mappedResponse);

        var result = await _assignmentService.GetById(5);

        Assert.Same(mappedResponse, result);
    }

    [Fact]
    public async Task GetById_AssignmentDoesNotExist_ThrowsKeyNotFoundException()
    {
        _assignmentRepositoryMock
            .Setup(repository => repository.GetByIdWithCategory(5))
            .ReturnsAsync((Assignment?)null);

        var exception = await Assert.ThrowsAsync<KeyNotFoundException>(
            () => _assignmentService.GetById(5));

        Assert.Equal("Assignment not found", exception.Message);
    }

    [Fact]
    public async Task GetById_AssignmentBelongsToAnotherUser_ThrowsKeyNotFoundException()
    {
        var assignment = new Assignment
        {
            Id = 5,
            CategoryId = 1,
            AssignmentName = "Study Mathematics",
            Category = new Category
            {
                Id = 1,
                UserId = 99,
                CategoryName = "Personal",
            },
        };

        _currentUserMock
            .Setup(currentUser => currentUser.UserId)
            .Returns(42);

        _assignmentRepositoryMock
            .Setup(repository => repository.GetByIdWithCategory(5))
            .ReturnsAsync(assignment);

        var exception = await Assert.ThrowsAsync<KeyNotFoundException>(
            () => _assignmentService.GetById(5));

        Assert.Equal("Assignment not found", exception.Message);
    }

    [Fact]
    public async Task Update_ValidAssignmentRequest_UpdatesAssignment()
    {
        var request = new AssignmentRequest
        {
            CategoryId = 2,
            AssignmentName = "Prepare Backend Interview",
            Description = "Review ASP.NET Core and unit testing",
        };

        var assignment = new Assignment
        {
            Id = 5,
            CategoryId = 1,
            AssignmentName = "Prepare Interview",
            Category = new Category
            {
                Id = 1,
                UserId = 42,
                CategoryName = "Personal",
            },
        };

        var workCategory = new Category
        {
            Id = 2,
            UserId = 42,
            CategoryName = "Work",
        };

        var updatedAssignment = new Assignment
        {
            Id = 5,
            CategoryId = 2,
            AssignmentName = "Prepare Backend Interview",
            Description = "Review ASP.NET Core and unit testing",
        };

        var assignmentResponse = new AssignmentResponse
        {
            Id = 5,
            CategoryId = 2,
            AssignmentName = "Prepare Backend Interview",
            Description = "Review ASP.NET Core and unit testing",
        };

        _currentUserMock
            .Setup(currentUser => currentUser.UserId)
            .Returns(42);

        _assignmentRepositoryMock
            .Setup(repository => repository.GetByIdWithCategory(5))
            .ReturnsAsync(assignment);

        _categoryRepositoryMock
            .Setup(repository => repository.GetById(2))
            .ReturnsAsync(workCategory);

        _assignmentRepositoryMock
            .Setup(repository => repository.GetByNameAndCategoryId(
                "Prepare Backend Interview",
                2))
            .ReturnsAsync((Assignment?)null);

        _assignmentRepositoryMock
            .Setup(repository => repository.Update(assignment))
            .ReturnsAsync(updatedAssignment);

        _mapperMock
            .Setup(mapper => mapper.Map<AssignmentResponse>(updatedAssignment))
            .Returns(assignmentResponse);

        var result = await _assignmentService.Update(5, request);

        Assert.Same(assignmentResponse, result);

        _mapperMock.Verify(
            mapper => mapper.Map(request, assignment),
            Times.Once);

        _assignmentRepositoryMock.Verify(
            repository => repository.Update(assignment),
            Times.Once);
    }

    [Fact]
    public async Task Update_InvalidAssignmentRequest_ThrowsArgumentException()
    {
        var request = new AssignmentRequest
        {
            CategoryId = 1,
            AssignmentName = "   ",
        };

        var exception = await Assert.ThrowsAsync<ArgumentException>(
            () => _assignmentService.Update(5, request));

        Assert.Equal("Assignment name is required", exception.Message);

        _assignmentRepositoryMock.Verify(
            repository => repository.Update(It.IsAny<Assignment>()),
            Times.Never);
    }

    [Fact]
    public async Task Update_AssignmentDoesNotExist_ThrowsKeyNotFoundException()
    {
        var request = new AssignmentRequest
        {
            CategoryId = 1,
            AssignmentName = "Study Mathematics",
        };

        _assignmentRepositoryMock
            .Setup(repository => repository.GetByIdWithCategory(5))
            .ReturnsAsync((Assignment?)null);

        var exception = await Assert.ThrowsAsync<KeyNotFoundException>(
            () => _assignmentService.Update(5, request));

        Assert.Equal("Assignment not found", exception.Message);

        _assignmentRepositoryMock.Verify(
            repository => repository.Update(It.IsAny<Assignment>()),
            Times.Never);
    }

    [Fact]
    public async Task Update_AssignmentBelongsToAnotherUser_ThrowsKeyNotFoundException()
    {
        var request = new AssignmentRequest
        {
            CategoryId = 1,
            AssignmentName = "Study Mathematics",
        };

        var assignment = new Assignment
        {
            Id = 5,
            CategoryId = 1,
            AssignmentName = "Study Mathematics",
            Category = new Category
            {
                Id = 1,
                UserId = 99,
                CategoryName = "Personal",
            },
        };

        _currentUserMock
            .Setup(currentUser => currentUser.UserId)
            .Returns(42);

        _assignmentRepositoryMock
            .Setup(repository => repository.GetByIdWithCategory(5))
            .ReturnsAsync(assignment);

        var exception = await Assert.ThrowsAsync<KeyNotFoundException>(
            () => _assignmentService.Update(5, request));

        Assert.Equal("Assignment not found", exception.Message);

        _assignmentRepositoryMock.Verify(
            repository => repository.Update(It.IsAny<Assignment>()),
            Times.Never);
    }

    [Fact]
    public async Task Update_ArchivedAssignment_ThrowsInvalidOperationException()
    {
        var request = new AssignmentRequest
        {
            CategoryId = 1,
            AssignmentName = "Study Mathematics",
        };

        var assignment = new Assignment
        {
            Id = 5,
            CategoryId = 1,
            AssignmentName = "Study Mathematics",
            ArchivedAt = new DateTime(2026, 11, 20, 10, 0, 0, DateTimeKind.Utc),
            Category = new Category
            {
                Id = 1,
                UserId = 42,
                CategoryName = "Personal",
            },
        };

        _currentUserMock
            .Setup(currentUser => currentUser.UserId)
            .Returns(42);

        _assignmentRepositoryMock
            .Setup(repository => repository.GetByIdWithCategory(5))
            .ReturnsAsync(assignment);

        var exception = await Assert.ThrowsAsync<InvalidOperationException>(
            () => _assignmentService.Update(5, request));

        Assert.Equal("Assignment is archived", exception.Message);

        _assignmentRepositoryMock.Verify(
            repository => repository.Update(It.IsAny<Assignment>()),
            Times.Never);
    }

    [Fact]
    public async Task Update_CategoryDoesNotExist_ThrowsKeyNotFoundException()
    {
        var request = new AssignmentRequest
        {
            CategoryId = 2,
            AssignmentName = "Study Mathematics",
        };

        var assignment = new Assignment
        {
            Id = 5,
            CategoryId = 1,
            AssignmentName = "Study Mathematics",
            Category = new Category
            {
                Id = 1,
                UserId = 42,
                CategoryName = "Personal",
            },
        };

        _currentUserMock
            .Setup(currentUser => currentUser.UserId)
            .Returns(42);

        _assignmentRepositoryMock
            .Setup(repository => repository.GetByIdWithCategory(5))
            .ReturnsAsync(assignment);

        _categoryRepositoryMock
            .Setup(repository => repository.GetById(2))
            .ReturnsAsync((Category?)null);

        var exception = await Assert.ThrowsAsync<KeyNotFoundException>(
            () => _assignmentService.Update(5, request));

        Assert.Equal("Category not found", exception.Message);

        _assignmentRepositoryMock.Verify(
            repository => repository.Update(It.IsAny<Assignment>()),
            Times.Never);
    }

    [Fact]
    public async Task Update_CategoryBelongsToAnotherUser_ThrowsKeyNotFoundException()
    {
        var request = new AssignmentRequest
        {
            CategoryId = 2,
            AssignmentName = "Study Mathematics",
        };

        var assignment = new Assignment
        {
            Id = 5,
            CategoryId = 1,
            AssignmentName = "Study Mathematics",
            Category = new Category
            {
                Id = 1,
                UserId = 42,
                CategoryName = "Personal",
            },
        };

        var otherUserCategory = new Category
        {
            Id = 2,
            UserId = 99,
            CategoryName = "Work",
        };

        _currentUserMock
            .Setup(currentUser => currentUser.UserId)
            .Returns(42);

        _assignmentRepositoryMock
            .Setup(repository => repository.GetByIdWithCategory(5))
            .ReturnsAsync(assignment);

        _categoryRepositoryMock
            .Setup(repository => repository.GetById(2))
            .ReturnsAsync(otherUserCategory);

        var exception = await Assert.ThrowsAsync<KeyNotFoundException>(
            () => _assignmentService.Update(5, request));

        Assert.Equal("Category not found", exception.Message);

        _assignmentRepositoryMock.Verify(
            repository => repository.GetByNameAndCategoryId(
                It.IsAny<string>(),
                It.IsAny<int>()),
            Times.Never);

        _assignmentRepositoryMock.Verify(
            repository => repository.Update(It.IsAny<Assignment>()),
            Times.Never);
    }

    [Fact]
    public async Task Update_DuplicateAssignment_ThrowsInvalidOperationException()
    {
        var request = new AssignmentRequest
        {
            CategoryId = 1,
            AssignmentName = "Practice Linux",
        };

        var category = new Category
        {
            Id = 1,
            UserId = 42,
            CategoryName = "Personal",
        };

        var assignment = new Assignment
        {
            Id = 5,
            CategoryId = 1,
            AssignmentName = "Study Mathematics",
            Category = category,
        };

        var existingAssignment = new Assignment
        {
            Id = 6,
            CategoryId = 1,
            AssignmentName = "Practice Linux",
        };

        _currentUserMock
            .Setup(currentUser => currentUser.UserId)
            .Returns(42);

        _assignmentRepositoryMock
            .Setup(repository => repository.GetByIdWithCategory(5))
            .ReturnsAsync(assignment);

        _categoryRepositoryMock
            .Setup(repository => repository.GetById(1))
            .ReturnsAsync(category);

        _assignmentRepositoryMock
            .Setup(repository => repository.GetByNameAndCategoryId(
                "Practice Linux",
                1))
            .ReturnsAsync(existingAssignment);

        var exception = await Assert.ThrowsAsync<InvalidOperationException>(
            () => _assignmentService.Update(5, request));

        Assert.Equal(
            "Assignment with this name already exists",
            exception.Message);

        _assignmentRepositoryMock.Verify(
            repository => repository.Update(
                It.IsAny<Assignment>()),
            Times.Never);
    }

    [Fact]
    public async Task Update_SameAssignmentName_UpdatesAssignment()
    {
        var request = new AssignmentRequest
        {
            CategoryId = 1,
            AssignmentName = "Study Mathematics",
            Description = "Complete calculus exercises",
        };

        var category = new Category
        {
            Id = 1,
            UserId = 42,
            CategoryName = "Personal",
        };

        var assignment = new Assignment
        {
            Id = 5,
            CategoryId = 1,
            AssignmentName = "Study Mathematics",
            Category = category,
        };

        var mappedResponse = new AssignmentResponse
        {
            Id = 5,
            CategoryId = 1,
            AssignmentName = "Study Mathematics",
            Description = "Complete calculus exercises",
        };

        _currentUserMock
            .Setup(currentUser => currentUser.UserId)
            .Returns(42);

        _assignmentRepositoryMock
            .Setup(repository => repository.GetByIdWithCategory(5))
            .ReturnsAsync(assignment);

        _categoryRepositoryMock
            .Setup(repository => repository.GetById(1))
            .ReturnsAsync(category);

        _assignmentRepositoryMock
            .Setup(repository => repository.GetByNameAndCategoryId(
                "Study Mathematics", 1))
            .ReturnsAsync(assignment);

        _assignmentRepositoryMock
            .Setup(repository => repository.Update(assignment))
            .ReturnsAsync(assignment);

        _mapperMock
            .Setup(mapper => mapper.Map<AssignmentResponse>(assignment))
            .Returns(mappedResponse);

        var result = await _assignmentService.Update(5, request);

        Assert.Same(mappedResponse, result);

        _assignmentRepositoryMock.Verify(
            repository => repository.Update(assignment),
            Times.Once);
    }

    [Fact]
    public async Task Delete_OwnedAssignmentWithoutActiveTimeEntry_DeletesAssignment()
    {
        var assignment = new Assignment
        {
            Id = 5,
            CategoryId = 1,
            AssignmentName = "Study Mathematics",
            Category = new Category
            {
                Id = 1,
                UserId = 42,
                CategoryName = "Personal",
            },
        };

        _currentUserMock
            .Setup(currentUser => currentUser.UserId)
            .Returns(42);

        _assignmentRepositoryMock
            .Setup(repository => repository.GetByIdWithCategory(5))
            .ReturnsAsync(assignment);

        _timeEntryRepositoryMock
            .Setup(repository => repository.GetRunningByUserId(42))
            .ReturnsAsync((TimeEntry?)null);

        await _assignmentService.Delete(5);

        _timeEntryRepositoryMock.Verify(
            repository => repository.GetRunningByUserId(42),
            Times.Once);

        _assignmentRepositoryMock.Verify(
            repository => repository.Delete(5),
            Times.Once);
    }

    [Fact]
    public async Task Delete_AssignmentDoesNotExist_ThrowsKeyNotFoundException()
    {
        _assignmentRepositoryMock
            .Setup(repository => repository.GetByIdWithCategory(5))
            .ReturnsAsync((Assignment?)null);

        var exception = await Assert.ThrowsAsync<KeyNotFoundException>(
            () => _assignmentService.Delete(5));

        Assert.Equal("Assignment not found", exception.Message);

        _assignmentRepositoryMock.Verify(
            repository => repository.Delete(It.IsAny<int>()),
            Times.Never);
    }

    [Fact]
    public async Task Delete_AssignmentBelongsToAnotherUser_ThrowsKeyNotFoundException()
    {
        var assignment = new Assignment
        {
            Id = 5,
            CategoryId = 1,
            AssignmentName = "Study Mathematics",
            Category = new Category
            {
                Id = 1,
                UserId = 99,
                CategoryName = "Personal",
            },
        };

        _currentUserMock
            .Setup(currentUser => currentUser.UserId)
            .Returns(42);

        _assignmentRepositoryMock
            .Setup(repository => repository.GetByIdWithCategory(5))
            .ReturnsAsync(assignment);

        var exception = await Assert.ThrowsAsync<KeyNotFoundException>(
            () => _assignmentService.Delete(5));

        Assert.Equal("Assignment not found", exception.Message);

        _assignmentRepositoryMock.Verify(
            repository => repository.Delete(It.IsAny<int>()),
            Times.Never);
    }

    [Fact]
    public async Task Delete_AssignmentWithActiveTimeEntry_ThrowsInvalidOperationException()
    {
        var assignment = new Assignment
        {
            Id = 5,
            CategoryId = 1,
            AssignmentName = "Study Mathematics",
            Category = new Category
            {
                Id = 1,
                UserId = 42,
                CategoryName = "Personal",
            },
        };

        var activeTimeEntry = new TimeEntry
        {
            Id = 10,
            UserId = 42,
            AssignmentId = 5,
            StartedAt = new DateTime(2026, 11, 24, 9, 0, 0, DateTimeKind.Utc),
            EndedAt = null,
        };

        _currentUserMock
            .Setup(currentUser => currentUser.UserId)
            .Returns(42);

        _assignmentRepositoryMock
            .Setup(repository => repository.GetByIdWithCategory(5))
            .ReturnsAsync(assignment);

        _timeEntryRepositoryMock
            .Setup(repository => repository.GetRunningByUserId(42))
            .ReturnsAsync(activeTimeEntry);

        var exception = await Assert.ThrowsAsync<InvalidOperationException>(
            () => _assignmentService.Delete(5));

        Assert.Equal("Cannot delete an assignment with active time entry", exception.Message);

        _assignmentRepositoryMock.Verify(
            repository => repository.Delete(It.IsAny<int>()),
            Times.Never);
    }

    [Fact]
    public async Task Delete_ActiveTimeEntryOnAnotherAssignment_DeletesAssignment()
    {
        var assignment = new Assignment
        {
            Id = 5,
            CategoryId = 1,
            AssignmentName = "Study Mathematics",
            Category = new Category
            {
                Id = 1,
                UserId = 42,
                CategoryName = "Personal",
            },
        };

        var activeTimeEntry = new TimeEntry
        {
            Id = 10,
            UserId = 42,
            AssignmentId = 6,
            StartedAt = new DateTime(2026, 11, 24, 9, 0, 0, DateTimeKind.Utc),
            EndedAt = null,
        };

        _currentUserMock
            .Setup(currentUser => currentUser.UserId)
            .Returns(42);

        _assignmentRepositoryMock
            .Setup(repository => repository.GetByIdWithCategory(5))
            .ReturnsAsync(assignment);

        _timeEntryRepositoryMock
            .Setup(repository => repository.GetRunningByUserId(42))
            .ReturnsAsync(activeTimeEntry);

        await _assignmentService.Delete(5);

        _assignmentRepositoryMock.Verify(
            repository => repository.Delete(5),
            Times.Once);
    }

    [Fact]
    public async Task ChangeStatus_Done_CompletesAssignment()
    {
        var assignment = new Assignment
        {
            Id = 5,
            CategoryId = 1,
            AssignmentName = "Study Mathematics",
            Status = AssignmentStatus.InProgress,
            Category = new Category
            {
                Id = 1,
                UserId = 42,
                CategoryName = "Personal",
            },
        };

        var mappedResponse = new AssignmentResponse
        {
            Id = 5,
            CategoryId = 1,
            AssignmentName = "Study Mathematics",
            Status = AssignmentStatus.Done,
        };

        _currentUserMock
            .Setup(currentUser => currentUser.UserId)
            .Returns(42);

        _assignmentRepositoryMock
            .Setup(repository => repository.GetByIdWithCategory(5))
            .ReturnsAsync(assignment);

        _timeEntryRepositoryMock
            .Setup(repository => repository.GetRunningByUserId(42))
            .ReturnsAsync((TimeEntry?)null);

        _assignmentRepositoryMock
            .Setup(repository => repository.Update(assignment))
            .ReturnsAsync(assignment);

        _mapperMock
            .Setup(mapper => mapper.Map<AssignmentResponse>(assignment))
            .Returns(mappedResponse);

        var before = DateTime.UtcNow;

        var result = await _assignmentService.ChangeStatus(5, AssignmentStatus.Done);

        var after = DateTime.UtcNow;

        Assert.Same(mappedResponse, result);
        Assert.Equal(AssignmentStatus.Done, assignment.Status);
        Assert.NotNull(assignment.CompletedAt);
        Assert.InRange(assignment.CompletedAt.Value, before, after);

        _assignmentRepositoryMock.Verify(
            repository => repository.Update(assignment),
            Times.Once);
    }

    [Fact]
    public async Task ChangeStatus_InvalidStatus_ThrowsArgumentException()
    {
        var exception = await Assert.ThrowsAsync<ArgumentException>(
            () => _assignmentService.ChangeStatus(5, (AssignmentStatus)999));

        Assert.Equal("Invalid assignment status", exception.Message);

        _assignmentRepositoryMock.Verify(
            repository => repository.Update(It.IsAny<Assignment>()),
            Times.Never);
    }

    [Fact]
    public async Task ChangeStatus_AssignmentDoesNotExist_ThrowsKeyNotFoundException()
    {
        _assignmentRepositoryMock
            .Setup(repository => repository.GetByIdWithCategory(5))
            .ReturnsAsync((Assignment?)null);

        var exception = await Assert.ThrowsAsync<KeyNotFoundException>(
            () => _assignmentService.ChangeStatus(5, AssignmentStatus.Done));

        Assert.Equal("Assignment not found", exception.Message);

        _assignmentRepositoryMock.Verify(
            repository => repository.Update(It.IsAny<Assignment>()),
            Times.Never);
    }

    [Fact]
    public async Task ChangeStatus_AssignmentBelongsToAnotherUser_ThrowsKeyNotFoundException()
    {
        var assignment = new Assignment
        {
            Id = 5,
            CategoryId = 1,
            AssignmentName = "Study Mathematics",
            Category = new Category
            {
                Id = 1,
                UserId = 99,
                CategoryName = "Personal",
            },
        };

        _currentUserMock
            .Setup(currentUser => currentUser.UserId)
            .Returns(42);

        _assignmentRepositoryMock
            .Setup(repository => repository.GetByIdWithCategory(5))
            .ReturnsAsync(assignment);

        var exception = await Assert.ThrowsAsync<KeyNotFoundException>(
            () => _assignmentService.ChangeStatus(5, AssignmentStatus.Done));

        Assert.Equal("Assignment not found", exception.Message);

        _assignmentRepositoryMock.Verify(
            repository => repository.Update(It.IsAny<Assignment>()),
            Times.Never);
    }

    [Fact]
    public async Task ChangeStatus_ArchivedAssignment_ThrowsInvalidOperationException()
    {
        var assignment = new Assignment
        {
            Id = 5,
            CategoryId = 1,
            AssignmentName = "Study Mathematics",
            Status = AssignmentStatus.InProgress,
            ArchivedAt = new DateTime(2026, 11, 20, 10, 0, 0, DateTimeKind.Utc),
            Category = new Category
            {
                Id = 1,
                UserId = 42,
                CategoryName = "Personal",
            },
        };

        _currentUserMock
            .Setup(currentUser => currentUser.UserId)
            .Returns(42);

        _assignmentRepositoryMock
            .Setup(repository => repository.GetByIdWithCategory(5))
            .ReturnsAsync(assignment);

        var exception = await Assert.ThrowsAsync<InvalidOperationException>(
            () => _assignmentService.ChangeStatus(5, AssignmentStatus.Done));

        Assert.Equal("Assignment is archived", exception.Message);
        Assert.Equal(AssignmentStatus.InProgress, assignment.Status);

        _assignmentRepositoryMock.Verify(
            repository => repository.Update(It.IsAny<Assignment>()),
            Times.Never);
    }

    [Fact]
    public async Task ChangeStatus_DoneWithActiveTimeEntry_StopsTimeEntry()
    {
        var assignment = new Assignment
        {
            Id = 5,
            CategoryId = 1,
            AssignmentName = "Study Mathematics",
            Status = AssignmentStatus.InProgress,
            Category = new Category
            {
                Id = 1,
                UserId = 42,
                CategoryName = "Personal",
            },
        };

        var activeTimeEntry = new TimeEntry
        {
            Id = 10,
            UserId = 42,
            AssignmentId = 5,
            StartedAt = new DateTime(2026, 11, 24, 9, 0, 0, DateTimeKind.Utc),
            EndedAt = null,
        };

        _currentUserMock
            .Setup(currentUser => currentUser.UserId)
            .Returns(42);

        _assignmentRepositoryMock
            .Setup(repository => repository.GetByIdWithCategory(5))
            .ReturnsAsync(assignment);

        _timeEntryRepositoryMock
            .Setup(repository => repository.GetRunningByUserId(42))
            .ReturnsAsync(activeTimeEntry);

        _assignmentRepositoryMock
            .Setup(repository => repository.Update(assignment))
            .ReturnsAsync(assignment);

        var before = DateTime.UtcNow;

        await _assignmentService.ChangeStatus(5, AssignmentStatus.Done);

        var after = DateTime.UtcNow;

        Assert.NotNull(activeTimeEntry.EndedAt);
        Assert.InRange(activeTimeEntry.EndedAt.Value, before, after);
        Assert.Equal(assignment.CompletedAt, activeTimeEntry.EndedAt);

        _assignmentRepositoryMock.Verify(
            repository => repository.Update(assignment),
            Times.Once);
    }

    [Fact]
    public async Task ChangeStatus_DoneWithActiveTimeEntryOnAnotherAssignment_KeepsTimeEntryRunning()
    {
        var assignment = new Assignment
        {
            Id = 5,
            CategoryId = 1,
            AssignmentName = "Study Mathematics",
            Status = AssignmentStatus.InProgress,
            Category = new Category
            {
                Id = 1,
                UserId = 42,
                CategoryName = "Personal",
            },
        };

        var activeTimeEntry = new TimeEntry
        {
            Id = 10,
            UserId = 42,
            AssignmentId = 6,
            StartedAt = new DateTime(2026, 11, 24, 9, 0, 0, DateTimeKind.Utc),
            EndedAt = null,
        };

        _currentUserMock
            .Setup(currentUser => currentUser.UserId)
            .Returns(42);

        _assignmentRepositoryMock
            .Setup(repository => repository.GetByIdWithCategory(5))
            .ReturnsAsync(assignment);

        _timeEntryRepositoryMock
            .Setup(repository => repository.GetRunningByUserId(42))
            .ReturnsAsync(activeTimeEntry);

        _assignmentRepositoryMock
            .Setup(repository => repository.Update(assignment))
            .ReturnsAsync(assignment);

        await _assignmentService.ChangeStatus(5, AssignmentStatus.Done);

        Assert.Null(activeTimeEntry.EndedAt);
        Assert.Equal(AssignmentStatus.Done, assignment.Status);

        _assignmentRepositoryMock.Verify(
            repository => repository.Update(assignment),
            Times.Once);
    }

    [Fact]
    public async Task ChangeStatus_DoneWhenAlreadyCompleted_KeepsOriginalCompletedAt()
    {
        var originalCompletedAt = new DateTime(2026, 11, 20, 15, 0, 0, DateTimeKind.Utc);

        var assignment = new Assignment
        {
            Id = 5,
            CategoryId = 1,
            AssignmentName = "Study Mathematics",
            Status = AssignmentStatus.Done,
            CompletedAt = originalCompletedAt,
            Category = new Category
            {
                Id = 1,
                UserId = 42,
                CategoryName = "Personal",
            },
        };

        _currentUserMock
            .Setup(currentUser => currentUser.UserId)
            .Returns(42);

        _assignmentRepositoryMock
            .Setup(repository => repository.GetByIdWithCategory(5))
            .ReturnsAsync(assignment);

        _timeEntryRepositoryMock
            .Setup(repository => repository.GetRunningByUserId(42))
            .ReturnsAsync((TimeEntry?)null);

        _assignmentRepositoryMock
            .Setup(repository => repository.Update(assignment))
            .ReturnsAsync(assignment);

        await _assignmentService.ChangeStatus(5, AssignmentStatus.Done);

        Assert.Equal(originalCompletedAt, assignment.CompletedAt);
        Assert.Equal(AssignmentStatus.Done, assignment.Status);
    }

    [Fact]
    public async Task ChangeStatus_ToDoWithActiveTimeEntry_ThrowsInvalidOperationException()
    {
        var assignment = new Assignment
        {
            Id = 5,
            CategoryId = 1,
            AssignmentName = "Study Mathematics",
            Status = AssignmentStatus.InProgress,
            Category = new Category
            {
                Id = 1,
                UserId = 42,
                CategoryName = "Personal",
            },
        };

        var activeTimeEntry = new TimeEntry
        {
            Id = 10,
            UserId = 42,
            AssignmentId = 5,
            StartedAt = new DateTime(2026, 11, 24, 9, 0, 0, DateTimeKind.Utc),
            EndedAt = null,
        };

        _currentUserMock
            .Setup(currentUser => currentUser.UserId)
            .Returns(42);

        _assignmentRepositoryMock
            .Setup(repository => repository.GetByIdWithCategory(5))
            .ReturnsAsync(assignment);

        _timeEntryRepositoryMock
            .Setup(repository => repository.GetRunningByUserId(42))
            .ReturnsAsync(activeTimeEntry);

        var exception = await Assert.ThrowsAsync<InvalidOperationException>(
            () => _assignmentService.ChangeStatus(5, AssignmentStatus.ToDo));

        Assert.Equal(
            "Cannot move an assignment with an active time entry back to To Do",
            exception.Message);
        Assert.Equal(AssignmentStatus.InProgress, assignment.Status);

        _assignmentRepositoryMock.Verify(
            repository => repository.Update(It.IsAny<Assignment>()),
            Times.Never);
    }

    [Fact]
    public async Task ChangeStatus_ToDoWithoutActiveTimeEntry_MovesAssignmentToToDo()
    {
        var assignment = new Assignment
        {
            Id = 5,
            CategoryId = 1,
            AssignmentName = "Study Mathematics",
            Status = AssignmentStatus.InProgress,
            Category = new Category
            {
                Id = 1,
                UserId = 42,
                CategoryName = "Personal",
            },
        };

        _currentUserMock
            .Setup(currentUser => currentUser.UserId)
            .Returns(42);

        _assignmentRepositoryMock
            .Setup(repository => repository.GetByIdWithCategory(5))
            .ReturnsAsync(assignment);

        _timeEntryRepositoryMock
            .Setup(repository => repository.GetRunningByUserId(42))
            .ReturnsAsync((TimeEntry?)null);

        _assignmentRepositoryMock
            .Setup(repository => repository.Update(assignment))
            .ReturnsAsync(assignment);

        await _assignmentService.ChangeStatus(5, AssignmentStatus.ToDo);

        Assert.Equal(AssignmentStatus.ToDo, assignment.Status);

        _assignmentRepositoryMock.Verify(
            repository => repository.Update(assignment),
            Times.Once);
    }

    [Fact]
    public async Task ChangeStatus_ToDoWithActiveTimeEntryOnAnotherAssignment_ClearsCompletedAt()
    {
        var assignment = new Assignment
        {
            Id = 5,
            CategoryId = 1,
            AssignmentName = "Study Mathematics",
            Status = AssignmentStatus.Done,
            CompletedAt = new DateTime(2026, 11, 20, 15, 0, 0, DateTimeKind.Utc),
            Category = new Category
            {
                Id = 1,
                UserId = 42,
                CategoryName = "Personal",
            },
        };

        var activeTimeEntry = new TimeEntry
        {
            Id = 10,
            UserId = 42,
            AssignmentId = 6,
            StartedAt = new DateTime(2026, 11, 24, 9, 0, 0, DateTimeKind.Utc),
            EndedAt = null,
        };

        _currentUserMock
            .Setup(currentUser => currentUser.UserId)
            .Returns(42);

        _assignmentRepositoryMock
            .Setup(repository => repository.GetByIdWithCategory(5))
            .ReturnsAsync(assignment);

        _timeEntryRepositoryMock
            .Setup(repository => repository.GetRunningByUserId(42))
            .ReturnsAsync(activeTimeEntry);

        _assignmentRepositoryMock
            .Setup(repository => repository.Update(assignment))
            .ReturnsAsync(assignment);

        await _assignmentService.ChangeStatus(5, AssignmentStatus.ToDo);

        Assert.Null(assignment.CompletedAt);
        Assert.Equal(AssignmentStatus.ToDo, assignment.Status);

        _assignmentRepositoryMock.Verify(
            repository => repository.Update(assignment),
            Times.Once);
    }

    [Fact]
    public async Task ChangeStatus_InProgressWithActiveTimeEntry_ClearsCompletedAt()
    {
        var assignment = new Assignment
        {
            Id = 5,
            CategoryId = 1,
            AssignmentName = "Study Mathematics",
            Status = AssignmentStatus.Done,
            CompletedAt = new DateTime(2026, 11, 20, 15, 0, 0, DateTimeKind.Utc),
            Category = new Category
            {
                Id = 1,
                UserId = 42,
                CategoryName = "Personal",
            },
        };

        var activeTimeEntry = new TimeEntry
        {
            Id = 10,
            UserId = 42,
            AssignmentId = 5,
            StartedAt = new DateTime(2026, 11, 24, 9, 0, 0, DateTimeKind.Utc),
            EndedAt = null,
        };

        _currentUserMock
            .Setup(currentUser => currentUser.UserId)
            .Returns(42);

        _assignmentRepositoryMock
            .Setup(repository => repository.GetByIdWithCategory(5))
            .ReturnsAsync(assignment);

        _timeEntryRepositoryMock
            .Setup(repository => repository.GetRunningByUserId(42))
            .ReturnsAsync(activeTimeEntry);

        _assignmentRepositoryMock
            .Setup(repository => repository.Update(assignment))
            .ReturnsAsync(assignment);

        await _assignmentService.ChangeStatus(5, AssignmentStatus.InProgress);

        Assert.Null(assignment.CompletedAt);
        Assert.Equal(AssignmentStatus.InProgress, assignment.Status);
        Assert.Null(activeTimeEntry.EndedAt);

        _assignmentRepositoryMock.Verify(
            repository => repository.Update(assignment),
            Times.Once);
    }

    [Fact]
    public async Task GetByCategoryId_OwnedCategory_ReturnsMappedAssignments()
    {
        var category = new Category
        {
            Id = 1,
            UserId = 42,
            CategoryName = "Personal",
        };

        var assignments = new List<Assignment>
        {
            new()
            {
                Id = 5,
                CategoryId = 1,
                AssignmentName = "Study Mathematics",
            },
            new()
            {
                Id = 6,
                CategoryId = 1,
                AssignmentName = "Practice Linux",
            }
        };

        var mappedResponses = new List<AssignmentResponse>
        {
            new()
            {
                Id = 5,
                CategoryId = 1,
                AssignmentName = "Study Mathematics",
            },
            new()
            {
                Id = 6,
                CategoryId = 1,
                AssignmentName = "Practice Linux",
            }
        };

        _currentUserMock
            .Setup(currentUser => currentUser.UserId)
            .Returns(42);

        _categoryRepositoryMock
            .Setup(repository => repository.GetById(1))
            .ReturnsAsync(category);

        _assignmentRepositoryMock
            .Setup(repository => repository.GetByCategoryId(1))
            .ReturnsAsync(assignments);

        _mapperMock
            .Setup(mapper => mapper.Map<IEnumerable<AssignmentResponse>>(assignments))
            .Returns(mappedResponses);

        var result = await _assignmentService.GetByCategoryId(1);

        Assert.Same(mappedResponses, result);
    }

    [Fact]
    public async Task GetByCategoryId_CategoryDoesNotExist_ThrowsKeyNotFoundException()
    {
        _categoryRepositoryMock
            .Setup(repository => repository.GetById(1))
            .ReturnsAsync((Category?)null);

        var exception = await Assert.ThrowsAsync<KeyNotFoundException>(
            () => _assignmentService.GetByCategoryId(1));

        Assert.Equal("Category not found", exception.Message);
    }

    [Fact]
    public async Task GetByCategoryId_CategoryBelongsToAnotherUser_ThrowsKeyNotFoundException()
    {
        var category = new Category
        {
            Id = 1,
            UserId = 99,
            CategoryName = "Personal",
        };

        _currentUserMock
            .Setup(currentUser => currentUser.UserId)
            .Returns(42);

        _categoryRepositoryMock
            .Setup(repository => repository.GetById(1))
            .ReturnsAsync(category);

        var exception = await Assert.ThrowsAsync<KeyNotFoundException>(
            () => _assignmentService.GetByCategoryId(1));

        Assert.Equal("Category not found", exception.Message);
    }

    [Fact]
    public async Task Archive_OwnedAssignmentWithoutActiveTimeEntry_ArchivesAssignment()
    {
        var assignment = new Assignment
        {
            Id = 5,
            CategoryId = 1,
            AssignmentName = "Study Mathematics",
            Category = new Category
            {
                Id = 1,
                UserId = 42,
                CategoryName = "Personal",
            },
        };

        _currentUserMock
            .Setup(currentUser => currentUser.UserId)
            .Returns(42);

        _assignmentRepositoryMock
            .Setup(repository => repository.GetByIdWithCategory(5))
            .ReturnsAsync(assignment);

        _timeEntryRepositoryMock
            .Setup(repository => repository.GetRunningByUserId(42))
            .ReturnsAsync((TimeEntry?)null);

        var before = DateTime.UtcNow;

        await _assignmentService.Archive(5);

        var after = DateTime.UtcNow;

        Assert.NotNull(assignment.ArchivedAt);
        Assert.InRange(assignment.ArchivedAt.Value, before, after);

        _assignmentRepositoryMock.Verify(
            repository => repository.Update(assignment),
            Times.Once);
    }

    [Fact]
    public async Task Archive_AssignmentDoesNotExist_ThrowsKeyNotFoundException()
    {
        _assignmentRepositoryMock
            .Setup(repository => repository.GetByIdWithCategory(5))
            .ReturnsAsync((Assignment?)null);

        var exception = await Assert.ThrowsAsync<KeyNotFoundException>(
            () => _assignmentService.Archive(5));

        Assert.Equal("Assignment not found", exception.Message);

        _assignmentRepositoryMock.Verify(
            repository => repository.Update(It.IsAny<Assignment>()),
            Times.Never);
    }

    [Fact]
    public async Task Archive_AssignmentBelongsToAnotherUser_ThrowsKeyNotFoundException()
    {
        var assignment = new Assignment
        {
            Id = 5,
            CategoryId = 1,
            AssignmentName = "Study Mathematics",
            Category = new Category
            {
                Id = 1,
                UserId = 99,
                CategoryName = "Personal",
            },
        };

        _currentUserMock
            .Setup(currentUser => currentUser.UserId)
            .Returns(42);

        _assignmentRepositoryMock
            .Setup(repository => repository.GetByIdWithCategory(5))
            .ReturnsAsync(assignment);

        var exception = await Assert.ThrowsAsync<KeyNotFoundException>(
            () => _assignmentService.Archive(5));

        Assert.Equal("Assignment not found", exception.Message);
        Assert.Null(assignment.ArchivedAt);

        _assignmentRepositoryMock.Verify(
            repository => repository.Update(It.IsAny<Assignment>()),
            Times.Never);
    }

    [Fact]
    public async Task Archive_AlreadyArchivedAssignment_ThrowsInvalidOperationException()
    {
        var originalArchivedAt = new DateTime(2026, 11, 20, 10, 0, 0, DateTimeKind.Utc);

        var assignment = new Assignment
        {
            Id = 5,
            CategoryId = 1,
            AssignmentName = "Study Mathematics",
            ArchivedAt = originalArchivedAt,
            Category = new Category
            {
                Id = 1,
                UserId = 42,
                CategoryName = "Personal",
            },
        };

        _currentUserMock
            .Setup(currentUser => currentUser.UserId)
            .Returns(42);

        _assignmentRepositoryMock
            .Setup(repository => repository.GetByIdWithCategory(5))
            .ReturnsAsync(assignment);

        var exception = await Assert.ThrowsAsync<InvalidOperationException>(
            () => _assignmentService.Archive(5));

        Assert.Equal("Assignment is archived", exception.Message);
        Assert.Equal(originalArchivedAt, assignment.ArchivedAt);

        _assignmentRepositoryMock.Verify(
            repository => repository.Update(It.IsAny<Assignment>()),
            Times.Never);
    }

    [Fact]
    public async Task Archive_AssignmentWithActiveTimeEntry_ThrowsInvalidOperationException()
    {
        var assignment = new Assignment
        {
            Id = 5,
            CategoryId = 1,
            AssignmentName = "Study Mathematics",
            Category = new Category
            {
                Id = 1,
                UserId = 42,
                CategoryName = "Personal",
            },
        };

        var activeTimeEntry = new TimeEntry
        {
            Id = 10,
            UserId = 42,
            AssignmentId = 5,
            StartedAt = new DateTime(2026, 11, 24, 9, 0, 0, DateTimeKind.Utc),
            EndedAt = null,
        };

        _currentUserMock
            .Setup(currentUser => currentUser.UserId)
            .Returns(42);

        _assignmentRepositoryMock
            .Setup(repository => repository.GetByIdWithCategory(5))
            .ReturnsAsync(assignment);

        _timeEntryRepositoryMock
            .Setup(repository => repository.GetRunningByUserId(42))
            .ReturnsAsync(activeTimeEntry);

        var exception = await Assert.ThrowsAsync<InvalidOperationException>(
            () => _assignmentService.Archive(5));

        Assert.Equal(
            "Cannot archive an assignment with an active time entry",
            exception.Message);
        Assert.Null(assignment.ArchivedAt);

        _assignmentRepositoryMock.Verify(
            repository => repository.Update(It.IsAny<Assignment>()),
            Times.Never);
    }

    [Fact]
    public async Task Archive_ActiveTimeEntryOnAnotherAssignment_ArchivesAssignment()
    {
        var assignment = new Assignment
        {
            Id = 5,
            CategoryId = 1,
            AssignmentName = "Study Mathematics",
            Category = new Category
            {
                Id = 1,
                UserId = 42,
                CategoryName = "Personal",
            },
        };

        var activeTimeEntry = new TimeEntry
        {
            Id = 10,
            UserId = 42,
            AssignmentId = 6,
            StartedAt = new DateTime(2026, 11, 24, 9, 0, 0, DateTimeKind.Utc),
            EndedAt = null,
        };

        _currentUserMock
            .Setup(currentUser => currentUser.UserId)
            .Returns(42);

        _assignmentRepositoryMock
            .Setup(repository => repository.GetByIdWithCategory(5))
            .ReturnsAsync(assignment);

        _timeEntryRepositoryMock
            .Setup(repository => repository.GetRunningByUserId(42))
            .ReturnsAsync(activeTimeEntry);

        await _assignmentService.Archive(5);

        Assert.NotNull(assignment.ArchivedAt);

        _assignmentRepositoryMock.Verify(
            repository => repository.Update(assignment),
            Times.Once);
    }

    [Fact]
    public async Task GetArchivedByUserId_CurrentUserHasArchivedAssignments_ReturnsMappedAssignments()
    {
        var archivedAssignments = new List<Assignment>
        {
            new()
            {
                Id = 5,
                CategoryId = 1,
                AssignmentName = "Study Mathematics",
                ArchivedAt = new DateTime(2026, 11, 20, 10, 0, 0, DateTimeKind.Utc),
            },
            new()
            {
                Id = 6,
                CategoryId = 1,
                AssignmentName = "Practice Linux",
                ArchivedAt = new DateTime(2026, 11, 21, 10, 0, 0, DateTimeKind.Utc),
            }
        };

        var mappedResponses = new List<AssignmentResponse>
        {
            new()
            {
                Id = 5,
                CategoryId = 1,
                AssignmentName = "Study Mathematics",
                ArchivedAt = new DateTime(2026, 11, 20, 10, 0, 0, DateTimeKind.Utc),
            },
            new()
            {
                Id = 6,
                CategoryId = 1,
                AssignmentName = "Practice Linux",
                ArchivedAt = new DateTime(2026, 11, 21, 10, 0, 0, DateTimeKind.Utc),
            }
        };

        _currentUserMock
            .Setup(currentUser => currentUser.UserId)
            .Returns(42);

        _assignmentRepositoryMock
            .Setup(repository => repository.GetArchivedByUserId(42))
            .ReturnsAsync(archivedAssignments);

        _mapperMock
            .Setup(mapper => mapper.Map<IEnumerable<AssignmentResponse>>(archivedAssignments))
            .Returns(mappedResponses);

        var result = await _assignmentService.GetArchivedByUserId();

        Assert.Same(mappedResponses, result);
    }

    [Fact]
    public async Task Restore_OwnedArchivedAssignment_RestoresAssignment()
    {
        var assignment = new Assignment
        {
            Id = 5,
            CategoryId = 1,
            AssignmentName = "Study Mathematics",
            ArchivedAt = new DateTime(2026, 11, 20, 10, 0, 0, DateTimeKind.Utc),
            Category = new Category
            {
                Id = 1,
                UserId = 42,
                CategoryName = "Personal",
            },
        };

        var mappedResponse = new AssignmentResponse
        {
            Id = 5,
            CategoryId = 1,
            AssignmentName = "Study Mathematics",
            ArchivedAt = null,
        };

        _currentUserMock
            .Setup(currentUser => currentUser.UserId)
            .Returns(42);

        _assignmentRepositoryMock
            .Setup(repository => repository.GetByIdWithCategory(5))
            .ReturnsAsync(assignment);

        _mapperMock
            .Setup(mapper => mapper.Map<AssignmentResponse>(assignment))
            .Returns(mappedResponse);

        var result = await _assignmentService.Restore(5);

        Assert.Same(mappedResponse, result);
        Assert.Null(assignment.ArchivedAt);

        _assignmentRepositoryMock.Verify(
            repository => repository.Update(assignment),
            Times.Once);
    }

    [Fact]
    public async Task Restore_AssignmentDoesNotExist_ThrowsKeyNotFoundException()
    {
        _assignmentRepositoryMock
            .Setup(repository => repository.GetByIdWithCategory(5))
            .ReturnsAsync((Assignment?)null);

        var exception = await Assert.ThrowsAsync<KeyNotFoundException>(
            () => _assignmentService.Restore(5));

        Assert.Equal("Assignment not found", exception.Message);

        _assignmentRepositoryMock.Verify(
            repository => repository.Update(It.IsAny<Assignment>()),
            Times.Never);
    }

    [Fact]
    public async Task Restore_AssignmentBelongsToAnotherUser_ThrowsKeyNotFoundException()
    {
        var originalArchivedAt = new DateTime(2026, 11, 20, 10, 0, 0, DateTimeKind.Utc);

        var assignment = new Assignment
        {
            Id = 5,
            CategoryId = 1,
            AssignmentName = "Study Mathematics",
            ArchivedAt = originalArchivedAt,
            Category = new Category
            {
                Id = 1,
                UserId = 99,
                CategoryName = "Personal",
            },
        };

        _currentUserMock
            .Setup(currentUser => currentUser.UserId)
            .Returns(42);

        _assignmentRepositoryMock
            .Setup(repository => repository.GetByIdWithCategory(5))
            .ReturnsAsync(assignment);

        var exception = await Assert.ThrowsAsync<KeyNotFoundException>(
            () => _assignmentService.Restore(5));

        Assert.Equal("Assignment not found", exception.Message);
        Assert.Equal(originalArchivedAt, assignment.ArchivedAt);

        _assignmentRepositoryMock.Verify(
            repository => repository.Update(It.IsAny<Assignment>()),
            Times.Never);
    }

    [Fact]
    public async Task Restore_AssignmentNotArchived_ThrowsInvalidOperationException()
    {
        var assignment = new Assignment
        {
            Id = 5,
            CategoryId = 1,
            AssignmentName = "Study Mathematics",
            ArchivedAt = null,
            Category = new Category
            {
                Id = 1,
                UserId = 42,
                CategoryName = "Personal",
            },
        };

        _currentUserMock
            .Setup(currentUser => currentUser.UserId)
            .Returns(42);

        _assignmentRepositoryMock
            .Setup(repository => repository.GetByIdWithCategory(5))
            .ReturnsAsync(assignment);

        var exception = await Assert.ThrowsAsync<InvalidOperationException>(
            () => _assignmentService.Restore(5));

        Assert.Equal("Assignment is not archived", exception.Message);

        _assignmentRepositoryMock.Verify(
            repository => repository.Update(It.IsAny<Assignment>()),
            Times.Never);
    }
}