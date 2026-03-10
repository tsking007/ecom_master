//using EcommerceApp.Application.Features.Banners.DTOs;
//using EcommerceApp.Application.Features.Banners.Queries;
//using MediatR;
//using Microsoft.AspNetCore.Authorization;
//using Microsoft.AspNetCore.Mvc;

//namespace EcommerceApp.API.Controllers;

///// <summary>
///// Public banners endpoint — returns only active, in-schedule banners
///// ordered by DisplayOrder. No auth required.
///// Used by the Home page carousel (Part 28).
///// </summary>
//[ApiController]
//[Route("api/banners")]
//[AllowAnonymous]
//public class BannersController : ControllerBase
//{
//    private readonly ISender _sender;

//    public BannersController(ISender sender) => _sender = sender;

//    // ── GET /api/banners ──────────────────────────────────────────────────────
//    [HttpGet]
//    [ProducesResponseType(typeof(IReadOnlyList<BannerDto>), StatusCodes.Status200OK)]
//    public async Task<IActionResult> GetBanners(CancellationToken ct)
//        => Ok(await _sender.Send(new GetBannersQuery(), ct));
//}