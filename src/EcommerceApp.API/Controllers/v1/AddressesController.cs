using EcommerceApp.Application.Features.Addresses.Commands;
using EcommerceApp.Application.Features.Addresses.DTOs;
using EcommerceApp.Application.Features.Addresses.Queries;
using MediatR;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.RateLimiting;

namespace EcommerceApp.API.Controllers.v1;

[ApiController]
[Route("api/v1/addresses")]
[Authorize]
[EnableRateLimiting("general")]
public class AddressesController : ControllerBase
{
    private readonly ISender _sender;

    public AddressesController(ISender sender)
    {
        _sender = sender;
    }

    /// <summary>
    /// GET api/v1/addresses
    /// Returns all saved addresses for the authenticated user.
    /// Default address is always first in the list.
    /// </summary>
    [HttpGet]
    [ProducesResponseType(typeof(IReadOnlyList<AddressDto>), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    public async Task<ActionResult<IReadOnlyList<AddressDto>>> GetAddresses(
        CancellationToken cancellationToken)
    {
        var result = await _sender.Send(new GetAddressesQuery(), cancellationToken);
        return Ok(result);
    }

    /// <summary>
    /// POST api/v1/addresses
    /// Adds a new address to the user's saved addresses.
    /// If this is the first address, it is automatically set as default.
    /// </summary>
    [HttpPost]
    [ProducesResponseType(typeof(AddressDto), StatusCodes.Status201Created)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    public async Task<ActionResult<AddressDto>> AddAddress(
        [FromBody] AddAddressRequest request,
        CancellationToken cancellationToken)
    {
        var command = new AddAddressCommand(
            request.FullName,
            request.PhoneNumber,
            request.AddressLine1,
            request.AddressLine2,
            request.City,
            request.State,
            request.PostalCode,
            request.Country,
            request.AddressType,
            request.IsDefault);

        var result = await _sender.Send(command, cancellationToken);

        return CreatedAtAction(
            nameof(GetAddresses),
            new { },
            result);
    }

    /// <summary>
    /// PUT api/v1/addresses/{addressId}
    /// Updates all fields of an existing address.
    /// </summary>
    [HttpPut("{addressId:guid}")]
    [ProducesResponseType(typeof(AddressDto), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<ActionResult<AddressDto>> UpdateAddress(
        Guid addressId,
        [FromBody] UpdateAddressRequest request,
        CancellationToken cancellationToken)
    {
        var command = new UpdateAddressCommand(
            addressId,
            request.FullName,
            request.PhoneNumber,
            request.AddressLine1,
            request.AddressLine2,
            request.City,
            request.State,
            request.PostalCode,
            request.Country,
            request.AddressType,
            request.IsDefault);

        var result = await _sender.Send(command, cancellationToken);
        return Ok(result);
    }

    /// <summary>
    /// DELETE api/v1/addresses/{addressId}
    /// Soft-deletes an address. If it was the default, the next oldest
    /// address is automatically promoted to default.
    /// </summary>
    [HttpDelete("{addressId:guid}")]
    [ProducesResponseType(StatusCodes.Status204NoContent)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<IActionResult> DeleteAddress(
        Guid addressId,
        CancellationToken cancellationToken)
    {
        await _sender.Send(new DeleteAddressCommand(addressId), cancellationToken);
        return NoContent();
    }

    /// <summary>
    /// PATCH api/v1/addresses/{addressId}/set-default
    /// Promotes the given address to default and demotes the previous default.
    /// Use this from the checkout screen's "Change address" UI.
    /// </summary>
    [HttpPatch("{addressId:guid}/set-default")]
    [ProducesResponseType(typeof(AddressDto), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<ActionResult<AddressDto>> SetDefault(
        Guid addressId,
        CancellationToken cancellationToken)
    {
        var result = await _sender.Send(
            new SetDefaultAddressCommand(addressId), cancellationToken);
        return Ok(result);
    }

    // -------------------------------------------------------------------------
    // Request models (kept here since they are controller-specific)
    // -------------------------------------------------------------------------

    public class AddAddressRequest
    {
        public string FullName { get; set; } = string.Empty;
        public string PhoneNumber { get; set; } = string.Empty;
        public string AddressLine1 { get; set; } = string.Empty;
        public string? AddressLine2 { get; set; }
        public string City { get; set; } = string.Empty;
        public string State { get; set; } = string.Empty;
        public string PostalCode { get; set; } = string.Empty;
        public string Country { get; set; } = string.Empty;
        public string AddressType { get; set; } = "Home";
        public bool IsDefault { get; set; } = false;
    }

    public class UpdateAddressRequest
    {
        public string FullName { get; set; } = string.Empty;
        public string PhoneNumber { get; set; } = string.Empty;
        public string AddressLine1 { get; set; } = string.Empty;
        public string? AddressLine2 { get; set; }
        public string City { get; set; } = string.Empty;
        public string State { get; set; } = string.Empty;
        public string PostalCode { get; set; } = string.Empty;
        public string Country { get; set; } = string.Empty;
        public string AddressType { get; set; } = "Home";
        public bool IsDefault { get; set; } = false;
    }
}