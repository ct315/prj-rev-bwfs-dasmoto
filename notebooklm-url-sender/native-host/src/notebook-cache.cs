using System.Text.Json;

namespace NotebookLmSender.Host;

internal sealed record NotebookEntry(string Name, string? Id);

internal sealed class NotebookCache
{
    private readonly string _cachePath;

    public NotebookCache()
    {
        var dir = Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.LocalApplicationData), "NotebookLmUrlSender");
        Directory.CreateDirectory(dir);
        _cachePath = Path.Combine(dir, "notebooks-cache.json");
    }

    public List<NotebookEntry> Read()
    {
        if (!File.Exists(_cachePath)) return [];
        var json = File.ReadAllText(_cachePath);
        return JsonSerializer.Deserialize<List<NotebookEntry>>(json) ?? [];
    }

    public void Write(List<NotebookEntry> items)
    {
        var json = JsonSerializer.Serialize(items);
        File.WriteAllText(_cachePath, json);
    }
}
