using TimePlanner.Domain.Entities;
using TimePlanner.Domain.Enums;
using TimePlanner.Repositories.Interfaces;
using TimePlanner.Services.Interfaces;

namespace TimePlanner.Services.Services
{
    public class AssignmentService : BaseService<Assignment>, IAssignmentService
    {
        private readonly IAssignmentRepository _assignmentRepository;

        public AssignmentService(IAssignmentRepository assignmentRepository) : base(assignmentRepository)        {
            _assignmentRepository = assignmentRepository;
        }

        public async Task<IEnumerable<Assignment>> GetByUserId(int userId)
        {
            return await _assignmentRepository.GetByUserId(userId);
        }

        public async Task<IEnumerable<Assignment>> GetByCategoryId(int categoryId)
        {
            return await _assignmentRepository.GetByCategoryId(categoryId);
        }

        public async Task<IEnumerable<Assignment>> GetByStatus(int userId, AssignmentStatus status)
        {
            return await _assignmentRepository.GetByStatus(userId, status);
        }
    }
}
