using FluentValidation;

namespace EcommerceApp.Application.Common.Validators;

/// <summary>
/// Reusable FluentValidation rule builder extension methods.
/// Call these on any IRuleBuilder inside any command or query validator
/// to apply consistent validation rules project-wide.
///
/// ── Usage examples ─────────────────────────────────────────────────────────
///
///   RuleFor(x => x.Password).ApplyPasswordRules();
///   RuleFor(x => x.Email).ApplyEmailRules();
///   RuleFor(x => x.PhoneNumber).ApplyPhoneRules();
///   RuleFor(x => x.Otp).ApplyOtpRules();
///   RuleFor(x => x.Price).ApplyPriceRules("DiscountedPrice");
///   RuleFor(x => x.PageNumber).ApplyPageNumberRules();
///   RuleFor(x => x.PageSize).ApplyPageSizeRules(50);
///   RuleFor(x => x.Slug).ApplySlugRules();
///   RuleFor(x => x.ProductId).ApplyGuidRules("ProductId");
///   RuleFor(x => x.Rating).ApplyRatingRules();
/// </summary>
public static class CommonValidators
{
    // ── Password ──────────────────────────────────────────────────────────────

    /// <summary>
    /// Enforces password strength policy:
    ///   - 8 to 100 characters
    ///   - At least one uppercase letter
    ///   - At least one lowercase letter
    ///   - At least one digit
    ///   - At least one special character
    /// The 100-character maximum prevents BCrypt input limit issues.
    /// </summary>
    public static IRuleBuilderOptions<T, string> ApplyPasswordRules<T>(
        this IRuleBuilder<T, string> ruleBuilder)
    {
        return ruleBuilder
            .NotEmpty()
                .WithMessage("Password is required.")
            .MinimumLength(8)
                .WithMessage("Password must be at least 8 characters long.")
            .MaximumLength(100)
                .WithMessage("Password must not exceed 100 characters.")
            .Matches("[A-Z]")
                .WithMessage("Password must contain at least one uppercase letter.")
            .Matches("[a-z]")
                .WithMessage("Password must contain at least one lowercase letter.")
            .Matches("[0-9]")
                .WithMessage("Password must contain at least one digit (0–9).")
            .Matches(@"[!@#$%^&*()_+\-=\[\]{};':""\\|,.<>\/?]")
                .WithMessage(
                    "Password must contain at least one special character " +
                    "(!@#$%^&*()_+-=[]{}|,.<>?).");
    }

    // ── Email ─────────────────────────────────────────────────────────────────

    /// <summary>
    /// Validates email address format:
    ///   - Not empty
    ///   - Valid format per FluentValidation's built-in EmailAddress rule
    ///   - Maximum 256 characters (RFC 5321 SMTP limit)
    ///   - No consecutive dots
    /// </summary>
    public static IRuleBuilderOptions<T, string> ApplyEmailRules<T>(
        this IRuleBuilder<T, string> ruleBuilder)
    {
        return ruleBuilder
            .NotEmpty()
                .WithMessage("Email address is required.")
            .EmailAddress()
                .WithMessage("Please enter a valid email address.")
            .MaximumLength(256)
                .WithMessage("Email address must not exceed 256 characters.")
            .Must(email =>
                !string.IsNullOrEmpty(email) &&
                !email.Contains("..") &&
                !email.StartsWith('.') &&
                !email.EndsWith('.'))
                .WithMessage("Email address format is invalid.");
    }

    // ── Phone number ──────────────────────────────────────────────────────────

    /// <summary>
    /// Validates international phone numbers (E.164 standard):
    ///   - Optional leading + sign
    ///   - 7 to 15 digits total (spaces/hyphens/parentheses allowed)
    ///   - Examples: +91 9876543210, +1-800-555-0199, 07911123456
    /// </summary>
    public static IRuleBuilderOptions<T, string> ApplyPhoneRules<T>(
        this IRuleBuilder<T, string> ruleBuilder)
    {
        return ruleBuilder
            .NotEmpty()
                .WithMessage("Phone number is required.")
            .Matches(@"^\+?[\d\s\-\(\)]{7,20}$")
                .WithMessage(
                    "Phone number must be a valid format " +
                    "(e.g. +91 9876543210 or +1-800-555-0199).")
            .Must(phone =>
            {
                var digitsOnly = new string(
                    phone.Where(char.IsDigit).ToArray());
                return digitsOnly.Length >= 7 && digitsOnly.Length <= 15;
            })
                .WithMessage(
                    "Phone number must contain between 7 and 15 digits.");
    }

    // ── OTP ───────────────────────────────────────────────────────────────────

    /// <summary>
    /// Validates a 6-digit numeric OTP as sent by the OtpService.
    /// </summary>
    public static IRuleBuilderOptions<T, string> ApplyOtpRules<T>(
        this IRuleBuilder<T, string> ruleBuilder)
    {
        return ruleBuilder
            .NotEmpty()
                .WithMessage("OTP is required.")
            .Length(6)
                .WithMessage("OTP must be exactly 6 digits.")
            .Matches(@"^\d{6}$")
                .WithMessage("OTP must contain digits only.");
    }

    // ── Slug ──────────────────────────────────────────────────────────────────

