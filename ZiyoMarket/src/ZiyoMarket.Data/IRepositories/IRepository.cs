using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using System.Threading.Tasks;
using ZiyoMarket.Domain.Entities.Users;

namespace ZiyoMarket.Data.IRepositories
{
    public interface IRepository<TEntity>
    {
        Task<TEntity> UpdateAsync(TEntity entity);

        IQueryable<TEntity> Table { get; }


        /// <summary>
        /// Yangi obyektni bazaga qo‘shish
        /// </summary>
        Task<TEntity> InsertAsync(TEntity entity);

        /// <summary>
        /// Bazadan obyektni o‘chirish (by Id)
        /// </summary>
        Task<bool> DeleteAsync(long id);

        /// <summary>
        /// Ma’lumotni yangilash
        /// </summary>
        Task<TEntity> Update(TEntity entity, long id);
        /// <summary>
        /// Ma’lumotni yangilash — shart (predicate) bo‘yicha (masalan: telefon bo'yicha)
        /// Odatda mavjud yozuv topilib, uning qiymatlari yangi entity qiymatlari bilan almashtiriladi.
        /// </summary>
        Task<TEntity> UpdateByAsync(Expression<Func<TEntity, bool>> predicate, TEntity entity);

        /// <summary>
        /// Ma’lumotni olish (bitta obyekt)
        /// </summary>
        Task<TEntity> SelectAsync(Expression<Func<TEntity, bool>> expression, string[] includes = null);

        /// <summary>
        /// Barcha ma’lumotlarni olish (IQueryable)
        /// </summary>
        IQueryable<TEntity> SelectAll(Expression<Func<TEntity, bool>> expression = null, string[] includes = null);
        /// <summary>
        /// ID orqali obyektni olish
        /// </summary>
        Task<TEntity> GetByIdAsync(long id, string[] includes = null);
        /// <summary>
        /// Barcha ma’lumotlarni olish (ToList)
        /// </summary>
        Task<IList<TEntity>> SelectAllAsync(Expression<Func<TEntity, bool>> expression = null, string[] includes = null);

        /// <summary>
        /// Ma’lumot bazasida shu shart bo‘yicha yozuv bormi?
        /// </summary>
        Task<bool> AnyAsync(Expression<Func<TEntity, bool>> expression);

        /// <summary>
        /// Yozuvlar soni (filtrlash bilan)
        /// </summary>
        Task<int> CountAsync(Expression<Func<TEntity, bool>> expression = null);

        /// <summary>
        /// O‘zgarishlarni saqlash
        /// </summary>
        Task<bool> SaveAsync();
        Task UpdateByAsync(Customer user);
    }
}
