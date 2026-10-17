using Taskr;
using Taskr.Models;

namespace Taskr.Tests;

public class TaskListTests
{
    private static readonly DateTime Now = new(2026, 1, 5, 10, 0, 0);

    [Fact]
    public void Add_AssignsIncreasingIds()
    {
        var list = new TaskList();
        Assert.Equal(1, list.Add("a", Now).Id);
        Assert.Equal(2, list.Add("b", Now).Id);
    }

    [Fact]
    public void Add_DoesNotReuseIdsAfterRemoval()
    {
        var list = new TaskList();
        list.Add("a", Now);
        list.Add("b", Now);
        list.Remove(2);
        Assert.Equal(2, list.Add("c", Now).Id);
    }

    [Fact]
    public void Add_TrimsTitleAndNormalisesTags()
    {
        var item = new TaskList().Add("  Write essay ", Now, tags: new[] { "#Uni", "uni", " Work ", "" });
        Assert.Equal("Write essay", item.Title);
        Assert.Equal(new[] { "uni", "work" }, item.Tags);
    }

    [Fact]
    public void Add_RejectsBlankTitle()
    {
        Assert.Throws<ArgumentException>(() => new TaskList().Add("   ", Now));
    }

    [Fact]
    public void Complete_SetsDoneAndTimestamp()
    {
        var list = new TaskList();
        list.Add("a", Now);
        var done = list.Complete(1, Now.AddHours(1));
        Assert.True(done.Done);
        Assert.Equal(Now.AddHours(1), done.CompletedAt);
    }

    [Fact]
    public void Complete_IsIdempotent()
    {
        var list = new TaskList();
        list.Add("a", Now);
        list.Complete(1, Now.AddHours(1));
        var again = list.Complete(1, Now.AddHours(5));
        Assert.Equal(Now.AddHours(1), again.CompletedAt);
    }

    [Fact]
    public void Reopen_ClearsCompletion()
    {
        var list = new TaskList();
        list.Add("a", Now);
        list.Complete(1, Now);
        var reopened = list.Reopen(1);
        Assert.False(reopened.Done);
        Assert.Null(reopened.CompletedAt);
    }

    [Fact]
    public void UnknownId_Throws()
    {
        var list = new TaskList();
        Assert.Throws<TaskNotFoundException>(() => list.Complete(99, Now));
        Assert.Throws<TaskNotFoundException>(() => list.Remove(99));
    }

    [Fact]
    public void ClearDone_RemovesOnlyCompletedTasks()
    {
        var list = new TaskList();
        list.Add("a", Now);
        list.Add("b", Now);
        list.Complete(1, Now);
        Assert.Equal(1, list.ClearDone());
        Assert.Equal("b", Assert.Single(list.Items).Title);
    }

    [Fact]
    public void IsOverdue_OnlyForOpenTasksPastDue()
    {
        var today = new DateOnly(2026, 1, 5);
        var list = new TaskList();
        list.Add("late", Now, due: today.AddDays(-1));
        list.Add("today", Now, due: today);
        list.Add("none", Now);
        list.Add("late but done", Now, due: today.AddDays(-3));
        list.Complete(4, Now);

        Assert.Equal(new[] { true, false, false, false }, list.Items.Select(i => i.IsOverdue(today)));
    }
}
