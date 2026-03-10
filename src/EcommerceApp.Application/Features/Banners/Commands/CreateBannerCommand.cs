using AutoMapper;
using EcommerceApp.Application.Features.Banners.DTOs;
using EcommerceApp.Domain.Entities;
using EcommerceApp.Domain.Interfaces;
using MediatR;

namespace EcommerceApp.Application.Features.Banners.Commands;

// ── Command ───────────────────────────────────────────────────────────────────

public record CreateBannerCommand(
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

public class CreateBannerCommandHandler
    : IRequestHandler<CreateBannerCommand, AdminBannerDto>
{
    private readonly IBannerRepository _bannerRepository;
    private readonly IMapper _mapper;

    public CreateBannerCommandHandler(IBannerRepository bannerRepository, IMapper mapper)
    {
        _bannerRepository = bannerRepository;
        _mapper = mapper;
    }

    public async Task<AdminBannerDto> Handle(
        CreateBannerCommand command,
        CancellationToken cancellationToken)
    {
        var banner = new Banner
        {
            Title = command.Title.Trim(),
            SubTitle = command.SubTitle?.Trim(),
            ImageUrl = command.ImageUrl.Trim(),
            LinkUrl = command.LinkUrl?.Trim(),
            DisplayOrder = command.DisplayOrder,
            IsActive = command.IsActive,
            StartDate = command.StartDate,
            EndDate = command.EndDate
        };

        await _bannerRepository.AddAsync(banner, cancellationToken);
        await _bannerRepository.SaveChangesAsync(cancellationToken);

        return _mapper.Map<AdminBannerDto>(banner);
    }
}