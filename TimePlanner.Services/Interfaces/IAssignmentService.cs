using TimePlanner.Domain.Enums;
using TimePlanner.Services.DTOs;

namespace TimePlanner.Services.Interfaces;

public interface IAssignmentService
{
    Task<AssignmentResponse> Add(AssignmentRequest request);
    Task<IEnumerable<AssignmentResponse>> GetAll(AssignmentStatus? status);
    Task<AssignmentResponse> GetById(int id);
    Task<AssignmentResponse> Update(int id, AssignmentRequest request);
    Task<AssignmentResponse> ChangeStatus(int id, AssignmentStatus status);
    Task Delete(int id);
    Task Archive(int id);
    Task<IEnumerable<AssignmentResponse>> GetByCategoryId(int categoryId);
    Task<IEnumerable<AssignmentResponse>> GetArchivedByUserId();
}