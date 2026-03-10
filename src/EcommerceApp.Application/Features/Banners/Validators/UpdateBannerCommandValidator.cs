using EcommerceApp.Application.Common.Validators;
using EcommerceApp.Application.Features.Banners.Commands;
using FluentValidation;

namespace EcommerceApp.Application.Features.Banners.Validators;

public class UpdateBannerCommandValidator : AbstractValidator<UpdateBannerCommand>
{
    public UpdateBannerCommandValidator()
    {
        RuleFor(x => x.BannerId)
            .ApplyGuidRules("Banner ID");

        RuleFor(x => x.Title)
            .NotEmpty().WithMessage("Banner title is required.")
            .MaximumLength(200).WithMessage("Title must not exceed 200 characters.");

        RuleFor(x => x.SubTitle)
            .MaximumLength(300)
            .When(x => x.SubTitle is not null);

        RuleFor(x => x.ImageUrl)
            .NotEmpty().WithMessage("Banner image URL is required.")
            .Must(url => Uri.TryCreate(url, UriKind.Absolute, out _))
            .WithMessage("ImageUrl must be a valid absolute URL.");

        RuleFor(x => x.LinkUrl)
            .Must(url => Uri.TryCreate(url, UriKind.Absolute, out _))
            .When(x => x.LinkUrl is not null)
            .WithMessage("LinkUrl must be a valid absolute URL.");

        RuleFor(x => x.DisplayOrder)
            .GreaterThanOrEqualTo(0).WithMessage("Display order must be 0 or greater.");

        When(x => x.StartDate.HasValue && x.EndDate.HasValue, () =>
        {
            RuleFor(x => x.EndDate!.Value)
                .GreaterThan(x => x.StartDate!.Value)
                .WithMessage("End date must be after start date.");
        });
    }
}