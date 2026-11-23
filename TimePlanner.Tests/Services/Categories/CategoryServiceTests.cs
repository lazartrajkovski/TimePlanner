using MapsterMapper;
using Moq;
using TimePlanner.Domain.Entities;
using TimePlanner.Repositories.Interfaces;
using TimePlanner.Services.DTOs.Categories;
using TimePlanner.Services.Interfaces;
using TimePlanner.Services.Services;

namespace TimePlanner.Tests.Services.Categories;

public class CategoryServiceTests
{
    private readonly Mock<ICategoryRepository> _categoryRepositoryMock;
    private readonly Mock<IAssignmentRepository> _assignmentRepositoryMock;
    private readonly Mock<ICurrentUser> _currentUserMock;
    private readonly Mock<ITimeEntryRepository> _timeEntryRepositoryMock;
    private readonly Mock<IMapper> _mapperMock;
    private readonly CategoryService _categoryService;

    public CategoryServiceTests()
    {
        _categoryRepositoryMock = new Mock<ICategoryRepository>();
        _assignmentRepositoryMock = new Mock<IAssignmentRepository>();
        _currentUserMock = new Mock<ICurrentUser>();
        _timeEntryRepositoryMock = new Mock<ITimeEntryRepository>();
        _mapperMock = new Mock<IMapper>();

        _categoryService = new CategoryService(
            _categoryRepositoryMock.Object,
            _assignmentRepositoryMock.Object,
            _currentUserMock.Object,
            _timeEntryRepositoryMock.Object,
            _mapperMock.Object);
    }

    [Fact]
    public async Task Add_ValidCategoryRequest_CreatesCategoryForCurrentUser()
    {
        var request = new CategoryRequest
        {
            CategoryName = "  Work  ",
        };

        var category = new Category
        {
            CategoryName = "Work",
        };

        var createdCategory = new Category
        {
            Id = 1,
            UserId = 42,
            CategoryName = "Work"
        };

        var mappedResponse = new CategoryResponse
        {
            Id = 1,
            CategoryName = "Work",
        };

        _currentUserMock
            .Setup(currentUser => currentUser.UserId)
            .Returns(42);

        _categoryRepositoryMock
            .Setup(repository => repository.GetByNameAndUserId(
                "Work",
                42))
            .ReturnsAsync((Category?)null);

        _mapperMock
            .Setup(mapper => mapper.Map<Category>(request))
            .Returns(category);

        _categoryRepositoryMock
            .Setup(repository => repository.Add(category))
            .ReturnsAsync(createdCategory);

        _mapperMock
            .Setup(mapper => mapper.Map<CategoryResponse>(createdCategory))
            .Returns(mappedResponse);

        var result = await _categoryService.Add(request);

        Assert.Equal("Work", result.CategoryName);
        Assert.Same(mappedResponse, result);

        _categoryRepositoryMock.Verify(
            repository => repository.GetByNameAndUserId(
                "Work",
                42),
            Times.Once);

        _categoryRepositoryMock.Verify(
            repository => repository.Add(
                It.Is<Category>(addedCategory =>
                    addedCategory.CategoryName == "Work" &&
                    addedCategory.UserId == 42)),
            Times.Once);

        _mapperMock.Verify(
            mapper => mapper.Map<CategoryResponse>(createdCategory),
            Times.Once);
    }

    [Fact]
    public async Task Add_DuplicateCategory_ThrowsInvalidOperationException()
    {
        var request = new CategoryRequest
        {
            CategoryName = "Work",
        };

        var existingCategory = new Category
        {
            Id = 1,
            UserId = 42,
            CategoryName = "Work"
        };

        _currentUserMock
            .Setup(currentUser => currentUser.UserId)
            .Returns(42);

        _categoryRepositoryMock
            .Setup(repository => repository.GetByNameAndUserId(
                "Work",
                42))
            .ReturnsAsync(existingCategory);

        var exception = await Assert.ThrowsAsync<InvalidOperationException>(
            () => _categoryService.Add(request));

        Assert.Equal("Category with this name already exists", exception.Message);

        _mapperMock.Verify(
            mapper => mapper.Map<Category>(It.IsAny<CategoryRequest>()),
            Times.Never);

        _categoryRepositoryMock.Verify(
            repository => repository.Add(It.IsAny<Category>()),
            Times.Never);

        _mapperMock.Verify(
            mapper => mapper.Map<CategoryResponse>(
                It.IsAny<Category>()),
            Times.Never);
    }

