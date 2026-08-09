using System.Text.Json;

namespace TranslatorTrayApp.History;

internal sealed record TranslationHistoryItem(string OriginalText, string TranslatedText, DateTimeOffset Timestamp);

internal sealed class TranslationHistoryStore
{
    private readonly object _gate = new();
    private readonly string _historyFilePath;
    private readonly List<TranslationHistoryItem> _items;
    private const int MaxItems = 20;

    public TranslationHistoryStore()
    {
        var appData = Path.Combine(
            Environment.GetFolderPath(Environment.SpecialFolder.LocalApplicationData),
            "TranslatorTrayApp");

        Directory.CreateDirectory(appData);
        _historyFilePath = Path.Combine(appData, "history.json");
        _items = LoadItems(_historyFilePath);
    }

    public void Add(string originalText, string translatedText)
    {
        if (string.IsNullOrWhiteSpace(originalText) || string.IsNullOrWhiteSpace(translatedText))
            return;

        List<TranslationHistoryItem> snapshot;

        lock (_gate)
        {
            _items.Insert(0, new TranslationHistoryItem(originalText, translatedText, DateTimeOffset.UtcNow));

            if (_items.Count > MaxItems)
                _items.RemoveRange(MaxItems, _items.Count - MaxItems);

            snapshot = new List<TranslationHistoryItem>(_items);
        }

        _ = PersistSnapshotAsync(snapshot);
    }

    private async Task PersistSnapshotAsync(List<TranslationHistoryItem> snapshot)
    {
        try
        {
            var json = JsonSerializer.Serialize(snapshot);
            await File.WriteAllTextAsync(_historyFilePath, json).ConfigureAwait(false);
        }
        catch
        {
            // Histórico é opcional; ignorar falhas de IO para manter UX rápida.
        }
    }

    private static List<TranslationHistoryItem> LoadItems(string path)
    {
        try
        {
            if (!File.Exists(path))
                return new List<TranslationHistoryItem>(MaxItems);

            var json = File.ReadAllText(path);
            var items = JsonSerializer.Deserialize<List<TranslationHistoryItem>>(json);
            return items ?? new List<TranslationHistoryItem>(MaxItems);
        }
        catch
        {
            return new List<TranslationHistoryItem>(MaxItems);
        }
    }
}
