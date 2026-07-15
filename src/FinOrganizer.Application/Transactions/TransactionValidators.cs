using FluentValidation;

namespace FinOrganizer.Application.Transactions;

public sealed class CreateTransactionValidator : AbstractValidator<CreateTransactionRequest>
{
    public CreateTransactionValidator()
    {
        RuleFor(x => x.AccountId).NotEmpty();
        RuleFor(x => x.Type).IsInEnum();
        RuleFor(x => x.Amount).GreaterThan(0);
        RuleFor(x => x.Currency).NotEmpty().Length(3);
        RuleFor(x => x.CounterpartyAccountId)
            .NotNull()
            .NotEqual(x => x.AccountId)
            .When(x => x.Type == Domain.Enums.TransactionType.Transfer)
            .WithMessage("A transfer requires a counterparty account different from the source account.");
    }
}

public sealed class UpdateTransactionValidator : AbstractValidator<UpdateTransactionRequest>
{
    public UpdateTransactionValidator()
    {
        RuleFor(x => x.AccountId).NotEmpty();
        RuleFor(x => x.Type).IsInEnum();
        RuleFor(x => x.Amount).GreaterThan(0);
        RuleFor(x => x.Currency).NotEmpty().Length(3);
        RuleFor(x => x.CounterpartyAccountId)
            .NotNull()
            .NotEqual(x => x.AccountId)
            .When(x => x.Type == Domain.Enums.TransactionType.Transfer)
            .WithMessage("A transfer requires a counterparty account different from the source account.");
    }
}
