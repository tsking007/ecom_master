using EcommerceApp.Application.Common;
using EcommerceApp.Application.Features.Search.DTOs;   
using EcommerceApp.Application.Interfaces;              
using EcommerceApp.Domain.Common;
using EcommerceApp.Domain.Interfaces;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using Nest;

namespace EcommerceApp.Infrastructure.Search;

/// <summary>
/// Elasticsearch implementation of ISearchService using NEST 7.x.
/// </summary>
public class ElasticsearchSearchService : ISearchService
{
    private readonly IElasticClient _client;
    private readonly SearchSettings _settings;
    private readonly IUnitOfWork _unitOfWork;
    private readonly ILogger<ElasticsearchSearchService> _logger;

    private sealed class EsProductDoc
    {
        public Guid Id { get; set; }
        public string Name { get; set; } = string.Empty;
        public string Slug { get; set; } = string.Empty;
        public string? ShortDescription { get; set; }
        public string? Brand { get; set; }
        public decimal Price { get; set; }
        public decimal? DiscountedPrice { get; set; }
        public decimal EffectivePrice { get; set; }
        public string Category { get; set; } = string.Empty;
        public string? SubCategory { get; set; }
        public string[] Tags { get; set; } = Array.Empty<string>();
        public string? MainImageUrl { get; set; }
        public double AverageRating { get; set; }
        public int ReviewCount { get; set; }
        public int SoldCount { get; set; }
        public bool IsActive { get; set; }
    }

    public ElasticsearchSearchService(
        IElasticClient client,
        IOptions<SearchSettings> settings,
        IUnitOfWork unitOfWork,
        ILogger<ElasticsearchSearchService> logger)
    {
        _client = client;
        _settings = settings.Value;
        _unitOfWork = unitOfWork;
        _logger = logger;
    }

    // ── Index creation ────────────────────────────────────────────────────────

    public async Task EnsureIndexExistsAsync(CancellationToken ct = default)
    {
        var exists = await _client.Indices.ExistsAsync(_settings.IndexName, ct: ct);
        if (exists.Exists) return;

        var createResponse = await _client.Indices.CreateAsync(_settings.IndexName, c => c
            .Map<EsProductDoc>(m => m
                .Properties(p => p
                    .Keyword(k => k.Name(n => n.Id))
                    .Text(t => t.Name(n => n.Name).Boost(5))
                    .Text(t => t.Name(n => n.Brand).Boost(3))
                    .Text(t => t.Name(n => n.Category).Boost(2))
                    .Text(t => t.Name(n => n.SubCategory))
                    .Text(t => t.Name(n => n.ShortDescription))
                    .Text(t => t.Name(n => n.Tags))
                    .Keyword(k => k.Name(n => n.Slug))
                    .Number(n => n.Name(f => f.Price).Type(NumberType.Double))
                    .Number(n => n.Name(f => f.DiscountedPrice).Type(NumberType.Double))
                    .Number(n => n.Name(f => f.EffectivePrice).Type(NumberType.Double))
                    .Number(n => n.Name(f => f.AverageRating).Type(NumberType.Double))
                    .Number(n => n.Name(f => f.ReviewCount).Type(NumberType.Integer))
                    .Number(n => n.Name(f => f.SoldCount).Type(NumberType.Integer))
                    .Boolean(b => b.Name(n => n.IsActive))
                    .Keyword(k => k.Name(n => n.MainImageUrl))
                )), ct);

        if (!createResponse.IsValid)
            _logger.LogError(
                "Failed to create Elasticsearch index '{Index}': {Error}",
                _settings.IndexName, createResponse.DebugInformation);
        else
            _logger.LogInformation(
                "Elasticsearch index '{Index}' created successfully.",
                _settings.IndexName);
    }

    // ── Search ────────────────────────────────────────────────────────────────

    public async Task<PagedResult<SearchResultDto>> SearchAsync(
        string term,
        int page,
        int pageSize,
        CancellationToken cancellationToken = default)
    {
        

        var from = (page - 1) * pageSize;

        var response = await _client.SearchAsync<EsProductDoc>(s => s
            .Index(_settings.IndexName)
            .From(from)
            .Size(pageSize)
            .Query(q => q
                .Bool(b => b
                    .Filter(f => f
                        .Term(t => t.Field(d => d.IsActive).Value(true)))
                    .Must(m => m
                        .MultiMatch(mm => mm
                            .Fields(f => f
                                .Field(d => d.Name, boost: 5)
                                .Field(d => d.Brand, boost: 3)
                                .Field(d => d.Category, boost: 2)
                                .Field(d => d.SubCategory)
                                .Field(d => d.ShortDescription)
                                .Field(d => d.Tags))
                            .Query(term)
                            .Type(TextQueryType.BestFields)
                            .Fuzziness(Fuzziness.Auto)
                            .PrefixLength(1)
                            .Operator(Operator.Or)))))
            .Sort(so => so
                .Descending(SortSpecialField.Score)
                .Descending(d => d.SoldCount)),
            cancellationToken);

        if (!response.IsValid)
        {
            _logger.LogError(
                "Elasticsearch search failed for term '{Term}': {Error}",
                term, response.DebugInformation);

            return PagedResult<SearchResultDto>.Empty(page, pageSize);
        }

        var items = response.Hits
            .Select(h => new SearchResultDto
            {
                Id = h.Source.Id,
                Name = h.Source.Name,
                Slug = h.Source.Slug,
                ShortDescription = h.Source.ShortDescription,
                Brand = h.Source.Brand,
                Price = h.Source.Price,
                DiscountedPrice = h.Source.DiscountedPrice,
                EffectivePrice = h.Source.EffectivePrice,
                Category = h.Source.Category,
                SubCategory = h.Source.SubCategory,
                MainImageUrl = h.Source.MainImageUrl,
                AverageRating = h.Source.AverageRating,
                ReviewCount = h.Source.ReviewCount,
                SoldCount = h.Source.SoldCount,
                IsActive = h.Source.IsActive,
                Score = h.Score
            })
            .ToList();

        return PagedResult<SearchResultDto>.Create(
            items,
            (int)response.Total,
            page,
            pageSize);
    }

