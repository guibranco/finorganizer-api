using FinOrganizer.Domain.Common;
using FinOrganizer.Domain.Enums;

namespace FinOrganizer.Domain.Entities;

public class Category : Entity
{
    public required string Name { get; set; }

    public CategoryKind Kind { get; set; }

    public Guid? ParentCategoryId { get; set; }

    public string? Color { get; set; }

    public string? Icon { get; set; }

    public bool IsPassive { get; set; }
}
