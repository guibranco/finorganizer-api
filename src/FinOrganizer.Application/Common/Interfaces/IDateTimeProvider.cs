namespace FinOrganizer.Application.Common.Interfaces;

public interface IDateTimeProvider
{
    DateTime UtcNow { get; }

    DateOnly Today { get; }
}
