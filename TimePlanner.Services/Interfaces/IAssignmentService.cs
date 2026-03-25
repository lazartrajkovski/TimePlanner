using TimePlanner.Domain.Entities;
using TimePlanner.Domain.Enums;

namespace TimePlanner.Services.Interfaces
{
    public interface IAssignmentService : IService<Assignment>
    {
        Task<IEnumerable<Assignment>> GetByUserId(int userId);
        Task<IEnumerable<Assignment>> GetByCategoryId(int categoryId);
        Task<IEnumerable<Assignment>> GetByStatus(int userId, AssignmentStatus status);
    }
}
