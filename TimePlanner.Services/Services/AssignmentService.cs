using MapsterMapper;
using TimePlanner.Domain.Entities;
using TimePlanner.Domain.Enums;
using TimePlanner.Repositories.Interfaces;
using TimePlanner.Services.DTOs;
using TimePlanner.Services.Interfaces;

namespace TimePlanner.Services.Services;

public class AssignmentService : IAssignmentService
{
    private readonly IAssignmentRepository _assignmentRepository;
    private readonly ICategoryRepository _categoryRepository;
    private readonly ICurrentUser _currentUser;
    private readonly ITimeEntryRepository _timeEntryRepository;
    private readonly IMapper _mapper;

    public AssignmentService(IAssignmentRepository assignmentRepository, ICategoryRepository categoryRepository,
        ICurrentUser currentUser, ITimeEntryRepository timeEntryRepository, IMapper mapper)
    {
        _assignmentRepository = assignmentRepository;
        _categoryRepository = categoryRepository;
        _currentUser = currentUser;
        _timeEntryRepository = timeEntryRepository;
        _mapper = mapper;
    }

    public async Task<AssignmentResponse> Add(AssignmentRequest request)
    {
        if (request.CategoryId <= 0)
        {
            throw new ArgumentException("Category id must be greater than zero");
        }

        if (string.IsNullOrWhiteSpace(request.AssignmentName))
        {
            throw new ArgumentException("Assignment name is required");
        }

        var assignmentName = request.AssignmentName.Trim();

        if (assignmentName.Length > 50)
        {
            throw new ArgumentException("Assignment name must be 50 characters or fewer");
        }

        var description = string.IsNullOrWhiteSpace(request.Description)
            ? null
            : request.Description.Trim();

        if (description?.Length > 256)
        {
            throw new ArgumentException("Assignment description must be 256 characters or fewer");
        }

        var category = await _categoryRepository.GetById(request.CategoryId);

        if (category is null || category.UserId != _currentUser.UserId)
        {
            throw new KeyNotFoundException("Category not found");
        }

        var assignment = _mapper.Map<Assignment>(request);

        assignment.AssignmentName = assignmentName;
        assignment.Description = description;

        var createdAssignment = await _assignmentRepository.Add(assignment);

        return _mapper.Map<AssignmentResponse>(createdAssignment);
    }

    public async Task<IEnumerable<AssignmentResponse>> GetAll(AssignmentStatus? status)
    {
        if (status.HasValue && !Enum.IsDefined(status.Value))
        {
            throw new ArgumentException("Invalid assignment status");
        }
        
        var assignments = await _assignmentRepository.GetByUserId(_currentUser.UserId, status);
        
        return _mapper.Map<IEnumerable<AssignmentResponse>>(assignments);
    }

    public async Task<AssignmentResponse> GetById(int id)
    {
        var assignment = await _assignmentRepository.GetByIdWithCategory(id);

        if (assignment is null || assignment.Category.UserId != _currentUser.UserId)
        {
            throw new KeyNotFoundException("Assignment not found");
        }

        return _mapper.Map<AssignmentResponse>(assignment);
    }

    public async Task<AssignmentResponse> Update(int id, AssignmentRequest request)
    {
        if (request.CategoryId <= 0)
        {
            throw new ArgumentException("Category id must be greater than zero");
        }

        if (string.IsNullOrWhiteSpace(request.AssignmentName))
        {
            throw new ArgumentException("Assignment name is required");
        }

        var assignmentName = request.AssignmentName.Trim();

        if (assignmentName.Length > 50)
        {
            throw new ArgumentException("Assignment name must be 50 characters or fewer");
        }

        var description = string.IsNullOrWhiteSpace(request.Description)
            ? null
            : request.Description.Trim();

        if (description?.Length > 256)
        {
            throw new ArgumentException("Assignment description must be 256 characters or fewer");
        }

        var assignment = await _assignmentRepository.GetByIdWithCategory(id);

        if (assignment is null || assignment.Category.UserId != _currentUser.UserId)
        {
            throw new KeyNotFoundException("Assignment not found");
        }

        var category = await _categoryRepository.GetById(request.CategoryId);

        if (category is null || category.UserId != _currentUser.UserId)
        {
            throw new KeyNotFoundException("Category not found");
        }

        _mapper.Map(request, assignment);

        assignment.AssignmentName = assignmentName;
        assignment.Description = description;

        var updatedAssignment = await _assignmentRepository.Update(assignment);

        return _mapper.Map<AssignmentResponse>(updatedAssignment);
    }

    public async Task Delete(int id)
    {
        var assignment = await _assignmentRepository.GetByIdWithCategory(id);

        if (assignment is null || assignment.Category.UserId != _currentUser.UserId)
        {
            throw new KeyNotFoundException("Assignment not found");
        }

        await _assignmentRepository.Delete(id);
    }

    public async Task<AssignmentResponse> ChangeStatus(int id, AssignmentStatus status)
    {
        if (!Enum.IsDefined(status))
        {
            throw new ArgumentException("Invalid assignment status");
        }

        var assignment = await _assignmentRepository.GetByIdWithCategory(id);

        if (assignment is null || assignment.Category.UserId != _currentUser.UserId)
        {
            throw new KeyNotFoundException("Assignment not found");
        }

        if (status == AssignmentStatus.Done)
        {
            var completedAt = DateTime.UtcNow;

            var activeTimeEntry = await _timeEntryRepository.GetRunningByUserId(_currentUser.UserId);

            if (activeTimeEntry is not null && activeTimeEntry.AssignmentId == assignment.Id)
            {
                activeTimeEntry.EndedAt = completedAt;

                await _timeEntryRepository.Update(activeTimeEntry);
            }

            assignment.CompletedAt ??= completedAt;
        }
        else
        {
            assignment.CompletedAt = null;
        }

        assignment.Status = status;

        var updatedAssignment = await _assignmentRepository.Update(assignment);

        return _mapper.Map<AssignmentResponse>(updatedAssignment);
    }

    public async Task<IEnumerable<AssignmentResponse>> GetByCategoryId(int categoryId)
    {
        var category = await _categoryRepository.GetById(categoryId);

        if (category is null || category.UserId != _currentUser.UserId)
        {
            throw new KeyNotFoundException("Category not found");
        }

        var assignments = await _assignmentRepository.GetByCategoryId(categoryId);

        return _mapper.Map<IEnumerable<AssignmentResponse>>(assignments);
    }
}