    [Fact]
    public async Task Add_InvalidCategoryRequest_ThrowsBeforeAccessingDependencies()
    {
        var request = new CategoryRequest
        {
            CategoryName = "    ",
        };

        var exception = await Assert.ThrowsAsync<ArgumentException>(
            () => _categoryService.Add(request));

        Assert.Equal("Category name is required", exception.Message);

        _categoryRepositoryMock.Verify(
            repository => repository.GetByNameAndUserId(
                It.IsAny<string>(),
                It.IsAny<int>()),
            Times.Never);

        _mapperMock.Verify(
            mapper => mapper.Map<Category>(
                It.IsAny<CategoryRequest>()),
            Times.Never);

        _categoryRepositoryMock.Verify(
            repository => repository.Add(It.IsAny<Category>()),
            Times.Never);
    }

    [Fact]
    public async Task GetAll_CurrentUserHasCategories_ReturnsMappedCategories()
    {
        var categories = new List<Category>
        {
            new()
            {
                Id = 1,
                UserId = 42,
                CategoryName = "Work",
            },
            new()
            {
                Id = 2,
                UserId = 42,
                CategoryName = "Personal",
            }
        };

        var mappedResponses = new List<CategoryResponse>
        {
            new()
            {
                Id = 1,
                CategoryName = "Work",
            },
            new()
            {
                Id = 2,
                CategoryName = "Personal",
            }
        };

        _currentUserMock
            .Setup(currentUser => currentUser.UserId)
            .Returns(42);

        _categoryRepositoryMock
            .Setup(repository => repository.GetByUserId(42))
            .ReturnsAsync(categories);

        _mapperMock
            .Setup(mapper => mapper.Map<IEnumerable<CategoryResponse>>(categories))
            .Returns(mappedResponses);

        var result = await _categoryService.GetAll();

        Assert.Same(mappedResponses, result);

        _categoryRepositoryMock.Verify(
            repository => repository.GetByUserId(42),
            Times.Once);

        _mapperMock.Verify(
            mapper => mapper.Map<IEnumerable<CategoryResponse>>(categories),
            Times.Once);
    }

    [Fact]
    public async Task GetById_OwnedCategory_ReturnsMappedCategory()
    {
        var category = new Category
        {
            Id = 1,
            UserId = 42,
            CategoryName = "Work",
        };

        var mappedResponse = new CategoryResponse
        {
            Id = 1,
            CategoryName = "Work",
        };

        _currentUserMock
            .Setup(currentUser => currentUser.UserId)
            .Returns(42);

        _categoryRepositoryMock
            .Setup(repository => repository.GetById(1))
            .ReturnsAsync(category);

        _mapperMock
            .Setup(mapper => mapper.Map<CategoryResponse>(category))
            .Returns(mappedResponse);

        var result = await _categoryService.GetById(1);

        Assert.Same(mappedResponse, result);

        _categoryRepositoryMock.Verify(
            repository => repository.GetById(1),
            Times.Once);

        _mapperMock.Verify(
            mapper => mapper.Map<CategoryResponse>(category),
            Times.Once);
    }

    [Fact]
    public async Task GetById_CategoryDoesNotExist_ThrowsKeyNotFoundException()
    {
        _currentUserMock
            .Setup(currentUser => currentUser.UserId)
            .Returns(42);

        _categoryRepositoryMock
            .Setup(repository => repository.GetById(1))
            .ReturnsAsync((Category?)null);

        var exception = await Assert.ThrowsAsync<KeyNotFoundException>(
            () => _categoryService.GetById(1));

        Assert.Equal("Category not found", exception.Message);

        _categoryRepositoryMock.Verify(
            repository => repository.GetById(1),
            Times.Once);

        _mapperMock.Verify(
            mapper => mapper.Map<CategoryResponse>(It.IsAny<Category>()),
            Times.Never);
    }

    [Fact]
    public async Task GetById_CategoryBelongsToAnotherUser_ThrowsKeyNotFoundException()
    {
        var category = new Category
        {
            Id = 1,
            UserId = 99,
            CategoryName = "Work",
        };

        _currentUserMock
            .Setup(currentUser => currentUser.UserId)
            .Returns(42);

        _categoryRepositoryMock
            .Setup(repository => repository.GetById(1))
            .ReturnsAsync(category);

        var exception = await Assert.ThrowsAsync<KeyNotFoundException>(
            () => _categoryService.GetById(1));

        Assert.Equal("Category not found", exception.Message);

        _categoryRepositoryMock.Verify(
            repository => repository.GetById(1),
            Times.Once);

        _mapperMock.Verify(
            mapper => mapper.Map<CategoryResponse>(It.IsAny<Category>()),
            Times.Never);
    }

