using FinOrganizer.Domain.Entities;
using FinOrganizer.Domain.Enums;

namespace FinOrganizer.Infrastructure.Persistence.SeedData;

/// <summary>
/// Fixed-Id default categories applied via EF Core migration seeding (<c>HasData</c>), so the Ids
/// stay stable across environments instead of being randomly generated per database.
/// </summary>
public static class CategorySeedData
{
    public static readonly Category Salary = new()
    {
        Id = Guid.Parse("00000000-0000-0000-0001-000000000001"),
        Name = "Salary",
        Kind = CategoryKind.Income,
    };

    public static readonly Category Freelance = new()
    {
        Id = Guid.Parse("00000000-0000-0000-0001-000000000002"),
        Name = "Freelance",
        Kind = CategoryKind.Income,
    };

    public static readonly Category Dividends = new()
    {
        Id = Guid.Parse("00000000-0000-0000-0001-000000000003"),
        Name = "Dividends",
        Kind = CategoryKind.Income,
        IsPassive = true,
    };

    public static readonly Category RentMortgage = new()
    {
        Id = Guid.Parse("00000000-0000-0000-0001-000000000004"),
        Name = "Rent/Mortgage",
        Kind = CategoryKind.Expense,
    };

    public static readonly Category Groceries = new()
    {
        Id = Guid.Parse("00000000-0000-0000-0001-000000000005"),
        Name = "Groceries",
        Kind = CategoryKind.Expense,
    };

    public static readonly Category Utilities = new()
    {
        Id = Guid.Parse("00000000-0000-0000-0001-000000000006"),
        Name = "Utilities",
        Kind = CategoryKind.Expense,
    };

    public static readonly Category Subscriptions = new()
    {
        Id = Guid.Parse("00000000-0000-0000-0001-000000000007"),
        Name = "Subscriptions",
        Kind = CategoryKind.Expense,
    };

    public static readonly Category Transport = new()
    {
        Id = Guid.Parse("00000000-0000-0000-0001-000000000008"),
        Name = "Transport",
        Kind = CategoryKind.Expense,
    };

    public static readonly Category Health = new()
    {
        Id = Guid.Parse("00000000-0000-0000-0001-000000000009"),
        Name = "Health",
        Kind = CategoryKind.Expense,
    };

    public static readonly Category Leisure = new()
    {
        Id = Guid.Parse("00000000-0000-0000-0001-00000000000a"),
        Name = "Leisure",
        Kind = CategoryKind.Expense,
    };

    public static IReadOnlyList<Category> All { get; } =
    [
        Salary, Freelance, Dividends, RentMortgage, Groceries, Utilities, Subscriptions, Transport, Health, Leisure,
    ];
}
