using MapsterMapper;
using TimePlanner.Domain.Entities;
using TimePlanner.Repositories.Interfaces;
using TimePlanner.Services.DTOs;
using TimePlanner.Services.Interfaces;
using TimePlanner.Services.Validators;

namespace TimePlanner.Services.Services
{
    public class CategoryService : ICategoryService
    {
        private readonly ICategoryRepository _categoryRepository;
        private readonly ICurrentUser _currentUser;
        private readonly IAssignmentRepository _assignmentRepository;
        private readonly ITimeEntryRepository _timeEntryRepository;
        private readonly IMapper _mapper;

        public CategoryService(ICategoryRepository categoryRepository, IAssignmentRepository assignmentRepository,
            ICurrentUser currentUser, ITimeEntryRepository timeEntryRepository, IMapper mapper)
        {
            _categoryRepository = categoryRepository;
            _currentUser = currentUser;
            _assignmentRepository = assignmentRepository;
            _timeEntryRepository = timeEntryRepository;
            _mapper = mapper;
        }

        public async Task<CategoryResponse> Add(CategoryRequest request)
        {
            CategoryValidator.ValidateAndNormalize(request);

            if (await _categoryRepository.GetByNameAndUserId(request.CategoryName, _currentUser.UserId) is not null)
            {
                throw new InvalidOperationException("Category with this name already exists");
            }

            var category = _mapper.Map<Category>(request);
            category.UserId = _currentUser.UserId;

            var createdCategory = await _categoryRepository.Add(category);

            return _mapper.Map<CategoryResponse>(createdCategory);
        }

        public async Task<IEnumerable<CategoryResponse>> GetAll()
        {
            var categories = await _categoryRepository.GetByUserId(_currentUser.UserId);

            return _mapper.Map<IEnumerable<CategoryResponse>>(categories);
        }

        public async Task<CategoryResponse> GetById(int id)
        {
            var category = await _categoryRepository.GetById(id);

            if (category is null || category.UserId != _currentUser.UserId)
            {
                throw new KeyNotFoundException("Category not found");
            }

            return _mapper.Map<CategoryResponse>(category);
        }

        public async Task<CategoryResponse> Update(int id, CategoryRequest request)
        {
            CategoryValidator.ValidateAndNormalize(request);

            var category = await _categoryRepository.GetById(id);

            if (category is null || category.UserId != _currentUser.UserId)
            {
                throw new KeyNotFoundException("Category not found");
            }

            var existingCategory = await _categoryRepository.GetByNameAndUserId(request.CategoryName, _currentUser.UserId);

            if (existingCategory is not null && existingCategory.Id != id)
            {
                throw new InvalidOperationException("Category with this name already exists");
            }

            category.CategoryName = request.CategoryName;

            var updatedCategory = await _categoryRepository.Update(category);

            return _mapper.Map<CategoryResponse>(updatedCategory);
        }

        public async Task Delete(int id)
        {
            var category = await _categoryRepository.GetById(id);

            if (category is null || category.UserId != _currentUser.UserId)
            {
                throw new KeyNotFoundException("Category not found");
            }

            var activeTimeEntry = await _timeEntryRepository.GetRunningByUserId(_currentUser.UserId);

            if (activeTimeEntry is not null)
            {
                var activeAssignment = await _assignmentRepository.GetByIdWithCategory(activeTimeEntry.AssignmentId);

                if (activeAssignment is not null && activeAssignment.Category.Id == category.Id)
                {
                    throw new InvalidOperationException("Cannot delete an assignment with active time entry");
                }
            }

            await _categoryRepository.Delete(id);
        }
    }
}