using TimePlanner.Domain.Entities;
using TimePlanner.Domain.Enums;
using TimePlanner.Repositories.Repositories;

namespace TimePlanner.Repositories.Interfaces;

public interface IAssignmentRepository : IBaseRepository<Assignment>
{
    Task<IEnumerable<Assignment>> GetByUserId(int userId, AssignmentStatus? status);
    Task<IEnumerable<Assignment>> GetByCategoryId(int categoryId);
    Task<Assignment?> GetByIdWithCategory(int assignmentId);
    Task<Assignment?> GetByNameAndCategoryId(string assignmentName, int categoryId);
    Task<IEnumerable<Assignment>> GetArchivedByUserId(int userId);
}