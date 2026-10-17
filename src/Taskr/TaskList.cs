using Taskr.Models;

namespace Taskr;

/// <summary>In-memory collection of tasks with all the domain operations.</summary>
public sealed class TaskList
{
    private readonly List<TaskItem> _items;

    public TaskList(IEnumerable<TaskItem>? items = null)
    {
        _items = items?.ToList() ?? new List<TaskItem>();
    }

    public IReadOnlyList<TaskItem> Items => _items;

    public TaskItem Add(string title, DateTime now, DateOnly? due = null, Priority priority = Priority.Medium, IEnumerable<string>? tags = null)
    {
        title = title.Trim();
        if (title.Length == 0)
        {
            throw new ArgumentException("A task needs a title.", nameof(title));
        }

        var normalised = (tags ?? Enumerable.Empty<string>())
            .Select(t => t.Trim().TrimStart('#').ToLowerInvariant())
            .Where(t => t.Length > 0)
            .Distinct()
            .ToList();

        var id = _items.Count == 0 ? 1 : _items.Max(i => i.Id) + 1;
        var item = new TaskItem(id, title, false, due, priority, normalised, now, null);
        _items.Add(item);
        return item;
    }

    public TaskItem Get(int id) =>
        _items.FirstOrDefault(i => i.Id == id) ?? throw new TaskNotFoundException(id);

    public TaskItem Complete(int id, DateTime now) =>
        Replace(id, item => item.Done ? item : item with { Done = true, CompletedAt = now });

    public TaskItem Reopen(int id) =>
        Replace(id, item => item with { Done = false, CompletedAt = null });

    public TaskItem Remove(int id)
    {
        var item = Get(id);
        _items.Remove(item);
        return item;
    }

    /// <summary>Removes every completed task and returns how many were removed.</summary>
    public int ClearDone() => _items.RemoveAll(i => i.Done);

    private TaskItem Replace(int id, Func<TaskItem, TaskItem> change)
    {
        var index = _items.FindIndex(i => i.Id == id);
        if (index < 0)
        {
            throw new TaskNotFoundException(id);
        }

        _items[index] = change(_items[index]);
        return _items[index];
    }
}

public sealed class TaskNotFoundException : Exception
{
    public TaskNotFoundException(int id) : base($"No task with id {id}.")
    {
        Id = id;
    }

    public int Id { get; }
}
