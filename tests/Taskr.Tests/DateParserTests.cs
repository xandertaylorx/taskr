using Taskr;

namespace Taskr.Tests;

public class DateParserTests
{
    // Monday 5 January 2026.
    private static readonly DateOnly Today = new(2026, 1, 5);

    [Theory]
    [InlineData("today", "2026-01-05")]
    [InlineData("TOMORROW", "2026-01-06")]
    [InlineData("2026-12-25", "2026-12-25")]
    [InlineData("+3d", "2026-01-08")]
    [InlineData("+2w", "2026-01-19")]
    [InlineData("+0d", "2026-01-05")]
    [InlineData("fri", "2026-01-09")]
    [InlineData("Friday", "2026-01-09")]
    [InlineData("sun", "2026-01-11")]
    public void Parse_UnderstandsFriendlyInput(string input, string expected)
    {
        Assert.Equal(DateOnly.Parse(expected), DateParser.Parse(input, Today));
    }

    [Fact]
    public void Parse_SameWeekdayMeansNextWeek()
    {
        Assert.Equal(new DateOnly(2026, 1, 12), DateParser.Parse("mon", Today));
    }

    [Theory]
    [InlineData("")]
    [InlineData("   ")]
    [InlineData("soon")]
    [InlineData("2026-13-45")]
    [InlineData("+d")]
    [InlineData("+3x")]
    [InlineData("-3d")]
    public void Parse_RejectsGarbage(string input)
    {
        Assert.False(DateParser.TryParse(input, Today, out _));
        Assert.Throws<FormatException>(() => DateParser.Parse(input, Today));
    }
}
