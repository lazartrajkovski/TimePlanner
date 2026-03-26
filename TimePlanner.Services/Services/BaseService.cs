using TimePlanner.Repositories.Repositories;
using TimePlanner.Services.Interfaces;

namespace TimePlanner.Services.Services
{
    public class BaseService<T> : IBaseService<T> where T : class
    {
        private readonly IBaseRepository<T> _repository;

        public BaseService(IBaseRepository<T> repository)
        {
            _repository = repository;
        }

        public async Task<IEnumerable<T>> GetAll()
        {
            return await _repository.GetAll();
        }

        public async Task<T?> GetById(int id)
        {
            return await _repository.GetById(id);
        }

        public async Task<T> Add(T entity)
        {
            await _repository.Add(entity);

            return entity;
        }

        public async Task<T> Update(T entity)
        {
            await _repository.Update(entity);

            return entity;
        }

        public async Task<bool> Delete(int id)
        {
            return await _repository.Delete(id);
        }
    }
}
