using Justin.EntityFramework.Model;
using Microsoft.EntityFrameworkCore;
using System.Collections;

namespace Justin.EntityFramework.Service {
    public interface IBaseService<TEntity> where TEntity : Base {
        Task<IEnumerable<TEntity>> GetAll();
        Task<TEntity> GetById(int id);
        Task<CrudResponse> Save(TEntity data, bool saveChanges = false);
        Task<CrudResponse> Update(TEntity data, bool updateChanges = false);
        Task<CrudResponse> Delete(TEntity data, bool deleteChanges = false);
        Task<PagedList<TEntity>> GetByFilter(PagedOptions pagedOptions);
    }
    public class BaseService<TEntity>(BaseDbContext dbContext) : IBaseService<TEntity> where TEntity : Base {

        private readonly BaseDbContext _dbContext = dbContext;

        #region Get

        public virtual async Task<IEnumerable<TEntity>> GetAll() {

            var entities = await _dbContext.Set<TEntity>()
                .Where(x => !x.IsDeleted)
                .AsNoTracking()
                .ToListAsync();

            return entities;

        }

        public virtual async Task<PagedList<TEntity>> GetByFilter(PagedOptions pagedOptions) {

            var entities = _dbContext.Set<TEntity>()
                .Where(x => !x.IsDeleted)
                .AsNoTracking();

            var pagedEntities = await PagedList<TEntity>.GetPagedList(entities, pagedOptions);
            return pagedEntities;

        }

        public virtual async Task<TEntity> GetById(int id) {

            var query = _dbContext.Set<TEntity>().Where(x => x.Id == id && !x.IsDeleted);

            foreach(var child in GetChildPropertyNames()) {

                query.Include(child);

            }

            var entity = await query.AsNoTracking().FirstOrDefaultAsync();

            return entity;

        }

        #endregion

        #region Save
        public virtual async Task<CrudResponse> Save(TEntity data, bool saveChanges = false) {

            if(data == null) return new CrudResponse() { IsSuccess = false, Message = "Data is null" };

            _dbContext.Set<TEntity>().Add(data);
            await SaveChanges(saveChanges);

            return new CrudResponse() {
                IsSuccess = true,
                Message = $"Successfully saved data to {typeof(TEntity).Name}",
                Entity = data
            };

        }

        #endregion

        #region Update
        public virtual async Task<CrudResponse> Update(TEntity data, bool updateChanges = false) {

            if(data == null) return new CrudResponse() { IsSuccess = false, Message = "Data is null" };

            var entry = _dbContext.ChangeTracker.Entries<TEntity>().FirstOrDefault(e => e.Entity.Id == data.Id);
            if(entry == null) {

                // entity not tracked, attatch
                _dbContext.Entry(data).State = EntityState.Modified;

            }
            else {

                if(!ReferenceEquals(data, entry.Entity)) {

                    entry.CurrentValues.SetValues(data);

                }
            }

            await SaveChanges(updateChanges);
            return new CrudResponse() {
                IsSuccess = true,
                Message = $"Successfully updated data to {typeof(TEntity).Name}",
                Entity = data
            };

        }

        #endregion

        #region Delete

        public virtual async Task<CrudResponse> Delete(TEntity data, bool deleteChanges = false) {

            _dbContext.Set<TEntity>().Remove(data);
            await SaveChanges(deleteChanges);
            return new CrudResponse() {
                IsSuccess = true,
                Message = $"Successfully deleted data from {typeof(TEntity).Name}",
                Entity = data
            };

        }

        #endregion

        #region Private Mehtod

        private async Task SaveChanges(bool saveChanges) {

            if(saveChanges)
                await _dbContext.SaveChangesAsync();

        }

        private static string[] GetChildPropertyNames() {

            return typeof(TEntity).GetProperties()
                .Where(p => p.PropertyType != typeof(string) && typeof(IEnumerable).IsAssignableFrom(p.PropertyType))
                .Select(p => p.Name)
                .ToArray();

        }

        #endregion

    }
}
