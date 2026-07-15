using FluentValidation;

namespace FinOrganizer.Application.Budgets;

public sealed class CreateBudgetValidator : AbstractValidator<CreateBudgetRequest>
{
    public CreateBudgetValidator()
    {
        RuleFor(x => x.CategoryId).NotEmpty();
        RuleFor(x => x.LimitAmount).GreaterThan(0);
    }
}

public sealed class UpdateBudgetValidator : AbstractValidator<UpdateBudgetRequest>
{
    public UpdateBudgetValidator()
    {
        RuleFor(x => x.LimitAmount).GreaterThan(0);
    }
}
