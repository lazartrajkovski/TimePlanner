using MapsterMapper;
using TimePlanner.Domain.Entities;
using TimePlanner.Repositories.Interfaces;
using TimePlanner.Services.DTOs;
using TimePlanner.Services.Interfaces;

namespace TimePlanner.Services.Services
{
    public class CategoryService : ICategoryService
    {
        private readonly ICategoryRepository _categoryRepository;
        private readonly ICurrentUser _currentUser;
        private readonly IMapper _mapper;

        public CategoryService(ICategoryRepository categoryRepository, ICurrentUser currentUser, IMapper mapper)
        {
            _categoryRepository = categoryRepository;
            _currentUser = currentUser;
            _mapper = mapper;
        }

        public async Task<CategoryResponse> Add(CategoryRequest request)
        {
            if (string.IsNullOrWhiteSpace(request.CategoryName))
            {
                throw new ArgumentException("Category name is required");
            }

            var categoryName = request.CategoryName.Trim();

            if (categoryName.Length > 50)
            {
                throw new ArgumentException("Category name is too long");
            }

            var category = _mapper.Map<Category>(request);
            category.CategoryName = categoryName;
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
            if (string.IsNullOrWhiteSpace(request.CategoryName))
            {
                throw new ArgumentException("Category name is required");
            }

            var categoryName = request.CategoryName.Trim();

            if (categoryName.Length > 50)
            {
                throw new ArgumentException("Category name is too long");
            }

            var category = await _categoryRepository.GetById(id);

            if (category is null || category.UserId != _currentUser.UserId)
            {
                throw new KeyNotFoundException("Category not found");
            }

            category.CategoryName = categoryName;

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

            await _categoryRepository.Delete(id);
        }
    }
}