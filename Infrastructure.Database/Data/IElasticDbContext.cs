namespace Infrastructure.Database.Data;

public interface IElasticDbContext
{
    Task<bool> CreateIndexIfNotExistsAsync(
        string indexName,
        CancellationToken cancellationToken = default);

    Task<bool> RemoveIndexAsync(
        string indexName,
        CancellationToken cancellationToken = default);

    Task<T?> GetDocByKeyAsync<T>(
        string key,
        string keyword,
        string indexName,
        CancellationToken cancellationToken = default);

    Task<IEnumerable<T>> GetAllDocsAsync<T>(
        string indexName,
        CancellationToken cancellationToken = default);


    Task<bool?> AddOrUpdateDocAsync<T>(
        T document,
        string indexName,
        CancellationToken cancellationToken = default);

    Task<bool> AddOrUpdateDocsBulkAsync<T>(
        IEnumerable<T> documents,
        string indexName,
        CancellationToken cancellationToken = default);

    Task<bool> RemoveDocByKeyAsync<T>(
        string key,
        string keyword,
        string indexName,
        CancellationToken cancellationToken = default);

    Task<long?> RemoveAllIndexDataAsync<T>(
        string indexName,
        CancellationToken cancellationToken = default);
}


