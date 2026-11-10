using MapsterMapper;
using TimePlanner.Domain.Entities;
using TimePlanner.Domain.Enums;
using TimePlanner.Repositories.Interfaces;
using TimePlanner.Services.DTOs;
using TimePlanner.Services.Interfaces;
using TimePlanner.Services.Validators;

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
        AssignmentValidator.ValidateAndNormalize(request);

        var category = await _categoryRepository.GetById(request.CategoryId);

        if (category is null || category.UserId != _currentUser.UserId)
        {
            throw new KeyNotFoundException("Category not found");
        }

        if (await _assignmentRepository.GetByNameAndCategoryId(request.AssignmentName, request.CategoryId) is not null)
        {
            throw new InvalidOperationException("Assignment with this name already exists");
        }

        var assignment = _mapper.Map<Assignment>(request);

        var createdAssignment = await _assignmentRepository.Add(assignment);

        return _mapper.Map<AssignmentResponse>(createdAssignment);
    }

    public async Task<IEnumerable<AssignmentResponse>> GetAll(AssignmentStatus? status)
    {
        AssignmentValidator.ValidateStatus(status);

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
        AssignmentValidator.ValidateAndNormalize(request);

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

        var existingAssignment = await _assignmentRepository.GetByNameAndCategoryId(request.AssignmentName, request.CategoryId);

        if (existingAssignment is not null && existingAssignment.Id != id)
        {
            throw new InvalidOperationException("Assignment with this name already exists");
        }

        _mapper.Map(request, assignment);

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
        AssignmentValidator.ValidateStatus(status);

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