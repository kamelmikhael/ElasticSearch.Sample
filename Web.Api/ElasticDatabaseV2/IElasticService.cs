namespace Web.Api.ElasticDatabaseV2;

public interface IElasticService<T>
{
    IEnumerable<T> SeedData();
    Task<(bool, string)> UpSertDocAsync(T document);
    Task<IEnumerable<T>> GetAll(string keyword);
}
