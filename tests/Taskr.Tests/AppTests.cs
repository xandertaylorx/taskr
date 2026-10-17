using Taskr;

namespace Taskr.Tests;

public class AppTests
{
    private sealed class MemoryStore : ITaskStore
    {
        private TaskList _tasks = new();

        public TaskList Load() => new(_tasks.Items);

        public void Save(TaskList tasks) => _tasks = new TaskList(tasks.Items);
    }

    private readonly MemoryStore _store = new();
    private readonly StringWriter _out = new();
    private readonly StringWriter _err = new();

    // Monday 5 January 2026, 10:00.
    private readonly DateTime _now = new(2026, 1, 5, 10, 0, 0);

    private int Run(params string[] args) =>
        new App(_store, _out, _err, () => _now).Run(args);

    [Fact]
    public void Add_ThenList_ShowsTheTask()
    {
        Assert.Equal(0, Run("add", "Write", "essay", "--due", "tomorrow", "-p", "high", "-t", "uni"));
        Assert.Contains("Added #1: Write essay", _out.ToString());

        _out.GetStringBuilder().Clear();
        Assert.Equal(0, Run("list"));
        var output = _out.ToString();
        Assert.Contains("Write essay", output);
        Assert.Contains("2026-01-06", output);
        Assert.Contains("high", output);
        Assert.Contains("#uni", output);
        Assert.Contains("1 open, 0 done", output);
    }

    [Fact]
    public void NoArguments_ListsTasks()
    {
        Run("add", "a");
        _out.GetStringBuilder().Clear();
        Assert.Equal(0, Run());
        Assert.Contains("a", _out.ToString());
    }

    [Fact]
    public void Done_HidesTaskFromDefaultListButShowsInAll()
    {
        Run("add", "Buy milk");
        Run("done", "1");
        _out.GetStringBuilder().Clear();

        Run("list");
        Assert.Contains("No tasks to show.", _out.ToString());

        _out.GetStringBuilder().Clear();
        Run("list", "--all");
        Assert.Contains("[x]", _out.ToString());
    }

    [Fact]
    public void List_FiltersByTag()
    {
        Run("add", "uni thing", "-t", "uni");
        Run("add", "home thing", "-t", "home");
        _out.GetStringBuilder().Clear();
        Run("list", "--tag", "#UNI");
        Assert.Contains("uni thing", _out.ToString());
        Assert.DoesNotContain("home thing", _out.ToString());
    }

    [Fact]
    public void List_OrdersBySoonestDueDateThenPriority()
    {
        Run("add", "later", "--due", "+5d");
        Run("add", "soon", "--due", "+1d");
        Run("add", "no date");
        _out.GetStringBuilder().Clear();
        Run("list");
        var output = _out.ToString();
        Assert.True(output.IndexOf("soon", StringComparison.Ordinal) < output.IndexOf("later", StringComparison.Ordinal));
        Assert.True(output.IndexOf("later", StringComparison.Ordinal) < output.IndexOf("no date", StringComparison.Ordinal));
    }

    [Fact]
    public void List_FlagsOverdueTasks()
    {
        Run("add", "late", "--due", "2026-01-01");
        _out.GetStringBuilder().Clear();
        Run("list");
        Assert.Contains("(overdue)", _out.ToString());
        Assert.Contains("1 overdue", _out.ToString());
    }

    [Fact]
    public void Undo_ReopensATask()
    {
        Run("add", "a");
        Run("done", "1");
        Assert.Equal(0, Run("undo", "1"));
        Assert.Contains("Reopened #1", _out.ToString());
    }

    [Fact]
    public void Rm_RemovesATask()
    {
        Run("add", "a");
        Assert.Equal(0, Run("rm", "1"));
        Assert.Empty(_store.Load().Items);
    }

    [Fact]
    public void Clear_RemovesCompletedTasks()
    {
        Run("add", "a");
        Run("add", "b");
        Run("done", "1");
        _out.GetStringBuilder().Clear();
        Assert.Equal(0, Run("clear"));
        Assert.Contains("Removed 1 completed task.", _out.ToString());
    }

    [Theory]
    [InlineData("done", "99")]
    [InlineData("done", "abc")]
    [InlineData("add")]
    [InlineData("add", "x", "--due", "someday")]
    [InlineData("add", "x", "-p", "urgent")]
    [InlineData("add", "x", "--due")]
    [InlineData("list", "--bogus")]
    public void BadInput_ReturnsOneAndExplainsOnStderr(params string[] args)
    {
        Assert.Equal(1, Run(args));
        Assert.StartsWith("taskr:", _err.ToString());
    }

    [Fact]
    public void UnknownCommand_ReturnsTwo()
    {
        Assert.Equal(2, Run("dance"));
        Assert.Contains("unknown command", _err.ToString());
    }

    [Fact]
    public void HelpAndVersion()
    {
        Assert.Equal(0, Run("--help"));
        Assert.Contains("Usage:", _out.ToString());
        Assert.Equal(0, Run("--version"));
        Assert.Contains("taskr " + App.Version, _out.ToString());
    }
}
