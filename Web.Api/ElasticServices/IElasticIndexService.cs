namespace Web.Api.ElasticServices;

/// <summary>
/// Manage the index operations (create, update, delete, search)
/// </summary>
/// <typeparam name="T"></typeparam>
public interface IElasticIndexService<T>
{
}

public class ElasticIndexService<T> : IElasticIndexService<T>
{
}