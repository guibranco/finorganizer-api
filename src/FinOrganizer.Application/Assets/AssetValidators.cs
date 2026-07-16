using FluentValidation;

namespace FinOrganizer.Application.Assets;

public sealed class CreateAssetValidator : AbstractValidator<CreateAssetRequest>
{
    public CreateAssetValidator()
    {
        RuleFor(x => x.Ticker).NotEmpty().MaximumLength(20);
        RuleFor(x => x.Name).NotEmpty().MaximumLength(150);
        RuleFor(x => x.Currency).NotEmpty().Length(3);
        RuleFor(x => x.Class).IsInEnum();
    }
}

public sealed class UpdateAssetValidator : AbstractValidator<UpdateAssetRequest>
{
    public UpdateAssetValidator()
    {
        RuleFor(x => x.Ticker).NotEmpty().MaximumLength(20);
        RuleFor(x => x.Name).NotEmpty().MaximumLength(150);
        RuleFor(x => x.Currency).NotEmpty().Length(3);
        RuleFor(x => x.Class).IsInEnum();
    }
}

public sealed class CreateAssetEventValidator : AbstractValidator<CreateAssetEventRequest>
{
    public CreateAssetEventValidator()
    {
        RuleFor(x => x.AssetId).NotEmpty();
        RuleFor(x => x.AccountId).NotEmpty();
        RuleFor(x => x.Type).IsInEnum();
        RuleFor(x => x.Quantity).GreaterThan(0)
            .When(x => x.Type is Domain.Enums.AssetEventType.Buy or Domain.Enums.AssetEventType.Sell);
        RuleFor(x => x.UnitPrice).GreaterThanOrEqualTo(0);
        RuleFor(x => x.Fees).GreaterThanOrEqualTo(0);
    }
}

public sealed class UpdateAssetEventValidator : AbstractValidator<UpdateAssetEventRequest>
{
    public UpdateAssetEventValidator()
    {
        RuleFor(x => x.AssetId).NotEmpty();
        RuleFor(x => x.AccountId).NotEmpty();
        RuleFor(x => x.Type).IsInEnum();
        RuleFor(x => x.Quantity).GreaterThan(0)
            .When(x => x.Type is Domain.Enums.AssetEventType.Buy or Domain.Enums.AssetEventType.Sell);
        RuleFor(x => x.UnitPrice).GreaterThanOrEqualTo(0);
        RuleFor(x => x.Fees).GreaterThanOrEqualTo(0);
    }
}

public sealed class UpsertAssetPriceSnapshotValidator : AbstractValidator<UpsertAssetPriceSnapshotRequest>
{
    public UpsertAssetPriceSnapshotValidator()
    {
        RuleFor(x => x.AssetId).NotEmpty();
        RuleFor(x => x.Price).GreaterThan(0);
    }
}
