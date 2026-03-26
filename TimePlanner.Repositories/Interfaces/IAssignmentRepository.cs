using TimePlanner.Domain.Entities;
using TimePlanner.Domain.Enums;
using TimePlanner.Repositories.Repositories;

namespace TimePlanner.Repositories.Interfaces
{
    public interface IAssignmentRepository : IBaseRepository<Assignment>
    {
        Task<IEnumerable<Assignment>> GetByUserId(int userId);
        Task<IEnumerable<Assignment>> GetByCategoryId(int categoryId);
        Task<IEnumerable<Assignment>> GetByStatus(int userId, AssignmentStatus status);
    }
}