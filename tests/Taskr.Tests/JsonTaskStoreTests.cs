using Taskr;
using Taskr.Models;

namespace Taskr.Tests;

public class JsonTaskStoreTests : IDisposable
{
    private readonly string _dir = Path.Combine(Path.GetTempPath(), "taskr-tests-" + Guid.NewGuid());

    public void Dispose()
    {
        if (Directory.Exists(_dir))
        {
            Directory.Delete(_dir, recursive: true);
        }
    }

    [Fact]
    public void Load_MissingFileGivesEmptyList()
    {
        var store = new JsonTaskStore(Path.Combine(_dir, "nope.json"));
        Assert.Empty(store.Load().Items);
    }

    [Fact]
    public void SaveThenLoad_RoundTripsEveryField()
    {
        var store = new JsonTaskStore(Path.Combine(_dir, "nested", "tasks.json"));
        var now = new DateTime(2026, 3, 1, 9, 30, 0);
        var list = new TaskList();
        list.Add("Write report", now, new DateOnly(2026, 3, 10), Priority.High, new[] { "uni", "urgent" });
        list.Complete(1, now.AddDays(1));

        store.Save(list);
        var loaded = store.Load().Items;

        var item = Assert.Single(loaded);
        Assert.Equal("Write report", item.Title);
        Assert.True(item.Done);
        Assert.Equal(new DateOnly(2026, 3, 10), item.Due);
        Assert.Equal(Priority.High, item.Priority);
        Assert.Equal(new[] { "uni", "urgent" }, item.Tags);
        Assert.Equal(now.AddDays(1), item.CompletedAt);
    }

    [Fact]
    public void Save_WritesPriorityAsReadableText()
    {
        var path = Path.Combine(_dir, "tasks.json");
        var list = new TaskList();
        list.Add("a", DateTime.Now, priority: Priority.High);
        new JsonTaskStore(path).Save(list);
        Assert.Contains("\"High\"", File.ReadAllText(path));
    }

    [Fact]
    public void Save_LeavesNoTemporaryFileBehind()
    {
        var path = Path.Combine(_dir, "tasks.json");
        new JsonTaskStore(path).Save(new TaskList());
        Assert.False(File.Exists(path + ".tmp"));
    }

    [Fact]
    public void Load_EmptyFileGivesEmptyList()
    {
        Directory.CreateDirectory(_dir);
        var path = Path.Combine(_dir, "tasks.json");
        File.WriteAllText(path, "");
        Assert.Empty(new JsonTaskStore(path).Load().Items);
    }
}