    [Fact]
    public async Task Update_ValidCategoryRequest_UpdatesCategoryForCurrentUser()
    {
        var request = new CategoryRequest
        {
            CategoryName = "  Personal  ",
        };

        var category = new Category
        {
            Id = 1,
            UserId = 42,
            CategoryName = "Work",
        };

        var updatedCategory = new Category
        {
            Id = 1,
            UserId = 42,
            CategoryName = "Personal",
        };

        var mappedResponse = new CategoryResponse
        {
            Id = 1,
            CategoryName = "Personal",
        };

        _currentUserMock
            .Setup(currentUser => currentUser.UserId)
            .Returns(42);

        _categoryRepositoryMock
            .Setup(repository => repository.GetById(1))
            .ReturnsAsync(category);

        _categoryRepositoryMock
            .Setup(repository => repository.GetByNameAndUserId(
                "Personal",
                42))
            .ReturnsAsync((Category?)null);

        _categoryRepositoryMock
            .Setup(repository => repository.Update(category))
            .ReturnsAsync(updatedCategory);

        _mapperMock
            .Setup(mapper => mapper.Map<CategoryResponse>(updatedCategory))
            .Returns(mappedResponse);

        var result = await _categoryService.Update(1, request);

        Assert.Same(mappedResponse, result);
        Assert.Equal("Personal", category.CategoryName);

        _categoryRepositoryMock.Verify(
            repository => repository.GetByNameAndUserId(
                "Personal",
                42),
            Times.Once);

        _categoryRepositoryMock.Verify(
            repository => repository.Update(
                It.Is<Category>(updated =>
                    updated.Id == 1 &&
                    updated.UserId == 42 &&
                    updated.CategoryName == "Personal")),
            Times.Once);

        _mapperMock.Verify(
            mapper => mapper.Map<CategoryResponse>(updatedCategory),
            Times.Once);
    }

    [Fact]
    public async Task Update_InvalidCategoryRequest_ThrowsBeforeAccessingDependencies()
    {
        var request = new CategoryRequest
        {
            CategoryName = "   ",
        };

        var exception = await Assert.ThrowsAsync<ArgumentException>(
            () => _categoryService.Update(1, request));

        Assert.Equal("Category name is required", exception.Message);

        _categoryRepositoryMock.Verify(
            repository => repository.GetByNameAndUserId(
                It.IsAny<string>(),
                It.IsAny<int>()),
            Times.Never);

        _categoryRepositoryMock.Verify(
            repository => repository.Update(It.IsAny<Category>()),
            Times.Never);

        _mapperMock.Verify(
            mapper => mapper.Map<CategoryResponse>(
                It.IsAny<Category>()),
            Times.Never);
    }

    [Fact]
    public async Task Update_CategoryDoesNotExist_ThrowsKeyNotFoundException()
    {
        var request = new CategoryRequest
        {
            CategoryName = "Personal",
        };

        _categoryRepositoryMock
            .Setup(repository => repository.GetById(1))
            .ReturnsAsync((Category?)null);

        var exception = await Assert.ThrowsAsync<KeyNotFoundException>(
            () => _categoryService.Update(1, request));

        Assert.Equal("Category not found", exception.Message);

        _categoryRepositoryMock.Verify(
            repository => repository.GetById(1),
            Times.Once);

        _categoryRepositoryMock.Verify(
            repository => repository.Update(It.IsAny<Category>()),
            Times.Never);

        _mapperMock.Verify(
            mapper => mapper.Map<CategoryResponse>(
                It.IsAny<Category>()),
            Times.Never);
    }

    [Fact]
    public async Task Update_CategoryBelongsToAnotherUser_ThrowsKeyNotFoundException()
    {
        var request = new CategoryRequest
        {
            CategoryName = "Personal",
        };

        var category = new Category
        {
            Id = 1,
            UserId = 99,
            CategoryName = "Work",
        };

        _currentUserMock
            .Setup(currentUser => currentUser.UserId)
            .Returns(42);

        _categoryRepositoryMock
            .Setup(repository => repository.GetById(1))
            .ReturnsAsync(category);

        var exception = await Assert.ThrowsAsync<KeyNotFoundException>(
            () => _categoryService.Update(1, request));

        Assert.Equal("Category not found", exception.Message);

        _categoryRepositoryMock.Verify(
            repository => repository.GetById(1),
            Times.Once);

        _categoryRepositoryMock.Verify(
            repository => repository.Update(It.IsAny<Category>()),
            Times.Never);
    }

