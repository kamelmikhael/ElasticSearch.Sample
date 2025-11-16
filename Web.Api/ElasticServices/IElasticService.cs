namespace Web.Api.ElasticServices;

public interface IElasticService<T>
{
    void SetIndexName(string indexName);

    Task CreateIndexIfNotExistsAsync(string indexName,
                                     CancellationToken cancellationToken = default);

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

    Task<long?> RemoveAllAsync(string indexName,
                               CancellationToken cancellationToken = default);
}
