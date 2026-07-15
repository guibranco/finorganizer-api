namespace FinOrganizer.Domain.Common;

public abstract class Entity
{
    public Guid Id { get; init; } = Guid.NewGuid();
}
