using FinOrganizer.Application.Common.Exceptions;
using FinOrganizer.Application.Common.Interfaces;
using FinOrganizer.Domain.Entities;
using FluentValidation;
using Microsoft.EntityFrameworkCore;

namespace FinOrganizer.Application.Categories;

public sealed class CategoriesService(IApplicationDbContext db) : ICategoriesService
{
    public async Task<List<CategoryDto>> GetAllAsync(CancellationToken cancellationToken = default)
        => await db.Categories.OrderBy(c => c.Name).Select(ToDtoExpression).ToListAsync(cancellationToken);

    public async Task<CategoryDto> GetByIdAsync(Guid id, CancellationToken cancellationToken = default)
        => ToDto(await FindAsync(id, cancellationToken));

    public async Task<CategoryDto> CreateAsync(CreateCategoryRequest request, CancellationToken cancellationToken = default)
    {
        await EnsureSingleLevelNestingAsync(request.ParentCategoryId, cancellationToken);

        var category = new Category
        {
            Name = request.Name,
            Kind = request.Kind,
            ParentCategoryId = request.ParentCategoryId,
            Color = request.Color,
            Icon = request.Icon,
            IsPassive = request.IsPassive,
        };

        db.Categories.Add(category);
        await db.SaveChangesAsync(cancellationToken);
        return ToDto(category);
    }

    public async Task<CategoryDto> UpdateAsync(Guid id, UpdateCategoryRequest request, CancellationToken cancellationToken = default)
    {
        var category = await FindAsync(id, cancellationToken);

        if (request.ParentCategoryId == id)
        {
            throw new ValidationException("A category cannot be its own parent.");
        }

        await EnsureSingleLevelNestingAsync(request.ParentCategoryId, cancellationToken);

        category.Name = request.Name;
        category.Kind = request.Kind;
        category.ParentCategoryId = request.ParentCategoryId;
        category.Color = request.Color;
        category.Icon = request.Icon;
        category.IsPassive = request.IsPassive;

        await db.SaveChangesAsync(cancellationToken);
        return ToDto(category);
    }

    public async Task DeleteAsync(Guid id, CancellationToken cancellationToken = default)
    {
        var category = await FindAsync(id, cancellationToken);

        var inUse = await db.Transactions.AnyAsync(t => t.CategoryId == id, cancellationToken)
                    || await db.Categories.AnyAsync(c => c.ParentCategoryId == id, cancellationToken)
                    || await db.RecurrenceRules.AnyAsync(r => r.CategoryId == id, cancellationToken)
                    || await db.Budgets.AnyAsync(b => b.CategoryId == id, cancellationToken);

        if (inUse)
        {
            throw new ValidationException($"Category '{category.Name}' is in use and cannot be deleted.");
        }

        db.Categories.Remove(category);
        await db.SaveChangesAsync(cancellationToken);
    }

    private async Task EnsureSingleLevelNestingAsync(Guid? parentCategoryId, CancellationToken cancellationToken)
    {
        if (parentCategoryId is not { } parentId)
        {
            return;
        }

        var parent = await db.Categories.FirstOrDefaultAsync(c => c.Id == parentId, cancellationToken)
                     ?? throw new NotFoundException(nameof(Category), parentId);

        if (parent.ParentCategoryId is not null)
        {
            throw new ValidationException("Categories only support one level of nesting.");
        }
    }

    private async Task<Category> FindAsync(Guid id, CancellationToken cancellationToken)
        => await db.Categories.FirstOrDefaultAsync(c => c.Id == id, cancellationToken)
           ?? throw new NotFoundException(nameof(Category), id);

    private static readonly System.Linq.Expressions.Expression<Func<Category, CategoryDto>> ToDtoExpression =
        c => new CategoryDto(c.Id, c.Name, c.Kind, c.ParentCategoryId, c.Color, c.Icon, c.IsPassive);

    private static CategoryDto ToDto(Category c)
        => new(c.Id, c.Name, c.Kind, c.ParentCategoryId, c.Color, c.Icon, c.IsPassive);
}
