using System.ComponentModel.DataAnnotations;

namespace Infrastructure.Database.Settings;

public class ElasticSettings
{
    [Required]
    public string Url { get; set; }

    [Required]
    public string DefaultIndex { get; set; }
}
