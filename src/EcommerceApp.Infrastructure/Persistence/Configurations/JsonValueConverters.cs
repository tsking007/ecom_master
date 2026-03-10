using Microsoft.EntityFrameworkCore.ChangeTracking;
using Microsoft.EntityFrameworkCore.Storage.ValueConversion;
using System.Text.Json;

namespace EcommerceApp.Infrastructure.Persistence.Configurations;

/// <summary>
/// Reusable EF Core value converters and comparers for types that
/// cannot be mapped directly to SQL Server columns (e.g. List<string>).
/// Used by ProductConfiguration and ElasticStoreConfiguration.
/// </summary>
internal static class JsonValueConverters
{
    private static readonly JsonSerializerOptions _options = new()
    {
        WriteIndented = false
    };

    /// <summary>
    /// Serializes List&lt;string&gt; to a JSON string for storage in nvarchar(max).
    /// The ValueComparer prevents EF from issuing spurious UPDATE statements
    /// when the list content hasn't actually changed.
    /// </summary>
    public static (
        ValueConverter<List<string>, string>,
        ValueComparer<List<string>>)
        StringList()
    {
        var converter = new ValueConverter<List<string>, string>(
            v => JsonSerializer.Serialize(v, _options),
            v => string.IsNullOrEmpty(v)
                ? new List<string>()
                : JsonSerializer.Deserialize<List<string>>(v, _options)
                  ?? new List<string>());

        var comparer = new ValueComparer<List<string>>(
            (c1, c2) => c1 != null && c2 != null && c1.SequenceEqual(c2),
            c => c.Aggregate(0, (a, v) => HashCode.Combine(a, v.GetHashCode())),
            c => c.ToList());

        return (converter, comparer);
    }
}