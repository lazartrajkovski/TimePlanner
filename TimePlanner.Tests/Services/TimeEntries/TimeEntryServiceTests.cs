using MapsterMapper;
using Moq;
using TimePlanner.Domain.Entities;
using TimePlanner.Domain.Enums;
using TimePlanner.Repositories.Interfaces;
using TimePlanner.Services.DTOs.TimeEntries;
using TimePlanner.Services.Interfaces;
using TimePlanner.Services.Services;

namespace TimePlanner.Tests.Services.TimeEntries;

public class TimeEntryServiceTests
{
    private readonly Mock<ITimeEntryRepository> _timeEntryRepositoryMock;
    private readonly Mock<IAssignmentRepository> _assignmentRepositoryMock;
    private readonly Mock<ICurrentUser> _currentUserMock;
    private readonly Mock<IMapper> _mapperMock;
    private readonly TimeEntryService _timeEntryService;

    public TimeEntryServiceTests()
    {
        _timeEntryRepositoryMock = new Mock<ITimeEntryRepository>();
        _assignmentRepositoryMock = new Mock<IAssignmentRepository>();
        _currentUserMock = new Mock<ICurrentUser>();
        _mapperMock = new Mock<IMapper>();

        _timeEntryService = new TimeEntryService(
            _timeEntryRepositoryMock.Object,
            _assignmentRepositoryMock.Object,
            _currentUserMock.Object,
            _mapperMock.Object);
    }

