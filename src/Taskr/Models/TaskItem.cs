namespace Taskr.Models;

/// <summary>A single to-do item. Immutable; use <c>with</c> to change it.</summary>
public sealed record TaskItem(
    int Id,
    string Title,
    bool Done,
    DateOnly? Due,
    Priority Priority,
    IReadOnlyList<string> Tags,
    DateTime CreatedAt,
    DateTime? CompletedAt)
{
    public bool IsOverdue(DateOnly today) => !Done && Due is { } due && due < today;
}
