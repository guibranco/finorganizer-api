using FluentValidation;

namespace FinOrganizer.Application.Goals;

public sealed class CreateSavingsGoalValidator : AbstractValidator<CreateSavingsGoalRequest>
{
    public CreateSavingsGoalValidator()
    {
        RuleFor(x => x.Name).NotEmpty().MaximumLength(100);
        RuleFor(x => x.TargetAmount).GreaterThan(0);
    }
}

public sealed class UpdateSavingsGoalValidator : AbstractValidator<UpdateSavingsGoalRequest>
{
    public UpdateSavingsGoalValidator()
    {
        RuleFor(x => x.Name).NotEmpty().MaximumLength(100);
        RuleFor(x => x.TargetAmount).GreaterThan(0);
    }
}

public sealed class AddSavingsGoalContributionValidator : AbstractValidator<AddSavingsGoalContributionRequest>
{
    public AddSavingsGoalContributionValidator()
    {
        RuleFor(x => x.Amount).NotEqual(0);
    }
}