    // ── Index management ──────────────────────────────────────────────────────

    public async Task UpsertProductAsync(
        SearchProductDocument product,
        CancellationToken cancellationToken = default)
    {
        var doc = ToEsDoc(product);

        var response = await _client.IndexAsync(doc, i => i
            .Index(_settings.IndexName)
            .Id(doc.Id.ToString()),
            cancellationToken);

        if (!response.IsValid)
            _logger.LogError(
                "Failed to index product {ProductId}: {Error}",
                product.Id, response.DebugInformation);
    }

    public async Task DeleteProductAsync(
        Guid productId,
        CancellationToken cancellationToken = default)
    {
        var response = await _client.DeleteAsync<EsProductDoc>(
            productId.ToString(),
            d => d.Index(_settings.IndexName),
            cancellationToken);

        // ↓ FIX: use Nest.Result to avoid ambiguity with Domain.Common.Result
        if (!response.IsValid && response.Result != Nest.Result.NotFound)
            _logger.LogError(
                "Failed to delete product {ProductId} from Elasticsearch: {Error}",
                productId, response.DebugInformation);
    }

    public async Task SyncAllAsync(CancellationToken cancellationToken = default)
    {
        _logger.LogInformation("Starting full Elasticsearch product sync...");

        await EnsureIndexExistsAsync(cancellationToken);

        var paged = await _unitOfWork.Products.GetPagedAsync(
            pageNumber: 1,
            pageSize: 10_000,
            category: null,
            subCategory: null,
            minPrice: null,
            maxPrice: null,
            minRating: null,
            brand: null,
            sortBy: null,
            sortDescending: false,
            isActive: null,
            cancellationToken: cancellationToken);

        if (!paged.Items.Any())
        {
            _logger.LogInformation("No products to sync.");
            return;
        }

        var docs = paged.Items
            .Select(p => ToEsDoc(new SearchProductDocument(
                Id: p.Id,
                Name: p.Name,
                Slug: p.Slug,
                ShortDescription: p.ShortDescription,
                Brand: p.Brand,
                Price: p.Price,
                DiscountedPrice: p.DiscountedPrice,
                Category: p.Category,
                SubCategory: p.SubCategory,
                Tags: p.Tags,
                MainImageUrl: p.ImageUrls.FirstOrDefault(),
                AverageRating: p.AverageRating,
                ReviewCount: p.ReviewCount,
                SoldCount: p.SoldCount,
                IsActive: p.IsActive)))
            .ToList();

        var bulkResponse = await _client.BulkAsync(b => b
            .Index(_settings.IndexName)
            .IndexMany(docs, (descriptor, doc) =>
                descriptor.Id(doc.Id.ToString())),
            cancellationToken);

        if (bulkResponse.Errors)
            foreach (var item in bulkResponse.ItemsWithErrors)
                _logger.LogError(
                    "Bulk index error for doc {Id}: {Error}",
                    item.Id, item.Error?.Reason);

        _logger.LogInformation(
            "Elasticsearch sync complete. {Count} products indexed.", docs.Count);
    }

    public async Task<bool> IsHealthyAsync(
        CancellationToken cancellationToken = default)
    {
        var response = await _client.Cluster.HealthAsync(ct: cancellationToken);
        return response.IsValid;
    }

    // ── Private helpers ───────────────────────────────────────────────────────

    private static EsProductDoc ToEsDoc(SearchProductDocument p) => new()
    {
        Id = p.Id,
        Name = p.Name,
        Slug = p.Slug,
        ShortDescription = p.ShortDescription,
        Brand = p.Brand,
        Price = p.Price,
        DiscountedPrice = p.DiscountedPrice,
        EffectivePrice = p.DiscountedPrice ?? p.Price,
        Category = p.Category,
        SubCategory = p.SubCategory,
        Tags = p.Tags.ToArray(),
        MainImageUrl = p.MainImageUrl,
        AverageRating = p.AverageRating,
        ReviewCount = p.ReviewCount,
        SoldCount = p.SoldCount,
        IsActive = p.IsActive
    };
}