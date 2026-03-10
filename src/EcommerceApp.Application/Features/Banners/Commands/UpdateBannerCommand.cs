using AutoMapper;
using EcommerceApp.Application.Common.Exceptions;
using EcommerceApp.Application.Features.Banners.DTOs;
using EcommerceApp.Domain.Interfaces;
using MediatR;

namespace EcommerceApp.Application.Features.Banners.Commands;

// ── Command ───────────────────────────────────────────────────────────────────

public record UpdateBannerCommand(
    Guid BannerId,
    string Title,
    string? SubTitle,
    string ImageUrl,
    string? LinkUrl,
    int DisplayOrder,
    bool IsActive,
    DateTime? StartDate,
    DateTime? EndDate
) : IRequest<AdminBannerDto>;

// ── Handler ───────────────────────────────────────────────────────────────────

public class UpdateBannerCommandHandler
    : IRequestHandler<UpdateBannerCommand, AdminBannerDto>
{
    private readonly IBannerRepository _bannerRepository;
    private readonly IMapper _mapper;

    public UpdateBannerCommandHandler(IBannerRepository bannerRepository, IMapper mapper)
    {
        _bannerRepository = bannerRepository;
        _mapper = mapper;
    }

    public async Task<AdminBannerDto> Handle(
        UpdateBannerCommand command,
        CancellationToken cancellationToken)
    {
        var banner = await _bannerRepository.GetByIdAsync(command.BannerId, cancellationToken)
            ?? throw new NotFoundException("Banner", command.BannerId);

        banner.Title = command.Title.Trim();
        banner.SubTitle = command.SubTitle?.Trim();
        banner.ImageUrl = command.ImageUrl.Trim();
        banner.LinkUrl = command.LinkUrl?.Trim();
        banner.DisplayOrder = command.DisplayOrder;
        banner.IsActive = command.IsActive;
        banner.StartDate = command.StartDate;
        banner.EndDate = command.EndDate;
        banner.UpdatedAt = DateTime.UtcNow;

        _bannerRepository.Update(banner);
        await _bannerRepository.SaveChangesAsync(cancellationToken);

        return _mapper.Map<AdminBannerDto>(banner);
    }
}