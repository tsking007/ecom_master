using EcommerceApp.Application.Common.Exceptions;
using EcommerceApp.Domain.Interfaces;
using MediatR;

namespace EcommerceApp.Application.Features.Banners.Commands;

// ── Command ───────────────────────────────────────────────────────────────────

/// <summary>
/// Soft-deletes a banner. Sets IsDeleted = true via IBannerRepository.SoftDelete().
/// The banner disappears from the public /api/banners carousel immediately.
/// </summary>
public record DeleteBannerCommand(Guid BannerId) : IRequest;

// ── Handler ───────────────────────────────────────────────────────────────────

public class DeleteBannerCommandHandler : IRequestHandler<DeleteBannerCommand>
{
    private readonly IBannerRepository _bannerRepository;

    public DeleteBannerCommandHandler(IBannerRepository bannerRepository)
    {
        _bannerRepository = bannerRepository;
    }

    public async Task Handle(
        DeleteBannerCommand command,
        CancellationToken cancellationToken)
    {
        var banner = await _bannerRepository.GetByIdAsync(command.BannerId, cancellationToken)
            ?? throw new NotFoundException("Banner", command.BannerId);

        _bannerRepository.SoftDelete(banner);
        await _bannerRepository.SaveChangesAsync(cancellationToken);
    }
}