    /// <summary>
    /// Validates a URL slug:
    ///   - Lowercase letters, digits, and hyphens only
    ///   - No leading or trailing hyphens
    ///   - No consecutive hyphens
    ///   - Maximum 300 characters
    ///   - Example: "apple-iphone-15-pro-128gb"
    /// </summary>
    public static IRuleBuilderOptions<T, string> ApplySlugRules<T>(
        this IRuleBuilder<T, string> ruleBuilder)
    {
        return ruleBuilder
            .NotEmpty()
                .WithMessage("Slug is required.")
            .MaximumLength(300)
                .WithMessage("Slug must not exceed 300 characters.")
            .Matches(@"^[a-z0-9]+(?:-[a-z0-9]+)*$")
                .WithMessage(
                    "Slug must contain only lowercase letters, digits, " +
                    "and hyphens. It must not start or end with a hyphen, " +
                    "and must not contain consecutive hyphens.");
    }

    // ── Price ─────────────────────────────────────────────────────────────────

    /// <summary>
    /// Validates a monetary price:
    ///   - Greater than zero
    ///   - Maximum 9,999,999.99
    ///   - At most 2 decimal places
    /// </summary>
    public static IRuleBuilderOptions<T, decimal> ApplyPriceRules<T>(
        this IRuleBuilder<T, decimal> ruleBuilder,
        string fieldName = "Price")
    {
        return ruleBuilder
            .GreaterThan(0)
                .WithMessage($"{fieldName} must be greater than 0.")
            .LessThanOrEqualTo(9_999_999.99m)
                .WithMessage($"{fieldName} must not exceed 9,999,999.99.")
            .Must(p => decimal.Round(p, 2) == p)
                .WithMessage(
                    $"{fieldName} must have at most 2 decimal places.");
    }

    // ── Nullable price (e.g. DiscountedPrice) ─────────────────────────────────

    /// <summary>
    /// Validates an optional price. Skips all rules if null.
    /// </summary>
    public static IRuleBuilderOptions<T, decimal?> ApplyOptionalPriceRules<T>(
        this IRuleBuilder<T, decimal?> ruleBuilder,
        string fieldName = "Price")
    {
        return ruleBuilder
            .GreaterThan(0)
                .WithMessage($"{fieldName} must be greater than 0.")
                .When(x => ruleBuilder != null)     // only validate when not null
            .LessThanOrEqualTo(9_999_999.99m)
                .WithMessage($"{fieldName} must not exceed 9,999,999.99.")
            .Must(p => p == null || decimal.Round(p.Value, 2) == p.Value)
                .WithMessage(
                    $"{fieldName} must have at most 2 decimal places.");
    }

    // ── Pagination ────────────────────────────────────────────────────────────

    /// <summary>
    /// Validates a 1-based page number (minimum 1).
    /// </summary>
    public static IRuleBuilderOptions<T, int> ApplyPageNumberRules<T>(
        this IRuleBuilder<T, int> ruleBuilder)
    {
        return ruleBuilder
            .GreaterThanOrEqualTo(1)
                .WithMessage("Page number must be at least 1.");
    }

    /// <summary>
    /// Validates a page size between 1 and maxPageSize (default 100).
    /// </summary>
    public static IRuleBuilderOptions<T, int> ApplyPageSizeRules<T>(
        this IRuleBuilder<T, int> ruleBuilder,
        int maxPageSize = 100)
    {
        return ruleBuilder
            .InclusiveBetween(1, maxPageSize)
                .WithMessage(
                    $"Page size must be between 1 and {maxPageSize}.");
    }

    // ── Rating ────────────────────────────────────────────────────────────────

    /// <summary>
    /// Validates a 1-to-5 star product rating.
    /// </summary>
    public static IRuleBuilderOptions<T, int> ApplyRatingRules<T>(
        this IRuleBuilder<T, int> ruleBuilder)
    {
        return ruleBuilder
            .InclusiveBetween(1, 5)
                .WithMessage("Rating must be between 1 and 5 stars.");
    }

    // ── Guid ──────────────────────────────────────────────────────────────────

    /// <summary>
    /// Validates that a Guid is not empty (i.e. not all zeros).
    /// </summary>
    public static IRuleBuilderOptions<T, Guid> ApplyGuidRules<T>(
        this IRuleBuilder<T, Guid> ruleBuilder,
        string fieldName = "Id")
    {
        return ruleBuilder
            .NotEmpty()
                .WithMessage(
                    $"{fieldName} must be a valid non-empty identifier.");
    }

    // ── Stock quantity ────────────────────────────────────────────────────────

    /// <summary>
    /// Validates a stock quantity (non-negative, reasonable upper bound).
    /// </summary>
    public static IRuleBuilderOptions<T, int> ApplyStockQuantityRules<T>(
        this IRuleBuilder<T, int> ruleBuilder)
    {
        return ruleBuilder
            .GreaterThanOrEqualTo(0)
                .WithMessage("Stock quantity cannot be negative.")
            .LessThanOrEqualTo(1_000_000)
                .WithMessage("Stock quantity must not exceed 1,000,000.");
    }

    // ── Cart item quantity ────────────────────────────────────────────────────

    /// <summary>
    /// Validates a cart item quantity (1 to 99 per line item).
    /// </summary>
    public static IRuleBuilderOptions<T, int> ApplyCartQuantityRules<T>(
        this IRuleBuilder<T, int> ruleBuilder)
    {
        return ruleBuilder
            .InclusiveBetween(1, 99)
                .WithMessage("Quantity must be between 1 and 99.");
    }
}