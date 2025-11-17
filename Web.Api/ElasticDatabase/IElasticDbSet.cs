namespace Web.Api.ElasticDatabase;

public interface IElasticDbSet<T>
{
    string IndexName { get; }

    void SetIndexName(string indexName);

    Task<bool?> AddOrUpdateAsync(T document,
                                 CancellationToken cancellationToken = default);

    Task<bool> AddOrUpdateBulkAsync(IEnumerable<T> documents,
                                    CancellationToken cancellationToken = default);

    Task<T?> GetByKeyAsync(string key,
                          CancellationToken cancellationToken = default);

    Task<IEnumerable<T>> GetAllAsync(CancellationToken cancellationToken = default);

    Task<bool> RemoveByKeyAsync(string key,
                                CancellationToken cancellationToken = default);

    Task<long?> RemoveAllAsync(CancellationToken cancellationToken = default);
}
