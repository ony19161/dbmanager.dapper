using System.Data;
using System.Text.RegularExpressions;
using DbManager.Dapper.Interfaces;
using SqlKata;
using SqlKata.Compilers;
using SqlKata.Execution;

namespace DbManager.Dapper.Implementations
{
    public abstract partial class BaseRepository<TEntity> : IRepository<TEntity> where TEntity : class
    {
        private static readonly PostgresCompiler Compiler = new();

        [GeneratedRegex("^[A-Za-z_][A-Za-z0-9_]{0,62}$")]
        private static partial Regex ColumnPattern();

        protected readonly QueryFactory _db;
        protected readonly string _tableName;

        /// <summary>Hard ceiling for page size, regardless of what callers request.</summary>
        protected virtual int MaxPageSize => 1000;

        /// <summary>
        /// Columns callers may filter on in <see cref="FindByAsync"/>. When null, any
        /// syntactically valid column name is accepted.
        /// </summary>
        protected virtual IReadOnlySet<string>? FilterableColumns => null;

        /// <param name="connection">
        /// Unopened, per-scope connection (see <see cref="IDbConnectionFactory"/>). Not shared across threads.
        /// </param>
        protected BaseRepository(IDbConnection connection, string tableName)
        {
            _db = new QueryFactory(connection, Compiler);
            _tableName = tableName;
        }

        private Query Active() => _db.Query(_tableName).WhereFalse("is_deleted");

        private (int limit, int offset) Page(int limit, int offset) =>
            (Math.Clamp(limit, 1, MaxPageSize), Math.Max(offset, 0));

        public virtual async Task<IEnumerable<TEntity>> GetAllAsync(int limit = 100, int offset = 0, CancellationToken ct = default)
        {
            var (l, o) = Page(limit, offset);
            return await Active().OrderBy("id").Limit(l).Offset(o).GetAsync<TEntity>(cancellationToken: ct);
        }

        public virtual async Task<TEntity?> GetByIdAsync(Guid id, CancellationToken ct = default)
        {
            return await Active().Where("id", id).FirstOrDefaultAsync<TEntity>(cancellationToken: ct);
        }

        public virtual async Task<IEnumerable<TEntity>> FindByAsync(IReadOnlyDictionary<string, object> conditions, int limit = 100, int offset = 0, CancellationToken ct = default)
        {
            var (l, o) = Page(limit, offset);
            var query = Active();

            foreach (var (column, value) in conditions)
            {
                if (!ColumnPattern().IsMatch(column) || (FilterableColumns is { } allowed && !allowed.Contains(column)))
                    throw new ArgumentException($"Filtering on column '{column}' is not allowed.", nameof(conditions));

                query = query.Where(column, value);
            }

            return await query.OrderBy("id").Limit(l).Offset(o).GetAsync<TEntity>(cancellationToken: ct);
        }

        public virtual async Task InsertAsync(TEntity entity, CancellationToken ct = default)
        {
            await _db.Query(_tableName).InsertAsync(entity, cancellationToken: ct);
        }

        public virtual async Task UpdateAsync(Guid id, object updatedFields, CancellationToken ct = default)
        {
            await Active().Where("id", id).UpdateAsync(updatedFields, cancellationToken: ct);
        }

        public virtual async Task DeleteAsync(Guid id, CancellationToken ct = default)
        {
            await Active().Where("id", id).UpdateAsync(new { is_deleted = true }, cancellationToken: ct);
        }

        public virtual async Task<bool> ExistsAsync(Guid id, CancellationToken ct = default)
        {
            var count = await Active().Where("id", id).CountAsync<int>(cancellationToken: ct);
            return count > 0;
        }
    }
}