    [Fact]
    public async Task Update_DuplicateCategory_ThrowsInvalidOperationException()
    {
        var request = new CategoryRequest
        {
            CategoryName = "Personal",
        };

        var category = new Category
        {
            Id = 1,
            UserId = 42,
            CategoryName = "Work",
        };

        var existingCategory = new Category
        {
            Id = 2,
            UserId = 42,
            CategoryName = "Personal",
        };

        _currentUserMock
            .Setup(currentUser => currentUser.UserId)
            .Returns(42);

        _categoryRepositoryMock
            .Setup(repository => repository.GetById(1))
            .ReturnsAsync(category);

        _categoryRepositoryMock
            .Setup(repository => repository.GetByNameAndUserId(
                "Personal",
                42))
            .ReturnsAsync(existingCategory);

        var exception = await Assert.ThrowsAsync<InvalidOperationException>(
            () => _categoryService.Update(1, request));

        Assert.Equal("Category with this name already exists", exception.Message);
        Assert.Equal("Work", category.CategoryName);

        _categoryRepositoryMock.Verify(
            repository => repository.GetByNameAndUserId(
                "Personal",
                42),
            Times.Once);

        _categoryRepositoryMock.Verify(
            repository => repository.Update(It.IsAny<Category>()),
            Times.Never);

        _mapperMock.Verify(
            mapper => mapper.Map<CategoryResponse>(
                It.IsAny<Category>()),
            Times.Never);
    }

    [Fact]
    public async Task Update_SameCategoryName_UpdatesCategorySuccessfully()
    {
        var request = new CategoryRequest
        {
            CategoryName = "  Work  ",
        };

        var category = new Category
        {
            Id = 1,
            UserId = 42,
            CategoryName = "Work",
        };

        var mappedResponse = new CategoryResponse
        {
            Id = 1,
            CategoryName = "Work",
        };

        _currentUserMock
            .Setup(currentUser => currentUser.UserId)
            .Returns(42);

        _categoryRepositoryMock
            .Setup(repository => repository.GetById(1))
            .ReturnsAsync(category);

        _categoryRepositoryMock
            .Setup(repository => repository.GetByNameAndUserId(
                "Work", 42))
            .ReturnsAsync(category);

        _categoryRepositoryMock
            .Setup(repository => repository.Update(category))
            .ReturnsAsync(category);

        _mapperMock
            .Setup(mapper => mapper.Map<CategoryResponse>(category))
            .Returns(mappedResponse);

        var result = await _categoryService.Update(1, request);

        Assert.Same(mappedResponse, result);

        _categoryRepositoryMock.Verify(
            repository => repository.GetByNameAndUserId(
                "Work",
                42),
            Times.Once);

        _categoryRepositoryMock.Verify(
            repository => repository.Update(category),
            Times.Once);

        _mapperMock.Verify(
            mapper => mapper.Map<CategoryResponse>(category),
            Times.Once);
    }

    [Fact]
    public async Task Delete_OwnedCategoryWithoutActiveTimeEntry_DeletesCategory()
    {
        var category = new Category
        {
            Id = 1,
            UserId = 42,
            CategoryName = "Work",
        };

        _currentUserMock
            .Setup(currentUser => currentUser.UserId)
            .Returns(42);

        _categoryRepositoryMock
            .Setup(repository => repository.GetById(1))
            .ReturnsAsync(category);

        _timeEntryRepositoryMock
            .Setup(repository => repository.GetRunningByUserId(42))
            .ReturnsAsync((TimeEntry?)null);

        await _categoryService.Delete(1);

        _categoryRepositoryMock.Verify(
            repository => repository.GetById(1),
            Times.Once);

        _timeEntryRepositoryMock.Verify(
            repository => repository.GetRunningByUserId(42),
            Times.Once);

        _assignmentRepositoryMock.Verify(
            repository => repository.GetByIdWithCategory(
                It.IsAny<int>()),
        Times.Never);

        _categoryRepositoryMock.Verify(
            repository => repository.Delete(1),
            Times.Once);
    }

    [Fact]
    public async Task Delete_CategoryDoesNotExist_ThrowsKeyNotFoundException()
    {
        _categoryRepositoryMock
            .Setup(repository => repository.GetById(1))
            .ReturnsAsync((Category?)null);

        var exception = await Assert.ThrowsAsync<KeyNotFoundException>(
            () => _categoryService.Delete(1));

        Assert.Equal("Category not found", exception.Message);

        _categoryRepositoryMock.Verify(
            repository => repository.GetById(1),
            Times.Once);

        _timeEntryRepositoryMock.Verify(
            repository => repository.GetRunningByUserId(
                It.IsAny<int>()),
            Times.Never);

        _assignmentRepositoryMock.Verify(
            repository => repository.GetByIdWithCategory(
                It.IsAny<int>()),
            Times.Never);

        _categoryRepositoryMock.Verify(
            repository => repository.Delete(It.IsAny<int>()),
            Times.Never);
    }

