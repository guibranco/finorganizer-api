using FinOrganizer.Domain.Enums;

namespace FinOrganizer.Application.Categories;

public sealed record CategoryDto(
    Guid Id,
    string Name,
    CategoryKind Kind,
    Guid? ParentCategoryId,
    string? Color,
    string? Icon,
    bool IsPassive);

public sealed record CreateCategoryRequest(
    string Name, CategoryKind Kind, Guid? ParentCategoryId, string? Color, string? Icon, bool IsPassive);

public sealed record UpdateCategoryRequest(
    string Name, CategoryKind Kind, Guid? ParentCategoryId, string? Color, string? Icon, bool IsPassive);
