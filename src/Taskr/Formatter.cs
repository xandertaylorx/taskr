using System.Text;
using Taskr.Models;

namespace Taskr;

/// <summary>Renders tasks as an aligned plain-text table.</summary>
public static class Formatter
{
    public static string Table(IReadOnlyList<TaskItem> tasks, DateOnly today)
    {
        if (tasks.Count == 0)
        {
            return "No tasks to show.\n";
        }

        var rows = tasks.Select(t => new[]
        {
            t.Id.ToString(),
            t.Done ? "[x]" : "[ ]",
            PriorityLabel(t.Priority),
            DueLabel(t, today),
            t.Title,
            string.Join(" ", t.Tags.Select(tag => "#" + tag)),
        }).ToList();

        var headers = new[] { "ID", "", "PRI", "DUE", "TASK", "TAGS" };
        var widths = headers.Select((h, i) => Math.Max(h.Length, rows.Max(r => r[i].Length))).ToArray();

        var sb = new StringBuilder();
        AppendRow(sb, headers, widths);
        foreach (var row in rows)
        {
            AppendRow(sb, row, widths);
        }

        return sb.ToString();
    }

    public static string Summary(IReadOnlyList<TaskItem> tasks, DateOnly today)
    {
        var done = tasks.Count(t => t.Done);
        var overdue = tasks.Count(t => t.IsOverdue(today));
        var text = $"{tasks.Count - done} open, {done} done";
        return overdue > 0 ? $"{text}, {overdue} overdue\n" : text + "\n";
    }

    private static string PriorityLabel(Priority priority) => priority switch
    {
        Priority.High => "high",
        Priority.Low => "low",
        _ => "med",
    };

    private static string DueLabel(TaskItem task, DateOnly today)
    {
        if (task.Due is not { } due)
        {
            return "-";
        }

        var label = due.ToString("yyyy-MM-dd");
        if (task.IsOverdue(today))
        {
            return label + " (overdue)";
        }

        return due == today ? label + " (today)" : label;
    }

    private static void AppendRow(StringBuilder sb, string[] cells, int[] widths)
    {
        var line = new StringBuilder();
        for (var i = 0; i < cells.Length; i++)
        {
            line.Append(i == cells.Length - 1 ? cells[i] : cells[i].PadRight(widths[i] + 2));
        }

        // Empty trailing cells would otherwise leave trailing spaces.
        sb.Append(line.ToString().TrimEnd()).Append('\n');
    }
}
