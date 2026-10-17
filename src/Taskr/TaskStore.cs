using System.Text.Json;
using System.Text.Json.Serialization;
using Taskr.Models;

namespace Taskr;

/// <summary>Where tasks are loaded from and saved to.</summary>
public interface ITaskStore
{
    TaskList Load();

    void Save(TaskList tasks);
}

/// <summary>Stores tasks as indented JSON, written atomically.</summary>
public sealed class JsonTaskStore : ITaskStore
{
    private static readonly JsonSerializerOptions Options = new()
    {
        WriteIndented = true,
        Converters = { new JsonStringEnumConverter() },
    };

    private readonly string _path;

    public JsonTaskStore(string path)
    {
        _path = path;
    }

    /// <summary>Default location: $TASKR_FILE, or ~/.taskr/tasks.json.</summary>
    public static string DefaultPath()
    {
        var fromEnv = Environment.GetEnvironmentVariable("TASKR_FILE");
        if (!string.IsNullOrWhiteSpace(fromEnv))
        {
            return fromEnv;
        }

        var home = Environment.GetFolderPath(Environment.SpecialFolder.UserProfile);
        return Path.Combine(home, ".taskr", "tasks.json");
    }

    public TaskList Load()
    {
        if (!File.Exists(_path))
        {
            return new TaskList();
        }

        var json = File.ReadAllText(_path);
        if (string.IsNullOrWhiteSpace(json))
        {
            return new TaskList();
        }

        var items = JsonSerializer.Deserialize<List<TaskItem>>(json, Options)
            ?? throw new InvalidDataException($"{_path} does not contain a task list.");
        return new TaskList(items);
    }

    public void Save(TaskList tasks)
    {
        var directory = Path.GetDirectoryName(Path.GetFullPath(_path));
        if (!string.IsNullOrEmpty(directory))
        {
            Directory.CreateDirectory(directory);
        }

        // Write to a temp file first so a crash never leaves a half-written list.
        var temp = _path + ".tmp";
        File.WriteAllText(temp, JsonSerializer.Serialize(tasks.Items, Options));
        File.Move(temp, _path, overwrite: true);
    }
}
