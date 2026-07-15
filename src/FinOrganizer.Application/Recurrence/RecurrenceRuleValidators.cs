using FluentValidation;

namespace FinOrganizer.Application.Recurrence;

public sealed class CreateRecurrenceRuleValidator : AbstractValidator<CreateRecurrenceRuleRequest>
{
    public CreateRecurrenceRuleValidator()
    {
        RuleFor(x => x.Name).NotEmpty().MaximumLength(100);
        RuleFor(x => x.AccountId).NotEmpty();
        RuleFor(x => x.CategoryId).NotEmpty();
        RuleFor(x => x.Type).IsInEnum();
        RuleFor(x => x.Amount).GreaterThan(0);
        RuleFor(x => x.Currency).NotEmpty().Length(3);
        RuleFor(x => x.Frequency).IsInEnum();
        RuleFor(x => x.Interval).GreaterThan(0);
        RuleFor(x => x.DayOfMonth).InclusiveBetween(1, 31).When(x => x.DayOfMonth is not null);
        RuleFor(x => x.EndDate).GreaterThan(x => x.StartDate).When(x => x.EndDate is not null);
    }
}

public sealed class UpdateRecurrenceRuleValidator : AbstractValidator<UpdateRecurrenceRuleRequest>
{
    public UpdateRecurrenceRuleValidator()
    {
        RuleFor(x => x.Name).NotEmpty().MaximumLength(100);
        RuleFor(x => x.AccountId).NotEmpty();
        RuleFor(x => x.CategoryId).NotEmpty();
        RuleFor(x => x.Type).IsInEnum();
        RuleFor(x => x.Amount).GreaterThan(0);
        RuleFor(x => x.Currency).NotEmpty().Length(3);
        RuleFor(x => x.Frequency).IsInEnum();
        RuleFor(x => x.Interval).GreaterThan(0);
        RuleFor(x => x.DayOfMonth).InclusiveBetween(1, 31).When(x => x.DayOfMonth is not null);
    }
}
