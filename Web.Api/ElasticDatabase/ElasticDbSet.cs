using Elastic.Clients.Elasticsearch;
using Microsoft.Extensions.Options;
using Web.Api.Settings;

namespace Web.Api.ElasticDatabase;

public class ElasticDbSet<T> : IElasticDbSet<T>
{
    private readonly ILogger<ElasticDbSet<T>> _logger;
    private readonly ElasticsearchClient _elasticsearchClient;
    private readonly ElasticSettings _elasticSettings;

    public ElasticDbSet(
        ILogger<ElasticDbSet<T>> logger,
        IOptions<ElasticSettings> elasticSettingsOption)
    {
        _logger = logger;

        _elasticSettings = elasticSettingsOption.Value
            ?? throw new("Elastic Settings not foun, you must add to configurations");

        IndexName = _elasticSettings.DefaultIndex;

        var settings = new ElasticsearchClientSettings(new Uri(_elasticSettings.Url))
            //.Authentication()
            .DefaultIndex(IndexName);

        _elasticsearchClient = new(settings);
    }


    public string IndexName { get; private set; }

    public void SetIndexName(string indexName)
    {
        IndexName = indexName;

        _logger.LogInformation("Elasticsearch index name set to: {IndexName}, at {Time} UTC.",
                               indexName,
                               DateTime.UtcNow.ToString());
    }

    public async Task<bool?> AddOrUpdateAsync(T document,
                                        CancellationToken cancellationToken = default)
    {
        var response = await _elasticsearchClient.IndexAsync(document,
                                                             i => i.Index(IndexName).OpType(OpType.Index),
                                                             cancellationToken);

        if (!response.IsValidResponse)
        {
            _logger.LogError("Failed to Add/Update document. Error: {@Error}, at {Time} UTC",
                             response.ElasticsearchServerError,
                             DateTime.UtcNow.ToString());
        }

        return response.IsValidResponse;
    }

    public async Task<bool> AddOrUpdateBulkAsync(IEnumerable<T> documents,
                                           CancellationToken cancellationToken = default)
    {
        var response = await _elasticsearchClient.BulkAsync(
            b => b.Index(IndexName)
                  .UpdateMany(documents,
                              (ud, document) => ud.Doc(document).DocAsUpsert(true)),
            cancellationToken);

        if (!response.IsValidResponse)
        {
            _logger.LogError("Failed to Add/Update Bulk. Error: {@Error}, at {Time} UTC",
                             response.ElasticsearchServerError,
                             DateTime.UtcNow.ToString());
        }

        return response.IsValidResponse;
    }

    public async Task<T?> GetByKeyAsync(string key,
                                 CancellationToken cancellationToken = default)
    {
        var response = await _elasticsearchClient.GetAsync<T>(key,
                                                        g => g.Index(IndexName),
                                                        cancellationToken);

        if (!response.IsValidResponse)
        {
            _logger.LogError("Failed to Get Document By Key. Error: {@Error}, at {Time} UTC",
                             response.ElasticsearchServerError,
                             DateTime.UtcNow.ToString());
        }

        return response.Source;
    }

    public async Task<IEnumerable<T>> GetAllAsync(CancellationToken cancellationToken = default)
    {
        var response = await _elasticsearchClient.SearchAsync<T>(
            s => s.Indices(IndexName).Query(q => q.MatchAll()),
            cancellationToken);

        if (!response.IsValidResponse)
        {
            _logger.LogError("Failed to Get All Documents. Error: {@Error}, at {Time} UTC",
                             response.ElasticsearchServerError,
                             DateTime.UtcNow.ToString());
        }

        return response.Documents ?? [];
    }

    public async Task<bool> RemoveByKeyAsync(string key,
                                       CancellationToken cancellationToken = default)
    {
        var response = await _elasticsearchClient.DeleteAsync<T>(key,
                                                    d => d.Index(IndexName),
                                                    cancellationToken);

        if (!response.IsValidResponse)
        {
            _logger.LogError("Failed to Remove Document By Key. Error: {@Error}, at {Time} UTC",
                             response.ElasticsearchServerError,
                             DateTime.UtcNow.ToString());
        }

        return response.IsValidResponse;
    }

    public async Task<long?> RemoveAllAsync(CancellationToken cancellationToken = default)
    {
        var response = await _elasticsearchClient.DeleteByQueryAsync<T>(
            d => d.Indices(IndexName)
                  .Query(q => q.MatchAll()),
            cancellationToken);

        if (!response.IsValidResponse)
        {
            _logger.LogError("Failed to Remove All Documents from {Index}. Error: {@Error}, at {Time} UTC",
                             IndexName,
                             response.ElasticsearchServerError,
                             DateTime.UtcNow.ToString());
        }

        return response.Deleted;
    }

}
