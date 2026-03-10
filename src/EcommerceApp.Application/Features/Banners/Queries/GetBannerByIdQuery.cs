using AutoMapper;
using EcommerceApp.Application.Common.Exceptions;
using EcommerceApp.Application.Features.Banners.DTOs;
using EcommerceApp.Domain.Interfaces;
using MediatR;

namespace EcommerceApp.Application.Features.Banners.Queries;

// ── Query ─────────────────────────────────────────────────────────────────────

public record GetBannerByIdQuery(Guid BannerId) : IRequest<AdminBannerDto>;

// ── Handler ───────────────────────────────────────────────────────────────────

public class GetBannerByIdQueryHandler
    : IRequestHandler<GetBannerByIdQuery, AdminBannerDto>
{
    private readonly IBannerRepository _bannerRepository;
    private readonly IMapper _mapper;

    public GetBannerByIdQueryHandler(IBannerRepository bannerRepository, IMapper mapper)
    {
        _bannerRepository = bannerRepository;
        _mapper = mapper;
    }

    public async Task<AdminBannerDto> Handle(
        GetBannerByIdQuery query,
        CancellationToken cancellationToken)
    {
        var banner = await _bannerRepository.GetByIdAsync(query.BannerId, cancellationToken)
            ?? throw new NotFoundException("Banner", query.BannerId);

        return _mapper.Map<AdminBannerDto>(banner);
    }
}