using DbManager.Dapper.Interfaces;
using SqlKata.Compilers;
using SqlKata.Execution;
using System;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DbManager.Dapper.Implementations
{
    public abstract class BaseRepository<TEntity> : IRepository<TEntity> where TEntity : class
    {
        protected readonly QueryFactory _db;
        protected readonly string _tableName;

        protected BaseRepository(IDbConnection connection, string tableName)
        {
            var compiler = new PostgresCompiler();
            _db = new QueryFactory(connection, compiler);
            _tableName = tableName;
        }

        public virtual async Task<IEnumerable<TEntity>> GetAllAsync()
        {
            return await _db.Query(_tableName).GetAsync<TEntity>();
        }

        public virtual async Task<TEntity?> GetByIdAsync(Guid id)
        {
            return await _db.Query(_tableName).Where("id", id).FirstOrDefaultAsync<TEntity>();
        }

        public virtual async Task<IEnumerable<TEntity>> FindByAsync(Dictionary<string, object> conditions)
        {
            var query = _db.Query(_tableName);

            foreach (var condition in conditions)
            {
                query = query.Where(condition.Key, condition.Value);
            }

            return await query.GetAsync<TEntity>();
        }

        public virtual async Task InsertAsync(TEntity entity)
        {
            await _db.Query(_tableName).InsertAsync(entity);
        }

        public virtual async Task UpdateAsync(Guid id, object updatedFields)
        {
            await _db.Query(_tableName).Where("id", id).UpdateAsync(updatedFields);
        }

        public virtual async Task DeleteAsync(Guid id)
        {
            await _db.Query(_tableName).Where("id", id).UpdateAsync(new { is_deleted = true });
        }

        public virtual async Task<bool> ExistsAsync(Guid id)
        {
            var count = await _db.Query(_tableName).Where("id", id).WhereFalse("is_deleted").CountAsync<int>();
            return count > 0;
        }
    }
}
