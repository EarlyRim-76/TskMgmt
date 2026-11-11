using Microsoft.EntityFrameworkCore;
using TskMgmt.Api.Context;
using TskMgmt.Api.Interface;

namespace TskMgmt.Api.Service
{
    public class GenericService<T> : IGenericService<T> where T : class
    {
        private readonly AppDbContext _context;

        //Add Generic Service for AsNoTracking and AsTacking list and one;
        public GenericService( AppDbContext context)
        {
            _context = context;
        }

        public async Task<IEnumerable<T>> GetAllAsync()
        {
            return await _context.Set<T>().ToListAsync();
        }

        public async Task<T?> GetByIdAsync(Guid id)
        {
            return await _context.Set<T>().FindAsync(id);
        }

        public async Task<T> CreateAsync(T entity)
        {
           _context.Set<T>().Add(entity);
            await _context.SaveChangesAsync();

            return entity;
        }

        public async Task<T> UpdateAsync(T entity)
        {
            _context.Set<T>().Update(entity);
            await _context.SaveChangesAsync();

            return entity;
        }

        public async Task<bool> DeleteAsync(Guid id)
        {
            var entity = await _context.Set<T>().FindAsync(id);

            if(entity == null)
                return false;

            _context.Set<T>().Remove(entity);
            await _context.SaveChangesAsync();

            return true;
        }

        public Task<T> PatchAsync(T entity)
        {
            throw new NotImplementedException(); //Modify when needed
        }
    }
}
