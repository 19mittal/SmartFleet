using Microsoft.EntityFrameworkCore;
using SmartFleet.Auth.Domain.Interfaces;
using SmartFleet.Auth.Infrastructure.Persistence;

namespace SmartFleet.Auth.Infrastructure.Repositories
{
    public class GenericRepository<T> : IGenericRepository<T> where T : class
    {
        protected readonly AuthDbContext _context;
        protected readonly DbSet<T> _dbSet;

        public GenericRepository(AuthDbContext context)
        {
            _context = context;
            _dbSet = context.Set<T>();
        }

        public async Task<T?> GetByIdAsync(Guid id) => await _dbSet.FindAsync(id);

        public async Task<IEnumerable<T>> GetAllAsync() => await _dbSet.ToListAsync();

        public async Task AddAsync(T entity) => await _dbSet.AddAsync(entity);

        public void Update(T entity) => _dbSet.Update(entity);

        public void Delete(T entity) => _dbSet.Remove(entity);
    }
}
