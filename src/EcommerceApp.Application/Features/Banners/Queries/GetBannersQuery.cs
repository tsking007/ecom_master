using AutoMapper;
using EcommerceApp.Application.Features.Banners.DTOs;
using EcommerceApp.Domain.Interfaces;
using MediatR;

namespace EcommerceApp.Application.Features.Banners.Queries;

// ── Query ─────────────────────────────────────────────────────────────────────

/// <summary>
/// Returns all active banners ordered by DisplayOrder.
/// The repository filters by IsActive, StartDate, and EndDate automatically.
/// Used on the Home page carousel (BannerCarousel component, Part 28).
/// </summary>
public record GetBannersQuery : IRequest<IReadOnlyList<BannerDto>>;

// ── Handler ───────────────────────────────────────────────────────────────────

public class GetBannersQueryHandler
    : IRequestHandler<GetBannersQuery, IReadOnlyList<BannerDto>>
{
    private readonly IBannerRepository _bannerRepository;
    private readonly IMapper _mapper;

    public GetBannersQueryHandler(IBannerRepository bannerRepository, IMapper mapper)
    {
        _bannerRepository = bannerRepository;
        _mapper = mapper;
    }

    public async Task<IReadOnlyList<BannerDto>> Handle(
        GetBannersQuery query,
        CancellationToken cancellationToken)
    {
        var banners = await _bannerRepository.GetActiveBannersAsync(cancellationToken);
        return _mapper.Map<IReadOnlyList<BannerDto>>(banners);
    }
}