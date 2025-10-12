using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using System.Threading.Tasks;
using ZiyoMarket.Data.DbContexts;
using ZiyoMarket.Data.IRepositories;
using ZiyoMarket.Domain.Common;
using ZiyoMarket.Domain.Entities.Users;

namespace ZiyoMarket.Data.Services
{
    public class RepositoryService<TEntity> : IRepository<TEntity> where TEntity : BaseEntity
    {
        private readonly ZiyoMarketDbContext _context;
        private readonly DbSet<TEntity> _dbSet;

        public RepositoryService(ZiyoMarketDbContext context)
        {
            _context = context;
            _dbSet = _context.Set<TEntity>();
        }

        public IQueryable<TEntity> Table => _dbSet;

        // ➕ Create
        public async Task<TEntity> InsertAsync(TEntity entity)
        {
            var entry = await _dbSet.AddAsync(entity);
            return entry.Entity;
        }

        // 🗑 Delete
        public async Task<bool> DeleteAsync(long id)
        {
            var entity = await _dbSet.FirstOrDefaultAsync(e => e.Id == id);
            if (entity == null)
                return false;

            _dbSet.Remove(entity);
            return true;
        }

        // 🔄 Update
        public async Task<TEntity> Update(TEntity entity, long id)
        {
            var existing = await _dbSet.AsNoTracking().FirstOrDefaultAsync(e => e.Id == id);
            if (existing == null)
                throw new Exception("Entity topilmadi.");

            _context.Entry(entity).State = EntityState.Modified;
            return _dbSet.Update(entity).Entity;
        }

        // 📄 Get one (with include)
        public async Task<TEntity> SelectAsync(Expression<Func<TEntity, bool>> expression, string[] includes = null)
        {
            IQueryable<TEntity> query = _dbSet;

            if (includes != null)
            {
                foreach (var include in includes)
                    query = query.Include(include);
            }

            return await query.FirstOrDefaultAsync(expression);
        }

        // 📋 Get all (IQueryable)
        public IQueryable<TEntity> SelectAll(Expression<Func<TEntity, bool>> expression = null, string[] includes = null)
        {
            IQueryable<TEntity> query = _dbSet;

            if (includes != null)
            {
                foreach (var include in includes)
                    query = query.Include(include);
            }

            if (expression != null)
                query = query.Where(expression);

            return query;
        }
        public async Task<TEntity> UpdateByAsync(Expression<Func<TEntity, bool>> predicate, TEntity entity)
        {
            // Topilgan yozuvni bazadan olamiz (tracking bilan)
            var existing = await _dbSet.FirstOrDefaultAsync(predicate);
            if (existing == null)
                throw new Exception($"{typeof(TEntity).Name} not found by given predicate.");

            // Agar siz Id saqlab qolmoqchi bo'lsangiz:
            entity.Id = existing.Id;

            // Barcha propertylarni yangilash (EF Core CurrentValues)
            _context.Entry(existing).CurrentValues.SetValues(entity);

            // Agar kerak bo'lsa: UpdatedAt/UpdatedBy kabi audit maydonlarini yangilash shu yerda qiling
            return existing;
        }

        public async Task<TEntity> UpdateAsync(TEntity entity)
        {
            var existing = await _dbSet.FindAsync(entity.Id);
            if (existing == null)
                throw new Exception($"{typeof(TEntity).Name} not found with Id = {entity.Id}");

            _context.Entry(existing).CurrentValues.SetValues(entity);
            return existing;
        }

        // 📋 Get all async (List)
        public async Task<IList<TEntity>> SelectAllAsync(Expression<Func<TEntity, bool>> expression = null, string[] includes = null)
        {
            IQueryable<TEntity> query = _dbSet;

            if (includes != null)
            {
                foreach (var include in includes)
                    query = query.Include(include);
            }

            if (expression != null)
                query = query.Where(expression);

            return await query.ToListAsync();
        }
        // 🔍 Get by Id
        public async Task<TEntity> GetByIdAsync(long id, string[] includes = null)
        {
            IQueryable<TEntity> query = _dbSet;

            if (includes != null)
            {
                foreach (var include in includes)
                    query = query.Include(include);
            }

            return await query.FirstOrDefaultAsync(e => e.Id == id);
        }
        // ❓ AnyAsync
        public async Task<bool> AnyAsync(Expression<Func<TEntity, bool>> expression)
        {
            return await _dbSet.AnyAsync(expression);
        }

        // 🔢 CountAsync
        public async Task<int> CountAsync(Expression<Func<TEntity, bool>> expression = null)
        {
            if (expression != null)
                return await _dbSet.CountAsync(expression);
            return await _dbSet.CountAsync();
        }

        // 💾 Save changes
        public async Task<bool> SaveAsync()
        {
            return await _context.SaveChangesAsync() > 0;
        }

        public Task UpdateByAsync(Customer user)
        {
            throw new NotImplementedException();
        }
    }
}
