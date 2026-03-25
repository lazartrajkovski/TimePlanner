using TimePlanner.Repositories.Data;
using Microsoft.EntityFrameworkCore;

namespace TimePlanner.Repositories.Repositories
{
    public class Repository<T> : IRepository<T> where T : class
    {
        protected readonly TimePlannerDbContext _context;

        public Repository(TimePlannerDbContext context)
        {
            _context = context;
        }
        public async Task<IEnumerable<T>> GetAll()
        {
            return await _context.Set<T>().ToListAsync();
        }

        public async Task<T?> GetById(int id)
        {
            return await _context.Set<T>().FindAsync(id);
        }

        public async Task<T> Add(T entity)
        {
            await _context.Set<T>().AddAsync(entity);
            await _context.SaveChangesAsync();

            return entity;
        }

        public async Task<T> Update(T entity)
        {
            _context.Set<T>().Update(entity);
            await _context.SaveChangesAsync();

            return entity;
        }

        public async Task<bool> Delete(int id)
        {
            var entity = await GetById(id);

            if (entity == null)
                return false;

            _context.Set<T>().Remove(entity);
            await _context.SaveChangesAsync();

            return true;
        }


    }
}
