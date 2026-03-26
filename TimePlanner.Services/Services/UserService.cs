using TimePlanner.Domain.Entities;
using TimePlanner.Repositories.Interfaces;
using TimePlanner.Services.Interfaces;

namespace TimePlanner.Services.Services
{
    public class UserService : BaseService<User>, IUserService
    {
        private readonly IUserRepository _userRepository;
        public UserService(IUserRepository userRepository) : base(userRepository)
        {
            _userRepository = userRepository;
        }

        public async Task<User?> GetByEmail(string email)
        {
            return await _userRepository.GetByEmail(email);
        }
    }
}