    [Fact]
    public async Task Delete_CategoryBelongsToAnotherUser_ThrowsKeyNotFoundException()
    {
        var category = new Category
        {
            Id = 1,
            UserId = 99,
            CategoryName = "Work",
        };

        _currentUserMock
            .Setup(currentUser => currentUser.UserId)
            .Returns(42);

        _categoryRepositoryMock
            .Setup(repository => repository.GetById(1))
            .ReturnsAsync(category);

        var exception = await Assert.ThrowsAsync<KeyNotFoundException>(
            () => _categoryService.Delete(1));

        Assert.Equal("Category not found", exception.Message);

        _categoryRepositoryMock.Verify(
            repository => repository.GetById(1),
            Times.Once);

        _timeEntryRepositoryMock.Verify(
            repository => repository.GetRunningByUserId(
                It.IsAny<int>()),
            Times.Never);

        _assignmentRepositoryMock.Verify(
            repository => repository.GetByIdWithCategory(
                It.IsAny<int>()),
            Times.Never);

        _categoryRepositoryMock.Verify(
            repository => repository.Delete(It.IsAny<int>()),
            Times.Never);
    }

    [Fact]
    public async Task Delete_CategoryWithActiveTimeEntry_ThrowsInvalidOperationException()
    {
        var category = new Category
        {
            Id = 1,
            UserId = 42,
            CategoryName = "Work",
        };

        var activeTimeEntry = new TimeEntry
        {
            Id = 10,
            UserId = 42,
            AssignmentId = 5,
            StartedAt = new DateTime(2026, 11, 23, 0, 0, 0, DateTimeKind.Utc),
            EndedAt = null,
        };

        var activeAssignment = new Assignment
        {
            Id = 5,
            CategoryId = 1,
            AssignmentName = "Write report",
            Category = category,
        };

        _currentUserMock
            .Setup(currentUser => currentUser.UserId)
            .Returns(42);

        _categoryRepositoryMock
            .Setup(repository => repository.GetById(1))
            .ReturnsAsync(category);

        _timeEntryRepositoryMock
            .Setup(repository => repository.GetRunningByUserId(42))
            .ReturnsAsync(activeTimeEntry);

        _assignmentRepositoryMock
            .Setup(repository => repository.GetByIdWithCategory(5))
            .ReturnsAsync(activeAssignment);

        var exception = await Assert.ThrowsAsync<InvalidOperationException>(
            () => _categoryService.Delete(1));

        Assert.Equal("Cannot delete a category that contains an assignment with an active time entry", exception.Message);

        _categoryRepositoryMock.Verify(
            repository => repository.Delete(It.IsAny<int>()),
                Times.Never);
    }

    [Fact]
    public async Task Delete_ActiveTimeEntryInAnotherCategory_DeletesCategory()
    {
        var category = new Category
        {
            Id = 1,
            UserId = 42,
            CategoryName = "Work",
        };

        var otherCategory = new Category
        {
            Id = 2,
            UserId = 42,
            CategoryName = "Personal",
        };

        var activeTimeEntry = new TimeEntry
        {

            Id = 10,
            UserId = 42,
            AssignmentId = 5,
            StartedAt = new DateTime(2026, 11, 23, 9, 0, 0, DateTimeKind.Utc),
            EndedAt = null,
        };

        var activeAssignment = new Assignment
        {
            Id = 5,
            CategoryId = 2,
            AssignmentName = "Write report",
            Category = otherCategory,
        };

        _currentUserMock
            .Setup(currentUser => currentUser.UserId)
            .Returns(42);

        _categoryRepositoryMock
            .Setup(repository => repository.GetById(1))
            .ReturnsAsync(category);

        _timeEntryRepositoryMock
            .Setup(repository => repository.GetRunningByUserId(42))
            .ReturnsAsync(activeTimeEntry);

        _assignmentRepositoryMock
            .Setup(repository => repository.GetByIdWithCategory(5))
            .ReturnsAsync(activeAssignment);

        await _categoryService.Delete(1);

        _categoryRepositoryMock.Verify(
            repository => repository.Delete(1),
            Times.Once);
    }
}