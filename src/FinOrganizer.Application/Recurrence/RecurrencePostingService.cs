using FinOrganizer.Application.Common.Interfaces;
using FinOrganizer.Domain.Entities;
using FinOrganizer.Domain.Enums;
using FinOrganizer.Domain.Services;
using Microsoft.EntityFrameworkCore;

namespace FinOrganizer.Application.Recurrence;

public sealed class RecurrencePostingService(IApplicationDbContext db) : IRecurrencePostingService
{
    public async Task<int> PostDueOccurrencesAsync(DateOnly asOf, CancellationToken cancellationToken = default)
    {
        var dueRules = await db.RecurrenceRules
            .Where(r => r.IsActive && r.NextDueDate <= asOf)
            .ToListAsync(cancellationToken);

        var materialized = 0;

        foreach (var rule in dueRules)
        {
            while (rule.IsActive && rule.NextDueDate <= asOf)
            {
                var dueDate = rule.NextDueDate;

                var alreadyExists = await db.RecurrenceOccurrences.AnyAsync(
                    o => o.RecurrenceRuleId == rule.Id && o.DueDate == dueDate, cancellationToken);

                if (!alreadyExists)
                {
                    var occurrence = new RecurrenceOccurrence
                    {
                        RecurrenceRuleId = rule.Id,
                        DueDate = dueDate,
                        Amount = rule.Amount,
                        Currency = rule.Currency,
                    };

                    if (rule.AutoPost)
                    {
                        var transaction = new Transaction
                        {
                            AccountId = rule.AccountId,
                            Type = rule.Type,
                            Amount = rule.Amount,
                            Currency = rule.Currency,
                            Date = dueDate,
                            Description = rule.Name,
                            CategoryId = rule.CategoryId,
                            RecurrenceId = rule.Id,
                        };

                        db.Transactions.Add(transaction);
                        occurrence.Status = RecurrenceOccurrenceStatus.Posted;
                        occurrence.PostedTransactionId = transaction.Id;
                    }

                    db.RecurrenceOccurrences.Add(occurrence);
                    materialized++;
                }

                var next = RecurrenceDateCalculator.GetNextDueDate(rule, dueDate);
                rule.NextDueDate = next;

                if (rule.EndDate is { } endDate && next > endDate)
                {
                    rule.IsActive = false;
                    break;
                }
            }
        }

        if (dueRules.Count > 0)
        {
            await db.SaveChangesAsync(cancellationToken);
        }

        return materialized;
    }
}
