using Taskr.Models;

namespace Taskr;

/// <summary>The command line application. Kept free of globals so it is easy to test.</summary>
public sealed class App
{
    public const string Version = "0.1.0";

    private const string Usage = """
        taskr - a fast, friendly to-do list for your terminal

        Usage:
          taskr add <title> [--due <date>] [--priority low|medium|high] [--tag <name>]...
          taskr list [--all] [--done] [--tag <name>]
          taskr done <id>
          taskr undo <id>
          taskr rm <id>
          taskr clear               remove all completed tasks
          taskr --help | --version

        Dates: YYYY-MM-DD, today, tomorrow, mon..sun, +3d, +2w
        Tasks are stored in $TASKR_FILE (default: ~/.taskr/tasks.json).
        """;

    private readonly ITaskStore _store;
    private readonly TextWriter _out;
    private readonly TextWriter _err;
    private readonly Func<DateTime> _clock;

    public App(ITaskStore store, TextWriter @out, TextWriter err, Func<DateTime>? clock = null)
    {
        _store = store;
        _out = @out;
        _err = err;
        _clock = clock ?? (() => DateTime.Now);
    }

    public int Run(string[] args)
    {
        if (args.Length == 0)
        {
            args = ["list"];
        }

        try
        {
            return Dispatch(args[0].ToLowerInvariant(), args[1..]);
        }
        catch (Exception ex) when (ex is ArgumentException or FormatException or TaskNotFoundException or InvalidDataException)
        {
            _err.WriteLine($"taskr: {ex.Message}");
            return 1;
        }
    }

    private int Dispatch(string command, string[] args)
    {
        switch (command)
        {
            case "add": return Add(args);
            case "list" or "ls": return List(args);
            case "done": return Mutate(args, (tasks, id, now) => tasks.Complete(id, now), "Completed");
            case "undo": return Mutate(args, (tasks, id, _) => tasks.Reopen(id), "Reopened");
            case "rm" or "remove": return Mutate(args, (tasks, id, _) => tasks.Remove(id), "Removed");
            case "clear": return Clear();
            case "-h" or "--help" or "help":
                _out.WriteLine(Usage);
                return 0;
            case "--version" or "version":
                _out.WriteLine($"taskr {Version}");
                return 0;
            default:
                _err.WriteLine($"taskr: unknown command '{command}'. Try 'taskr --help'.");
                return 2;
        }
    }

    private int Add(string[] args)
    {
        var now = _clock();
        var today = DateOnly.FromDateTime(now);
        var titleParts = new List<string>();
        var tags = new List<string>();
        DateOnly? due = null;
        var priority = Priority.Medium;

        for (var i = 0; i < args.Length; i++)
        {
            switch (args[i])
            {
                case "--due" or "-d":
                    due = DateParser.Parse(Next(args, ref i), today);
                    break;
                case "--priority" or "-p":
                    priority = ParsePriority(Next(args, ref i));
                    break;
                case "--tag" or "-t":
                    tags.Add(Next(args, ref i));
                    break;
                default:
                    titleParts.Add(args[i]);
                    break;
            }
        }

        var tasks = _store.Load();
        var item = tasks.Add(string.Join(' ', titleParts), now, due, priority, tags);
        _store.Save(tasks);
        _out.WriteLine($"Added #{item.Id}: {item.Title}");
        return 0;
    }

    private int List(string[] args)
    {
        var all = false;
        var onlyDone = false;
        string? tag = null;
        for (var i = 0; i < args.Length; i++)
        {
            switch (args[i])
            {
                case "--all" or "-a": all = true; break;
                case "--done": onlyDone = true; break;
                case "--tag" or "-t": tag = Next(args, ref i).TrimStart('#').ToLowerInvariant(); break;
                default: throw new ArgumentException($"Unknown option '{args[i]}'.");
            }
        }

        var today = DateOnly.FromDateTime(_clock());
        var items = _store.Load().Items.AsEnumerable();
        if (onlyDone)
        {
            items = items.Where(t => t.Done);
        }
        else if (!all)
        {
            items = items.Where(t => !t.Done);
        }

        if (tag is not null)
        {
            items = items.Where(t => t.Tags.Contains(tag));
        }

        // Open tasks first, then soonest due date, then highest priority.
        var ordered = items
            .OrderBy(t => t.Done)
            .ThenBy(t => t.Due ?? DateOnly.MaxValue)
            .ThenByDescending(t => t.Priority)
            .ThenBy(t => t.Id)
            .ToList();

        _out.Write(Formatter.Table(ordered, today));
        _out.WriteLine();
        _out.Write(Formatter.Summary(_store.Load().Items, today));
        return 0;
    }

    private int Mutate(string[] args, Func<TaskList, int, DateTime, TaskItem> action, string verb)
    {
        if (args.Length != 1 || !int.TryParse(args[0].TrimStart('#'), out var id))
        {
            throw new ArgumentException("Expected exactly one numeric task id.");
        }

        var tasks = _store.Load();
        var item = action(tasks, id, _clock());
        _store.Save(tasks);
        _out.WriteLine($"{verb} #{item.Id}: {item.Title}");
        return 0;
    }

    private int Clear()
    {
        var tasks = _store.Load();
        var removed = tasks.ClearDone();
        _store.Save(tasks);
        _out.WriteLine($"Removed {removed} completed task{(removed == 1 ? "" : "s")}.");
        return 0;
    }

    private static string Next(string[] args, ref int index)
    {
        if (index + 1 >= args.Length)
        {
            throw new ArgumentException($"Missing value for {args[index]}.");
        }

        return args[++index];
    }

    private static Priority ParsePriority(string text) => text.ToLowerInvariant() switch
    {
        "low" or "l" => Priority.Low,
        "medium" or "med" or "m" => Priority.Medium,
        "high" or "h" => Priority.High,
        _ => throw new ArgumentException($"Unknown priority '{text}'. Use low, medium or high."),
    };
}
