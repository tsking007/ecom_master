using AutoMapper;

namespace EcommerceApp.Application.Common.Mappings;

/// <summary>
/// Base AutoMapper profile for the Application layer.
///
/// Contains only common type converters and mapping conventions.
/// Entity-to-DTO mappings live in each feature's own Mappings/ folder
/// (e.g. Features/Auth/Mappings/AuthMappingProfile.cs) and are discovered
/// automatically by AddAutoMapper(Assembly.GetExecutingAssembly()) in
/// DependencyInjection.cs.
///
/// DO NOT add feature-specific mappings here — keep this file thin.
/// </summary>
public class MappingProfile : Profile
{
    public MappingProfile()
    {
        // ── DateTime normalization ─────────────────────────────────────────────
        // Always store and return UTC. If a DateTime without a Kind arrives,
        // we treat it as UTC to prevent timezone confusion.

        CreateMap<DateTime, DateTime>()
            .ConvertUsing(d =>
                d.Kind == DateTimeKind.Utc
                    ? d
                    : DateTime.SpecifyKind(d, DateTimeKind.Utc));

        CreateMap<DateTime?, DateTime?>()
            .ConvertUsing(d =>
                d.HasValue
                    ? d.Value.Kind == DateTimeKind.Utc
                        ? d
                        : DateTime.SpecifyKind(d.Value, DateTimeKind.Utc)
                    : null);
    }
}