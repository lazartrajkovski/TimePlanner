using Microsoft.EntityFrameworkCore;
using TimePlanner.Domain.Entities;
using TimePlanner.Repositories.Data;
using TimePlanner.Repositories.Interfaces;

namespace TimePlanner.Repositories.Repositories
{
    public class UserRepository : BaseRepository<User>, IUserRepository
    {
       public UserRepository(TimePlannerDbContext context) : base(context)
       {

       }

        public async Task<User?> GetByEmail(string email)
        {
            return await _context.Set<User>()
                .Where(u => u.Email == email)
                .FirstOrDefaultAsync();
        }
    }
}
