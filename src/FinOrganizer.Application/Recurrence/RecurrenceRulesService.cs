using FinOrganizer.Application.Common.Exceptions;
using FinOrganizer.Application.Common.Interfaces;
using FinOrganizer.Domain.Entities;
using FinOrganizer.Domain.Enums;
using FinOrganizer.Domain.Services;
using FluentValidation;
using Microsoft.EntityFrameworkCore;

namespace FinOrganizer.Application.Recurrence;

public sealed class RecurrenceRulesService(IApplicationDbContext db) : IRecurrenceRulesService
{
    public async Task<List<RecurrenceRuleDto>> GetAllAsync(CancellationToken cancellationToken = default)
        => await db.RecurrenceRules.OrderBy(r => r.NextDueDate).Select(ToDtoExpression).ToListAsync(cancellationToken);

    public async Task<RecurrenceRuleDto> GetByIdAsync(Guid id, CancellationToken cancellationToken = default)
        => ToDto(await FindRuleAsync(id, cancellationToken));

    public async Task<RecurrenceRuleDto> CreateAsync(CreateRecurrenceRuleRequest request, CancellationToken cancellationToken = default)
    {
        var rule = new RecurrenceRule
        {
            Name = request.Name,
            AccountId = request.AccountId,
            CategoryId = request.CategoryId,
            Type = request.Type,
            Amount = request.Amount,
            Currency = request.Currency,
            Frequency = request.Frequency,
            DayOfMonth = request.DayOfMonth,
            DayOfWeek = request.DayOfWeek,
            Interval = request.Interval,
            StartDate = request.StartDate,
            EndDate = request.EndDate,
            NextDueDate = request.StartDate,
            AutoPost = request.AutoPost,
        };

        db.RecurrenceRules.Add(rule);
        await db.SaveChangesAsync(cancellationToken);
        return ToDto(rule);
    }

    public async Task<RecurrenceRuleDto> UpdateAsync(Guid id, UpdateRecurrenceRuleRequest request, CancellationToken cancellationToken = default)
    {
        var rule = await FindRuleAsync(id, cancellationToken);

        rule.Name = request.Name;
        rule.AccountId = request.AccountId;
        rule.CategoryId = request.CategoryId;
        rule.Type = request.Type;
        rule.Amount = request.Amount;
        rule.Currency = request.Currency;
        rule.Frequency = request.Frequency;
        rule.DayOfMonth = request.DayOfMonth;
        rule.DayOfWeek = request.DayOfWeek;
        rule.Interval = request.Interval;
        rule.EndDate = request.EndDate;
        rule.AutoPost = request.AutoPost;
        rule.IsActive = request.IsActive;

        await db.SaveChangesAsync(cancellationToken);
        return ToDto(rule);
    }

    public async Task DeleteAsync(Guid id, CancellationToken cancellationToken = default)
    {
        var rule = await FindRuleAsync(id, cancellationToken);
        db.RecurrenceRules.Remove(rule);
        await db.SaveChangesAsync(cancellationToken);
    }

    public async Task<List<RecurrenceOccurrenceDto>> GetPendingOccurrencesAsync(CancellationToken cancellationToken = default)
    {
        var rules = await db.RecurrenceRules.ToDictionaryAsync(r => r.Id, cancellationToken);
        var occurrences = await db.RecurrenceOccurrences
            .Where(o => o.Status == RecurrenceOccurrenceStatus.PendingConfirmation)
            .OrderBy(o => o.DueDate)
            .ToListAsync(cancellationToken);

        return occurrences
            .Select(o => ToOccurrenceDto(o, rules.GetValueOrDefault(o.RecurrenceRuleId)?.Name ?? "(deleted rule)"))
            .ToList();
    }

    public async Task<RecurrenceOccurrenceDto> ConfirmOccurrenceAsync(Guid occurrenceId, CancellationToken cancellationToken = default)
    {
        var occurrence = await FindOccurrenceAsync(occurrenceId, cancellationToken);
        var rule = await FindRuleAsync(occurrence.RecurrenceRuleId, cancellationToken);

        if (occurrence.Status != RecurrenceOccurrenceStatus.PendingConfirmation)
        {
            throw new ValidationException($"Occurrence is already {occurrence.Status}.");
        }

        var transaction = new Transaction
        {
            AccountId = rule.AccountId,
            Type = rule.Type,
            Amount = occurrence.Amount,
            Currency = occurrence.Currency,
            Date = occurrence.DueDate,
            Description = rule.Name,
            CategoryId = rule.CategoryId,
            RecurrenceId = rule.Id,
        };

        db.Transactions.Add(transaction);
        occurrence.Status = RecurrenceOccurrenceStatus.Posted;
        occurrence.PostedTransactionId = transaction.Id;

        await db.SaveChangesAsync(cancellationToken);
        return ToOccurrenceDto(occurrence, rule.Name);
    }

    public async Task<RecurrenceOccurrenceDto> SkipOccurrenceAsync(Guid occurrenceId, CancellationToken cancellationToken = default)
    {
        var occurrence = await FindOccurrenceAsync(occurrenceId, cancellationToken);

        if (occurrence.Status != RecurrenceOccurrenceStatus.PendingConfirmation)
        {
            throw new ValidationException($"Occurrence is already {occurrence.Status}.");
        }

        occurrence.Status = RecurrenceOccurrenceStatus.Skipped;
        await db.SaveChangesAsync(cancellationToken);

        var ruleName = (await db.RecurrenceRules.FirstOrDefaultAsync(r => r.Id == occurrence.RecurrenceRuleId, cancellationToken))?.Name ?? "(deleted rule)";
        return ToOccurrenceDto(occurrence, ruleName);
    }

    private async Task<RecurrenceRule> FindRuleAsync(Guid id, CancellationToken cancellationToken)
        => await db.RecurrenceRules.FirstOrDefaultAsync(r => r.Id == id, cancellationToken)
           ?? throw new NotFoundException(nameof(RecurrenceRule), id);

    private async Task<RecurrenceOccurrence> FindOccurrenceAsync(Guid id, CancellationToken cancellationToken)
        => await db.RecurrenceOccurrences.FirstOrDefaultAsync(o => o.Id == id, cancellationToken)
           ?? throw new NotFoundException(nameof(RecurrenceOccurrence), id);

    private static readonly System.Linq.Expressions.Expression<Func<RecurrenceRule, RecurrenceRuleDto>> ToDtoExpression = r =>
        new RecurrenceRuleDto(r.Id, r.Name, r.AccountId, r.CategoryId, r.Type, r.Amount, r.Currency, r.Frequency,
            r.DayOfMonth, r.DayOfWeek, r.Interval, r.StartDate, r.EndDate, r.NextDueDate, r.AutoPost, r.IsActive);

    private static RecurrenceRuleDto ToDto(RecurrenceRule r) => new(
        r.Id, r.Name, r.AccountId, r.CategoryId, r.Type, r.Amount, r.Currency, r.Frequency,
        r.DayOfMonth, r.DayOfWeek, r.Interval, r.StartDate, r.EndDate, r.NextDueDate, r.AutoPost, r.IsActive);

    private static RecurrenceOccurrenceDto ToOccurrenceDto(RecurrenceOccurrence o, string ruleName) => new(
        o.Id, o.RecurrenceRuleId, ruleName, o.DueDate, o.Amount, o.Currency, o.Status);
}
