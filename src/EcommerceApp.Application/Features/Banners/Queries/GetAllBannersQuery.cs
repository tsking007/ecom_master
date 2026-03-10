using AutoMapper;
using EcommerceApp.Application.Features.Banners.DTOs;
using EcommerceApp.Domain.Interfaces;
using MediatR;

namespace EcommerceApp.Application.Features.Banners.Queries;

// ── Query ─────────────────────────────────────────────────────────────────────

/// <summary>
/// Admin query — returns ALL banners including inactive ones,
/// ordered by DisplayOrder. Uses AdminBannerDto so scheduling
/// metadata is visible in the admin panel.
/// </summary>
public record GetAllBannersQuery : IRequest<IReadOnlyList<AdminBannerDto>>;

// ── Handler ───────────────────────────────────────────────────────────────────

public class GetAllBannersQueryHandler
    : IRequestHandler<GetAllBannersQuery, IReadOnlyList<AdminBannerDto>>
{
    private readonly IBannerRepository _bannerRepository;
    private readonly IMapper _mapper;

    public GetAllBannersQueryHandler(IBannerRepository bannerRepository, IMapper mapper)
    {
        _bannerRepository = bannerRepository;
        _mapper = mapper;
    }

    public async Task<IReadOnlyList<AdminBannerDto>> Handle(
        GetAllBannersQuery query,
        CancellationToken cancellationToken)
    {
        var banners = await _bannerRepository.GetAllAsync(cancellationToken);
        return _mapper.Map<IReadOnlyList<AdminBannerDto>>(banners);
    }
}