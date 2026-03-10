namespace EcommerceApp.Application.Common;

/// <summary>
/// Bound to the "SearchSettings" section in appsettings.json.
/// Injected via IOptions&lt;SearchSettings&gt;.
///
/// Provider values:
///   "Elasticsearch" — full Elasticsearch with fuzzy search, boosts, highlights
///   "Sql"           — EF Core LIKE queries against the Products table (no extra infra)
/// </summary>
public class SearchSettings
{
    public const string SectionName = "SearchSettings";

    /// <summary>"Elasticsearch" or "Sql"</summary>

    //public string Provider { get; init; } = "Elasticsearch";
    public string Provider { get; init; } = "Sql";


    // ── Elasticsearch-specific ────────────────────────────────────────────────

    /// <summary>Elasticsearch node URI. Example: http://localhost:9200</summary>
    public string Uri { get; init; } = "http://localhost:9200";

    /// <summary>Username for Elasticsearch security (leave empty for local dev).</summary>
    public string? Username { get; init; }

    /// <summary>Password for Elasticsearch security (leave empty for local dev).</summary>
    public string? Password { get; init; }

    /// <summary>Name of the Elasticsearch product index. Default: "products"</summary>
    public string IndexName { get; init; } = "products";

    /// <summary>
    /// Maximum edit distance for fuzzy matching.
    /// 0 = exact, 1 = one typo allowed, 2 = two typos.
    /// Default: 1 (catches most common typos without too many false positives).
    /// </summary>
    public int FuzzinessLevel { get; init; } = 1;

    /// <summary>
    /// Minimum number of characters required before fuzziness kicks in.
    /// Short words ("TV", "PC") are matched exactly to avoid noise.
    /// Default: 4
    /// </summary>
    public int PrefixLength { get; init; } = 4;
}