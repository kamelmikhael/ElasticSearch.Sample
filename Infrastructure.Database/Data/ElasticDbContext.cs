using Elastic.Clients.Elasticsearch;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using Infrastructure.Database.Settings;

namespace Infrastructure.Database.Data;

public class ElasticDbContext : IElasticDbContext
{
    private readonly ILogger<ElasticDbContext> _logger;
    private readonly ElasticsearchClient _elasticsearchClient;
    private readonly ElasticSettings _elasticSettings;

    public ElasticDbContext(
        ILogger<ElasticDbContext> logger,
        IOptions<ElasticSettings> elasticSettingsOption)
    {
        _logger = logger;

        _elasticSettings = elasticSettingsOption.Value
            ?? throw new("Elastic Settings not foun, you must add to configurations");

        var settings = new ElasticsearchClientSettings(new Uri(_elasticSettings.Url))
            //.Authentication()
            .DefaultIndex(_elasticSettings.DefaultIndex);

        _elasticsearchClient = new(settings);
    }

    #region Index Operations (Create/Remove)
    public async Task<bool> CreateIndexIfNotExistsAsync(string indexName,
                                                  CancellationToken cancellationToken = default)
    {
        if (!_elasticsearchClient.Indices.Exists(indexName).Exists)
        {
            var createIndexResponse = await _elasticsearchClient.Indices.CreateAsync(indexName, cancellationToken);

            if (!createIndexResponse.IsValidResponse)
            {
                _logger.LogError("Failed to create {Index}. Error: {@Error}, at {Time} UTC",
                                 indexName,
                                 createIndexResponse.ElasticsearchServerError,
                                 DateTime.UtcNow.ToString());

                return false;
            }
        }

        return true;
    }

    public async Task<bool> RemoveIndexAsync(string indexName,
                                CancellationToken cancellationToken = default)
    {
        var response = await _elasticsearchClient.Indices.DeleteAsync(indexName, cancellationToken);

        if (!response.IsValidResponse)
        {
            _logger.LogError("Failed to Remove Index {Index}. Error: {@Error}, at {Time} UTC",
                             indexName,
                             response.ElasticsearchServerError,
                             DateTime.UtcNow.ToString());
        }

        return response.IsValidResponse;
    }
    #endregion

    #region Read Document from Index
    public async Task<T?> GetDocByKeyAsync<T>(
        string key,
        string keyword,
        string indexName,
        CancellationToken cancellationToken = default)
    {
        var response = await _elasticsearchClient.SearchAsync<T>(
            s => s.Indices(indexName)
                  .Query(q => 
                         q.Match(m => 
                                 m.Field(f => 
                                         f.Suffix(key))
                                  .Query(keyword))),
            cancellationToken);

        if (!response.IsValidResponse)
        {
            _logger.LogError("Failed to Get Document By {Key}. Error: {@Error}, at {Time} UTC",
                key,
                response.ElasticsearchServerError,
                DateTime.UtcNow.ToString());
        }

        return response.Documents.FirstOrDefault();
    }

    public async Task<IEnumerable<T>> GetAllDocsAsync<T>(string indexName,
                                                  CancellationToken cancellationToken = default)
    {
        var response = await _elasticsearchClient.SearchAsync<T>(
            s => s.Indices(indexName).Query(q => q.MatchAll()),
            cancellationToken);

        if (!response.IsValidResponse)
        {
            _logger.LogError("Failed to Get All Documents. Error: {@Error}, at {Time} UTC",
                             response.ElasticsearchServerError,
                             DateTime.UtcNow.ToString());
        }

        return response.Documents ?? [];
    }

    #endregion

    #region CRUD operations for Documents
    public async Task<bool?> AddOrUpdateDocAsync<T>(T document,
                                                 string indexName,
                                                 CancellationToken cancellationToken = default)
    {
        var response = await _elasticsearchClient.IndexAsync(document,
                                                             i => i.Index(indexName).OpType(OpType.Index),
                                                             cancellationToken);

        if (!response.IsValidResponse)
        {
            _logger.LogError("Failed to Add/Update document. Error: {@Error}, at {Time} UTC",
                             response.ElasticsearchServerError,
                             DateTime.UtcNow.ToString());
        }

        return response.IsValidResponse;
    }

    public async Task<bool> AddOrUpdateDocsBulkAsync<T>(IEnumerable<T> documents,
                                                    string indexName,
                                                    CancellationToken cancellationToken = default)
    {
        var response = await _elasticsearchClient.BulkAsync(
            b => b.Index(indexName)
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


    public async Task<bool> RemoveDocByKeyAsync<T>(
        string key,
        string keyword,
        string indexName,
        CancellationToken cancellationToken = default)
    {
        var response = await _elasticsearchClient.DeleteByQueryAsync<T>(
            d => d.Indices(indexName)
                  .Query(q => q.Match(m => m.Field(f => f.Suffix(key))
                                            .Query(keyword))
                  ),
            cancellationToken);

        if (!response.IsValidResponse)
        {
            _logger.LogError("Failed to Remove All Documents from {Index}. Error: {@Error}, at {Time} UTC",
                             indexName,
                             response.ElasticsearchServerError,
                             DateTime.UtcNow.ToString());
        }

        return response.IsValidResponse;
    }

    public async Task<long?> RemoveAllIndexDataAsync<T>(string indexName,
                                                  CancellationToken cancellationToken = default)
    {
        var response = await _elasticsearchClient.DeleteByQueryAsync<T>(
            d => d.Indices(indexName)
                  .Query(q => q.MatchAll()),
            cancellationToken);

        if (!response.IsValidResponse)
        {
            _logger.LogError("Failed to Remove All Documents from {Index}. Error: {@Error}, at {Time} UTC",
                             indexName,
                             response.ElasticsearchServerError,
                             DateTime.UtcNow.ToString());
        }

        return response.Deleted;
    }
    #endregion

}