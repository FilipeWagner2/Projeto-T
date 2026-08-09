using System.Net;
using System.Text.Json;

namespace TranslatorTrayApp.Translation;

internal sealed record TranslationResult(string TranslatedText, string? Context, string? Example);

internal sealed class TranslationService
{
    private static readonly HttpClient Http = new(new SocketsHttpHandler
    {
        AutomaticDecompression = DecompressionMethods.GZip | DecompressionMethods.Deflate,
        PooledConnectionLifetime = TimeSpan.FromMinutes(10),
        MaxConnectionsPerServer = 4
    })
    {
        Timeout = TimeSpan.FromSeconds(4)
    };

    private readonly object _cacheLock = new();
    private readonly Dictionary<string, TranslationResult> _cache = new(StringComparer.Ordinal);
    private readonly Queue<string> _cacheOrder = new();
    private const int MaxCacheItems = 100;

    public async Task<TranslationResult> TranslateAsync(string text, string targetLanguage, CancellationToken cancellationToken)
    {
        var cleanText = text.Trim();
        if (string.IsNullOrWhiteSpace(cleanText))
            return new TranslationResult(string.Empty, null, null);

        var key = $"{targetLanguage}|{cleanText}";
        if (TryGetCached(key, out var cached))
            return cached;

        var encoded = Uri.EscapeDataString(cleanText);
        var url = $"https://translate.googleapis.com/translate_a/single?client=gtx&sl=auto&tl={targetLanguage}&dt=t&q={encoded}";

        using var response = await Http.GetAsync(url, HttpCompletionOption.ResponseHeadersRead, cancellationToken);
        response.EnsureSuccessStatusCode();

        await using var stream = await response.Content.ReadAsStreamAsync(cancellationToken);
        using var json = await JsonDocument.ParseAsync(stream, cancellationToken: cancellationToken);

        var translated = ExtractTranslation(json.RootElement);
        var result = BuildResult(cleanText, translated);
        Cache(key, result);
        return result;
    }

    private static TranslationResult BuildResult(string originalText, string translatedText)
    {
        var words = CountWords(originalText);
        if (words < 2)
            return new TranslationResult(translatedText, null, null);

        var context = "Expressão curta; o significado exato depende do contexto da conversa.";
        var example = $"\"{NormalizeExample(originalText)}\" → \"{translatedText}\"";
        return new TranslationResult(translatedText, context, example);
    }

    private static int CountWords(string text)
    {
        var count = 0;
        var inWord = false;

        foreach (var c in text)
        {
            if (char.IsWhiteSpace(c))
            {
                inWord = false;
                continue;
            }

            if (!inWord)
            {
                count++;
                inWord = true;
            }
        }

        return count;
    }

    private static string NormalizeExample(string text)
    {
        var normalized = text.Trim();
        if (normalized.Length == 0)
            return normalized;

        return char.ToUpperInvariant(normalized[0]) + normalized[1..];
    }

    private static string ExtractTranslation(JsonElement root)
    {
        // Estrutura esperada: [[['translated','original',...],...],...]
        if (root.ValueKind != JsonValueKind.Array || root.GetArrayLength() == 0)
            return string.Empty;

        var first = root[0];
        if (first.ValueKind != JsonValueKind.Array)
            return string.Empty;

        var pieces = new List<string>(4);
        foreach (var item in first.EnumerateArray())
        {
            if (item.ValueKind == JsonValueKind.Array && item.GetArrayLength() > 0)
            {
                var segment = item[0].GetString();
                if (!string.IsNullOrEmpty(segment))
                    pieces.Add(segment);
            }
        }

        return string.Concat(pieces);
    }

    private bool TryGetCached(string key, out TranslationResult value)
    {
        lock (_cacheLock)
        {
            return _cache.TryGetValue(key, out value!);
        }
    }

    private void Cache(string key, TranslationResult value)
    {
        lock (_cacheLock)
        {
            if (_cache.ContainsKey(key))
                return;

            _cache[key] = value;
            _cacheOrder.Enqueue(key);

            while (_cacheOrder.Count > MaxCacheItems)
            {
                var old = _cacheOrder.Dequeue();
                _cache.Remove(old);
            }
        }
    }
}
