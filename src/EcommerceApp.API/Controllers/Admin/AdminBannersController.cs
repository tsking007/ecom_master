//using EcommerceApp.API.Models.Requests.Banners;
//using EcommerceApp.Application.Features.Banners.Commands;
//using EcommerceApp.Application.Features.Banners.DTOs;
//using EcommerceApp.Application.Features.Banners.Queries;
//using MediatR;
//using Microsoft.AspNetCore.Authorization;
//using Microsoft.AspNetCore.Mvc;

//namespace EcommerceApp.API.Controllers.Admin;

///// <summary>
///// Admin banner management — full CRUD with scheduling support.
///// All endpoints require the "Admin" role.
///// Unlike /api/banners, this returns AdminBannerDto which includes
///// IsActive, StartDate, EndDate, and audit timestamps.
///// </summary>
//[ApiController]
//[Route("api/admin/banners")]
//[Authorize(Roles = "Admin")]
//public class AdminBannersController : ControllerBase
//{
//    private readonly ISender _sender;

//    public AdminBannersController(ISender sender) => _sender = sender;

//    // ── GET /api/admin/banners ────────────────────────────────────────────────
//    [HttpGet]
//    [ProducesResponseType(typeof(IReadOnlyList<AdminBannerDto>), StatusCodes.Status200OK)]
//    public async Task<IActionResult> GetAll(CancellationToken ct)
//        => Ok(await _sender.Send(new GetAllBannersQuery(), ct));

//    // ── GET /api/admin/banners/{id} ───────────────────────────────────────────
//    [HttpGet("{id:guid}")]
//    [ProducesResponseType(typeof(AdminBannerDto), StatusCodes.Status200OK)]
//    [ProducesResponseType(StatusCodes.Status404NotFound)]
//    public async Task<IActionResult> GetById(Guid id, CancellationToken ct)
//        => Ok(await _sender.Send(new GetBannerByIdQuery(id), ct));

//    // ── POST /api/admin/banners ───────────────────────────────────────────────
//    [HttpPost]
//    [ProducesResponseType(typeof(AdminBannerDto), StatusCodes.Status201Created)]
//    [ProducesResponseType(StatusCodes.Status400BadRequest)]
//    public async Task<IActionResult> Create(
//        [FromBody] CreateBannerRequest req,
//        CancellationToken ct)
//    {
//        var command = new CreateBannerCommand(
//            Title: req.Title,
//            SubTitle: req.SubTitle,
//            ImageUrl: req.ImageUrl,
//            LinkUrl: req.LinkUrl,
//            DisplayOrder: req.DisplayOrder,
//            IsActive: req.IsActive,
//            StartDate: req.StartDate,
//            EndDate: req.EndDate);

//        var result = await _sender.Send(command, ct);

//        return CreatedAtAction(
//            nameof(GetById),
//            new { id = result.Id },
//            result);
//    }

//    // ── PUT /api/admin/banners/{id} ───────────────────────────────────────────
//    [HttpPut("{id:guid}")]
//    [ProducesResponseType(typeof(AdminBannerDto), StatusCodes.Status200OK)]
//    [ProducesResponseType(StatusCodes.Status400BadRequest)]
//    [ProducesResponseType(StatusCodes.Status404NotFound)]
//    public async Task<IActionResult> Update(
//        Guid id,
//        [FromBody] UpdateBannerRequest req,
//        CancellationToken ct)
//    {
//        var command = new UpdateBannerCommand(
//            BannerId: id,
//            Title: req.Title,
//            SubTitle: req.SubTitle,
//            ImageUrl: req.ImageUrl,
//            LinkUrl: req.LinkUrl,
//            DisplayOrder: req.DisplayOrder,
//            IsActive: req.IsActive,
//            StartDate: req.StartDate,
//            EndDate: req.EndDate);

//        return Ok(await _sender.Send(command, ct));
//    }

//    // ── DELETE /api/admin/banners/{id} ────────────────────────────────────────
//    [HttpDelete("{id:guid}")]
//    [ProducesResponseType(StatusCodes.Status204NoContent)]
//    [ProducesResponseType(StatusCodes.Status404NotFound)]
//    public async Task<IActionResult> Delete(Guid id, CancellationToken ct)
//    {
//        await _sender.Send(new DeleteBannerCommand(id), ct);
//        return NoContent();
//    }
//}