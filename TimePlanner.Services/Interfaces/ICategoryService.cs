using TimePlanner.Domain.Entities;
using TimePlanner.Services.DTOs;

namespace TimePlanner.Services.Interfaces
{
    public interface ICategoryService
    {
        Task<CategoryResponse> Add(CategoryRequest request);
        Task<IEnumerable<CategoryResponse>> GetAll();
        Task<CategoryResponse> GetById(int id);
        Task<CategoryResponse> Update(int id, CategoryRequest request);
        Task Delete(int id);
        Task<IEnumerable<CategoryResponse>> GetByUserId(int userId);
    }
}