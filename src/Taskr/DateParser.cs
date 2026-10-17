using System.Globalization;

namespace Taskr;

/// <summary>Parses friendly due-date input such as "tomorrow", "+3d" or "2026-12-25".</summary>
public static class DateParser
{
    public static DateOnly Parse(string input, DateOnly today)
    {
        if (TryParse(input, today, out var date))
        {
            return date;
        }

        throw new FormatException(
            $"Could not understand date '{input}'. Try YYYY-MM-DD, today, tomorrow, a weekday (mon..sun), +3d or +2w.");
    }

    public static bool TryParse(string? input, DateOnly today, out DateOnly date)
    {
        date = default;
        if (string.IsNullOrWhiteSpace(input))
        {
            return false;
        }

        var text = input.Trim().ToLowerInvariant();

        switch (text)
        {
            case "today":
                date = today;
                return true;
            case "tomorrow":
                date = today.AddDays(1);
                return true;
        }

        if (DateOnly.TryParseExact(text, "yyyy-MM-dd", CultureInfo.InvariantCulture, DateTimeStyles.None, out date))
        {
            return true;
        }

        if (TryParseOffset(text, today, out date))
        {
            return true;
        }

        return TryParseWeekday(text, today, out date);
    }

    // "+3d" is three days from today, "+2w" is two weeks.
    private static bool TryParseOffset(string text, DateOnly today, out DateOnly date)
    {
        date = default;
        if (text.Length < 3 || text[0] != '+' || !int.TryParse(text[1..^1], out var amount) || amount < 0)
        {
            return false;
        }

        switch (text[^1])
        {
            case 'd':
                date = today.AddDays(amount);
                return true;
            case 'w':
                date = today.AddDays(amount * 7);
                return true;
            default:
                return false;
        }
    }

    // A weekday name means the next time that day occurs (never today).
    private static bool TryParseWeekday(string text, DateOnly today, out DateOnly date)
    {
        date = default;
        var names = new[] { "sun", "mon", "tue", "wed", "thu", "fri", "sat" };
        var index = Array.FindIndex(names, n => text.StartsWith(n, StringComparison.Ordinal));
        if (index < 0)
        {
            return false;
        }

        var ahead = ((index - (int)today.DayOfWeek) + 7) % 7;
        date = today.AddDays(ahead == 0 ? 7 : ahead);
        return true;
    }
}
