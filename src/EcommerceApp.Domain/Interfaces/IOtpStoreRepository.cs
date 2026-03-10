using EcommerceApp.Domain.Entities;
using EcommerceApp.Domain.Enums;

namespace EcommerceApp.Domain.Interfaces;

/// <summary>
/// Manages one-time password records.
/// Injected directly into Auth handlers — not exposed via IUnitOfWork.
/// </summary>
public interface IOtpStoreRepository : IRepository<OtpStore>
{
    /// <summary>
    /// Returns the most recently created OTP record that is:
    ///   - For the given identifier (email or phone)
    ///   - For the given purpose
    ///   - Not yet used
    ///   - Not yet expired
    /// Returns null if no valid OTP exists.
    /// </summary>
    Task<OtpStore?> GetLatestUnusedAsync(
        string identifier,
        OtpPurpose purpose,
        CancellationToken cancellationToken = default);

    /// <summary>
    /// Marks all previous unused OTPs for this identifier+purpose as used
    /// before issuing a new one.
    /// Prevents replay attacks using older valid codes.
    /// </summary>
    Task InvalidateAllPreviousAsync(
        string identifier,
        OtpPurpose purpose,
        CancellationToken cancellationToken = default);
}