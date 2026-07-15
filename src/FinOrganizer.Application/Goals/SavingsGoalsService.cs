using FinOrganizer.Application.Common.Exceptions;
using FinOrganizer.Application.Common.Interfaces;
using FinOrganizer.Domain.Entities;
using FinOrganizer.Domain.Services;
using Microsoft.EntityFrameworkCore;

namespace FinOrganizer.Application.Goals;

public sealed class SavingsGoalsService(IApplicationDbContext db) : ISavingsGoalsService
{
    public async Task<List<SavingsGoalDto>> GetAllAsync(CancellationToken cancellationToken = default)
    {
        var goals = await db.SavingsGoals.OrderBy(g => g.TargetDate).ToListAsync(cancellationToken);
        var result = new List<SavingsGoalDto>(goals.Count);
        foreach (var goal in goals)
        {
            result.Add(await ToDtoAsync(goal, cancellationToken));
        }

        return result;
    }

    public async Task<SavingsGoalDto> GetByIdAsync(Guid id, CancellationToken cancellationToken = default)
        => await ToDtoAsync(await FindAsync(id, cancellationToken), cancellationToken);

    public async Task<SavingsGoalDto> CreateAsync(CreateSavingsGoalRequest request, CancellationToken cancellationToken = default)
    {
        if (request.LinkedAccountId is { } accountId && !await db.Accounts.AnyAsync(a => a.Id == accountId, cancellationToken))
        {
            throw new NotFoundException(nameof(Account), accountId);
        }

        var goal = new SavingsGoal
        {
            Name = request.Name,
            TargetAmount = request.TargetAmount,
            TargetDate = request.TargetDate,
            LinkedAccountId = request.LinkedAccountId,
        };

        db.SavingsGoals.Add(goal);
        await db.SaveChangesAsync(cancellationToken);
        return await ToDtoAsync(goal, cancellationToken);
    }

    public async Task<SavingsGoalDto> UpdateAsync(Guid id, UpdateSavingsGoalRequest request, CancellationToken cancellationToken = default)
    {
        var goal = await FindAsync(id, cancellationToken);

        if (request.LinkedAccountId is { } accountId && !await db.Accounts.AnyAsync(a => a.Id == accountId, cancellationToken))
        {
            throw new NotFoundException(nameof(Account), accountId);
        }

        goal.Name = request.Name;
        goal.TargetAmount = request.TargetAmount;
        goal.TargetDate = request.TargetDate;
        goal.LinkedAccountId = request.LinkedAccountId;

        await db.SaveChangesAsync(cancellationToken);
        return await ToDtoAsync(goal, cancellationToken);
    }

    public async Task DeleteAsync(Guid id, CancellationToken cancellationToken = default)
    {
        var goal = await FindAsync(id, cancellationToken);
        var contributions = await db.SavingsGoalContributions.Where(c => c.SavingsGoalId == id).ToListAsync(cancellationToken);
        db.SavingsGoalContributions.RemoveRange(contributions);
        db.SavingsGoals.Remove(goal);
        await db.SaveChangesAsync(cancellationToken);
    }

    public async Task<List<SavingsGoalContributionDto>> GetContributionsAsync(Guid goalId, CancellationToken cancellationToken = default)
        => await db.SavingsGoalContributions
            .Where(c => c.SavingsGoalId == goalId)
            .OrderByDescending(c => c.Date)
            .Select(c => new SavingsGoalContributionDto(c.Id, c.SavingsGoalId, c.Amount, c.Date, c.Note))
            .ToListAsync(cancellationToken);

    public async Task<SavingsGoalContributionDto> AddContributionAsync(Guid goalId, AddSavingsGoalContributionRequest request, CancellationToken cancellationToken = default)
    {
        var goal = await FindAsync(goalId, cancellationToken);

        if (goal.LinkedAccountId is not null)
        {
            throw new FluentValidation.ValidationException(
                "Cannot add manual contributions to a goal linked to an account; its progress tracks the account balance.");
        }

        var contribution = new SavingsGoalContribution { SavingsGoalId = goalId, Amount = request.Amount, Date = request.Date, Note = request.Note };
        db.SavingsGoalContributions.Add(contribution);
        await db.SaveChangesAsync(cancellationToken);
        return new SavingsGoalContributionDto(contribution.Id, contribution.SavingsGoalId, contribution.Amount, contribution.Date, contribution.Note);
    }

    private async Task<SavingsGoal> FindAsync(Guid id, CancellationToken cancellationToken)
        => await db.SavingsGoals.FirstOrDefaultAsync(g => g.Id == id, cancellationToken)
           ?? throw new NotFoundException(nameof(SavingsGoal), id);

    private async Task<SavingsGoalDto> ToDtoAsync(SavingsGoal goal, CancellationToken cancellationToken)
    {
        decimal currentAmount;

        if (goal.LinkedAccountId is { } accountId)
        {
            var account = await db.Accounts.FirstOrDefaultAsync(a => a.Id == accountId, cancellationToken);
            if (account is null)
            {
                currentAmount = 0;
            }
            else
            {
                var transactions = await db.Transactions
                    .Where(t => t.AccountId == accountId || t.CounterpartyAccountId == accountId)
                    .ToListAsync(cancellationToken);
                currentAmount = AccountBalanceCalculator.ComputeBalance(account, transactions);
            }
        }
        else
        {
            currentAmount = await db.SavingsGoalContributions
                .Where(c => c.SavingsGoalId == goal.Id)
                .SumAsync(c => c.Amount, cancellationToken);
        }

        var progress = goal.TargetAmount == 0 ? 0 : Math.Round(Math.Clamp(currentAmount / goal.TargetAmount * 100, 0, 100), 2);
        return new SavingsGoalDto(goal.Id, goal.Name, goal.TargetAmount, goal.TargetDate, goal.LinkedAccountId, currentAmount, progress);
    }
}