    [Fact]
    public async Task Start_ValidRequest_StartsTimeEntryAndMovesAssignmentToInProgress()
    {
        var request = new StartTimeEntryRequest
        {
            AssignmentId = 5,
        };

        var assignment = new Assignment
        {
            Id = 5,
            CategoryId = 1,
            AssignmentName = "Study Mathematics",
            Status = AssignmentStatus.ToDo,
            Category = new Category
            {
                Id = 1,
                UserId = 42,
                CategoryName = "Personal",
            },
        };

        var createdTimeEntry = new TimeEntry
        {
            Id = 10,
            AssignmentId = 5,
            UserId = 42,
            StartedAt = new DateTime(2026, 11, 24, 9, 0, 0, DateTimeKind.Utc),
        };

        var mappedResponse = new TimeEntryResponse
        {
            Id = 10,
            AssignmentId = 5,
            StartedAt = new DateTime(2026, 11, 24, 9, 0, 0, DateTimeKind.Utc),
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

        _timeEntryRepositoryMock
            .Setup(repository => repository.Add(It.IsAny<TimeEntry>()))
            .ReturnsAsync(createdTimeEntry);

        _mapperMock
            .Setup(mapper => mapper.Map<TimeEntryResponse>(createdTimeEntry))
            .Returns(mappedResponse);

        var before = DateTime.UtcNow;

        var result = await _timeEntryService.Start(request);

        var after = DateTime.UtcNow;

        Assert.Same(mappedResponse, result);
        Assert.Equal(AssignmentStatus.InProgress, assignment.Status);

        _timeEntryRepositoryMock.Verify(
            repository => repository.Add(
                It.Is<TimeEntry>(timeEntry =>
                    timeEntry.AssignmentId == 5 &&
                    timeEntry.UserId == 42 &&
                    timeEntry.StartedAt >= before &&
                    timeEntry.StartedAt <= after &&
                    timeEntry.EndedAt == null)),
            Times.Once);
    }

    [Fact]
    public async Task Start_InvalidAssignmentId_ThrowsArgumentException()
    {
        var request = new StartTimeEntryRequest
        {
            AssignmentId = 0,
        };

        var exception = await Assert.ThrowsAsync<ArgumentException>(
            () => _timeEntryService.Start(request));

        Assert.Equal("AssignmentId must be greater than zero", exception.Message);

        _timeEntryRepositoryMock.Verify(
            repository => repository.Add(It.IsAny<TimeEntry>()),
            Times.Never);
    }

    [Fact]
    public async Task Start_AssignmentDoesNotExist_ThrowsKeyNotFoundException()
    {
        var request = new StartTimeEntryRequest
        {
            AssignmentId = 5,
        };

        _assignmentRepositoryMock
            .Setup(repository => repository.GetByIdWithCategory(5))
            .ReturnsAsync((Assignment?)null);

        var exception = await Assert.ThrowsAsync<KeyNotFoundException>(
            () => _timeEntryService.Start(request));

        Assert.Equal("Assignment not found", exception.Message);

        _timeEntryRepositoryMock.Verify(
            repository => repository.Add(It.IsAny<TimeEntry>()),
            Times.Never);
    }

    [Fact]
    public async Task Start_AssignmentBelongsToAnotherUser_ThrowsKeyNotFoundException()
    {
        var request = new StartTimeEntryRequest
        {
            AssignmentId = 5,
        };

        var assignment = new Assignment
        {
            Id = 5,
            CategoryId = 1,
            AssignmentName = "Study Mathematics",
            Status = AssignmentStatus.ToDo,
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
            () => _timeEntryService.Start(request));

        Assert.Equal("Assignment not found", exception.Message);
        Assert.Equal(AssignmentStatus.ToDo, assignment.Status);

        _timeEntryRepositoryMock.Verify(
            repository => repository.Add(It.IsAny<TimeEntry>()),
            Times.Never);
    }

    [Fact]
    public async Task Start_ArchivedAssignment_ThrowsInvalidOperationException()
    {
        var request = new StartTimeEntryRequest
        {
            AssignmentId = 5,
        };

        var assignment = new Assignment
        {
            Id = 5,
            CategoryId = 1,
            AssignmentName = "Study Mathematics",
            Status = AssignmentStatus.ToDo,
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
            () => _timeEntryService.Start(request));

        Assert.Equal("Assignment is archived", exception.Message);
        Assert.Equal(AssignmentStatus.ToDo, assignment.Status);

        _timeEntryRepositoryMock.Verify(
            repository => repository.Add(It.IsAny<TimeEntry>()),
            Times.Never);
    }

    [Fact]
    public async Task Start_DoneAssignment_ThrowsInvalidOperationException()
    {
        var request = new StartTimeEntryRequest
        {
            AssignmentId = 5,
        };

        var assignment = new Assignment
        {
            Id = 5,
            CategoryId = 1,
            AssignmentName = "Study Mathematics",
            Status = AssignmentStatus.Done,
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
            () => _timeEntryService.Start(request));

        Assert.Equal("Assignment is already done", exception.Message);

        _timeEntryRepositoryMock.Verify(
            repository => repository.Add(It.IsAny<TimeEntry>()),
            Times.Never);
    }

    [Fact]
    public async Task Start_TimeEntryAlreadyRunning_ThrowsInvalidOperationException()
    {
        var request = new StartTimeEntryRequest
        {
            AssignmentId = 5,
        };

        var assignment = new Assignment
        {
            Id = 5,
            CategoryId = 1,
            AssignmentName = "Study Mathematics",
            Status = AssignmentStatus.ToDo,
            Category = new Category
            {
                Id = 1,
                UserId = 42,
                CategoryName = "Personal",
            },
        };

        var runningTimeEntry = new TimeEntry
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
            .ReturnsAsync(runningTimeEntry);

        var exception = await Assert.ThrowsAsync<InvalidOperationException>(
            () => _timeEntryService.Start(request));

        Assert.Equal("A time entry is already running", exception.Message);
        Assert.Equal(AssignmentStatus.ToDo, assignment.Status);

        _timeEntryRepositoryMock.Verify(
            repository => repository.Add(It.IsAny<TimeEntry>()),
            Times.Never);
    }

    [Fact]
    public async Task Stop_TimeEntryRunning_StopsTimeEntry()
    {
        var runningTimeEntry = new TimeEntry
        {
            Id = 10,
            UserId = 42,
            AssignmentId = 5,
            StartedAt = new DateTime(2026, 11, 24, 9, 0, 0, DateTimeKind.Utc),
            EndedAt = null,
        };

        var updatedTimeEntry = new TimeEntry
        {
            Id = 10,
            UserId = 42,
            AssignmentId = 5,
            StartedAt = new DateTime(2026, 11, 24, 9, 0, 0, DateTimeKind.Utc),
            EndedAt = new DateTime(2026, 11, 24, 10, 0, 0, DateTimeKind.Utc),
        };

        var mappedResponse = new TimeEntryResponse
        {
            Id = 10,
            AssignmentId = 5,
            StartedAt = new DateTime(2026, 11, 24, 9, 0, 0, DateTimeKind.Utc),
            EndedAt = new DateTime(2026, 11, 24, 10, 0, 0, DateTimeKind.Utc),
        };

        _currentUserMock
            .Setup(currentUser => currentUser.UserId)
            .Returns(42);

        _timeEntryRepositoryMock
            .Setup(repository => repository.GetRunningByUserId(42))
            .ReturnsAsync(runningTimeEntry);

        _timeEntryRepositoryMock
            .Setup(repository => repository.Update(runningTimeEntry))
            .ReturnsAsync(updatedTimeEntry);

        _mapperMock
            .Setup(mapper => mapper.Map<TimeEntryResponse>(updatedTimeEntry))
            .Returns(mappedResponse);

        var before = DateTime.UtcNow;

        var result = await _timeEntryService.Stop();

        var after = DateTime.UtcNow;

        Assert.Same(mappedResponse, result);
        Assert.NotNull(runningTimeEntry.EndedAt);
        Assert.InRange(runningTimeEntry.EndedAt.Value, before, after);

        _timeEntryRepositoryMock.Verify(
            repository => repository.Update(runningTimeEntry),
            Times.Once);
    }

    [Fact]
    public async Task Stop_NoTimeEntryRunning_ThrowsInvalidOperationException()
    {
        _currentUserMock
            .Setup(currentUser => currentUser.UserId)
            .Returns(42);

        _timeEntryRepositoryMock
            .Setup(repository => repository.GetRunningByUserId(42))
            .ReturnsAsync((TimeEntry?)null);

        var exception = await Assert.ThrowsAsync<InvalidOperationException>(
            () => _timeEntryService.Stop());

        Assert.Equal("No time entry is currently running", exception.Message);

        _timeEntryRepositoryMock.Verify(
            repository => repository.Update(It.IsAny<TimeEntry>()),
            Times.Never);
    }

    [Fact]
    public async Task GetActiveTimeEntry_TimeEntryRunning_ReturnsMappedTimeEntry()
    {
        var runningTimeEntry = new TimeEntry
        {
            Id = 10,
            UserId = 42,
            AssignmentId = 5,
            StartedAt = new DateTime(2026, 11, 24, 9, 0, 0, DateTimeKind.Utc),
            EndedAt = null,
        };

        var mappedResponse = new TimeEntryResponse
        {
            Id = 10,
            AssignmentId = 5,
            StartedAt = new DateTime(2026, 11, 24, 9, 0, 0, DateTimeKind.Utc),
            EndedAt = null,
        };

        _currentUserMock
            .Setup(currentUser => currentUser.UserId)
            .Returns(42);

        _timeEntryRepositoryMock
            .Setup(repository => repository.GetRunningByUserId(42))
            .ReturnsAsync(runningTimeEntry);

        _mapperMock
            .Setup(mapper => mapper.Map<TimeEntryResponse>(runningTimeEntry))
            .Returns(mappedResponse);

        var result = await _timeEntryService.GetActiveTimeEntry();

        Assert.Same(mappedResponse, result);
    }

    [Fact]
    public async Task GetActiveTimeEntry_NoTimeEntryRunning_ReturnsNull()
    {
        _currentUserMock
            .Setup(currentUser => currentUser.UserId)
            .Returns(42);

        _timeEntryRepositoryMock
            .Setup(repository => repository.GetRunningByUserId(42))
            .ReturnsAsync((TimeEntry?)null);

        var result = await _timeEntryService.GetActiveTimeEntry();

        Assert.Null(result);
    }

    [Fact]
    public async Task GetByAssignmentId_OwnedAssignment_ReturnsMappedTimeEntries()
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

        var timeEntries = new List<TimeEntry>
        {
            new()
            {
                Id = 10,
                UserId = 42,
                AssignmentId = 5,
                StartedAt = new DateTime(2026, 11, 23, 9, 0, 0, DateTimeKind.Utc),
                EndedAt = new DateTime(2026, 11, 23, 10, 0, 0, DateTimeKind.Utc),
            },
            new()
            {
                Id = 11,
                UserId = 42,
                AssignmentId = 5,
                StartedAt = new DateTime(2026, 11, 24, 9, 0, 0, DateTimeKind.Utc),
                EndedAt = new DateTime(2026, 11, 24, 9, 30, 0, DateTimeKind.Utc),
            }
        };

        var mappedResponses = new List<TimeEntryResponse>
        {
            new()
            {
                Id = 10,
                AssignmentId = 5,
                StartedAt = new DateTime(2026, 11, 23, 9, 0, 0, DateTimeKind.Utc),
                EndedAt = new DateTime(2026, 11, 23, 10, 0, 0, DateTimeKind.Utc),
            },
            new()
            {
                Id = 11,
                AssignmentId = 5,
                StartedAt = new DateTime(2026, 11, 24, 9, 0, 0, DateTimeKind.Utc),
                EndedAt = new DateTime(2026, 11, 24, 9, 30, 0, DateTimeKind.Utc),
            }
        };

        _currentUserMock
            .Setup(currentUser => currentUser.UserId)
            .Returns(42);

        _assignmentRepositoryMock
            .Setup(repository => repository.GetByIdWithCategory(5))
            .ReturnsAsync(assignment);

        _timeEntryRepositoryMock
            .Setup(repository => repository.GetByAssignmentId(5))
            .ReturnsAsync(timeEntries);

        _mapperMock
            .Setup(mapper => mapper.Map<IEnumerable<TimeEntryResponse>>(timeEntries))
            .Returns(mappedResponses);

        var result = await _timeEntryService.GetByAssignmentId(5);

        Assert.Same(mappedResponses, result);
    }

    [Fact]
    public async Task GetByAssignmentId_InvalidAssignmentId_ThrowsArgumentException()
    {
        var exception = await Assert.ThrowsAsync<ArgumentException>(
            () => _timeEntryService.GetByAssignmentId(0));

        Assert.Equal("AssignmentId must be greater than zero", exception.Message);
    }

    [Fact]
    public async Task GetByAssignmentId_AssignmentDoesNotExist_ThrowsKeyNotFoundException()
    {
        _assignmentRepositoryMock
            .Setup(repository => repository.GetByIdWithCategory(5))
            .ReturnsAsync((Assignment?)null);

        var exception = await Assert.ThrowsAsync<KeyNotFoundException>(
            () => _timeEntryService.GetByAssignmentId(5));

        Assert.Equal("Assignment not found", exception.Message);
    }

    [Fact]
    public async Task GetByAssignmentId_AssignmentBelongsToAnotherUser_ThrowsKeyNotFoundException()
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
            () => _timeEntryService.GetByAssignmentId(5));

        Assert.Equal("Assignment not found", exception.Message);
    }
}