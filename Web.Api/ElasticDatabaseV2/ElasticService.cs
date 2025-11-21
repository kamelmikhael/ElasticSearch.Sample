using Infrastructure.Database.Settings;
using Microsoft.Extensions.Options;
using Nest;

namespace Web.Api.ElasticDatabaseV2;

public class ElasticService<T> : IElasticService<T>
    where T : class
{
    private readonly IElasticClient _elasticClient;
    private readonly ElasticSettings _elasticSettings;

    public ElasticService(IOptions<ElasticSettings> elasticSettingsOptions)
    {
        _elasticSettings = elasticSettingsOptions.Value
            ?? throw new("Elastic Settings not found");

        var settings = new ConnectionSettings(new Uri(_elasticSettings.Url))
            .DefaultIndex(_elasticSettings.DefaultIndex);

        _elasticClient = new ElasticClient(settings);
    }

    public IEnumerable<T> SeedData()
    {
        // Index (Insert) document
        var product = new { Id = 1, Name = "Laptop", Price = 1200 };
        _elasticClient.IndexDocument(product);

        // Search
        //var searchResponse = _elasticClient.Search<T>(s => s
        //    .Query(q => q.Match(m => m.Field(f => f.Name).Query("laptop"))));

        // Search
        var searchResponse = _elasticClient.Search<T>(
            s => s.Query(q => q.MatchAll()));

        return searchResponse.Hits.Select(x => x.Source).ToList();
    }

    public async Task<(bool, string)> UpSertDocAsync(T document)
    {
        var response = await _elasticClient.IndexDocumentAsync(document);

        return (response.IsValid, response.Result.ToString());
    }

    public async Task<IEnumerable<T>> GetAll(string keyword)
    {
        var response = await _elasticClient.SearchAsync<T>(s => s
                .Query(q => q.Match(m => m.Field(f => f.Suffix("name")).Query(keyword))));

        return response.Documents;
    }
}
