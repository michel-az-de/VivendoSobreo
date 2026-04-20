using System.Text.Json;
using SoPorHoje.App.Models;

namespace SoPorHoje.App.Services;

/// <summary>
/// Motor de frases aleatórias não-repetitivas retiradas da literatura.
/// Desacoplável — pode ser substituído por uma API futuramente.
/// </summary>
public interface IQuoteEngine
{
    Task<QuoteItem> GetNextQuoteAsync();
}

public class QuoteItem
{
    public string Text { get; set; } = "";
    public string Source { get; set; } = "";
}

public class QuoteEngineService : IQuoteEngine
{
    private readonly DatabaseService _db;
    private const string DeckKey = "quote_deck_json";
    private const string PosKey  = "quote_deck_pos";

    public QuoteEngineService(DatabaseService db)
    {
        _db = db;
    }

    public async Task<QuoteItem> GetNextQuoteAsync()
    {
        var deck = await GetOrBuildDeckAsync();
        if (deck.Count == 0)
            return new QuoteItem { Text = "Um dia de cada vez.", Source = "AA" };

        var pos = Preferences.Get(PosKey, 0);
        if (pos >= deck.Count)
        {
            // Embaralha novamente ao esgotar o deck
            deck = Shuffle(deck);
            SaveDeck(deck);
            pos = 0;
        }

        var item = deck[pos];
        Preferences.Set(PosKey, pos + 1);
        return item;
    }

    private async Task<List<QuoteItem>> GetOrBuildDeckAsync()
    {
        var json = Preferences.Get(DeckKey, "");
        if (!string.IsNullOrEmpty(json))
        {
            try
            {
                var cached = JsonSerializer.Deserialize<List<QuoteItem>>(json);
                if (cached is { Count: > 0 }) return cached;
            }
            catch { /* rebuild */ }
        }

        var deck = await BuildDeckAsync();
        deck = Shuffle(deck);
        SaveDeck(deck);
        Preferences.Set(PosKey, 0);
        return deck;
    }

    private async Task<List<QuoteItem>> BuildDeckAsync()
    {
        var items = new List<QuoteItem>();

        // Fonte 1: citações das reflexões diárias
        var reflections = await _db.GetAllReflectionsAsync();
        foreach (var r in reflections)
        {
            if (!string.IsNullOrWhiteSpace(r.Quote))
                items.Add(new QuoteItem { Text = r.Quote.Trim(), Source = r.Reference ?? "Reflexão Diária" });

            // Primeiro parágrafo do texto longo
            if (!string.IsNullOrWhiteSpace(r.Text))
            {
                var first = r.Text.Split('\n', StringSplitOptions.RemoveEmptyEntries).FirstOrDefault();
                if (!string.IsNullOrWhiteSpace(first) && first.Length >= 30)
                    items.Add(new QuoteItem { Text = first.Trim(), Source = r.Reference ?? "Reflexão Diária" });
            }
        }

        // Fonte 2: textos resumidos das literaturas
        var litTexts = await _db.GetAllLiteratureTextsAsync();
        foreach (var t in litTexts)
        {
            if (!string.IsNullOrWhiteSpace(t.ShortText))
            {
                // Usa primeira frase/parágrafo do texto resumido
                var first = t.ShortText.Split('\n', StringSplitOptions.RemoveEmptyEntries).FirstOrDefault();
                if (!string.IsNullOrWhiteSpace(first) && first.Length >= 30)
                    items.Add(new QuoteItem { Text = first.Trim(), Source = t.BookTitle ?? "Literatura AA" });
            }
        }

        return items;
    }

    private static List<QuoteItem> Shuffle(List<QuoteItem> list)
    {
        var rng = new Random();
        var n = list.Count;
        for (int i = n - 1; i > 0; i--)
        {
            int j = rng.Next(i + 1);
            (list[i], list[j]) = (list[j], list[i]);
        }
        return list;
    }

    private static void SaveDeck(List<QuoteItem> deck)
    {
        var json = JsonSerializer.Serialize(deck);
        Preferences.Set(DeckKey, json);
    }
}
