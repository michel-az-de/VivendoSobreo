using SoPorHoje.App.Models;

namespace SoPorHoje.App.Services;

/// <summary>
/// Gerencia sorteio de textos literários usando sistema de "baralho":
/// cada texto é sorteado uma vez antes de qualquer repetição.
/// O baralho abrange todos os livros combinados.
/// Estado persiste entre sessões via Preferences.
/// </summary>
public class LiteratureService
{
    private readonly DatabaseService _db;
    private readonly Random _rng = new();

    private const string AllBooksKey = "lit_deck_all";

    public LiteratureService(DatabaseService db)
    {
        _db = db;
    }

    /// <summary>
    /// Sorteia o próximo texto do baralho combinado de todos os livros.
    /// Nunca repete até esgotar todos os textos.
    /// </summary>
    public async Task<LiteratureText?> DrawNextAsync()
    {
        var all = await _db.GetAllLiteratureTextsAsync(null);
        if (all.Count == 0) return null;

        var allIds = all.Select(t => t.Id).ToHashSet();
        var remaining = GetRemainingIds(allIds);

        // Se esgotou, reseta o baralho com todos os ids embaralhados
        if (remaining.Count == 0)
        {
            remaining = Shuffle(allIds.ToList());
            SaveRemainingIds(remaining);
        }

        // Pega o primeiro da fila embaralhada
        var nextId = remaining[0];
        remaining.RemoveAt(0);
        SaveRemainingIds(remaining);

        return all.FirstOrDefault(t => t.Id == nextId);
    }

    /// <summary>Quantos textos ainda faltam ser sorteados neste ciclo.</summary>
    public async Task<(int Remaining, int Total)> GetProgressAsync()
    {
        var all = await _db.GetAllLiteratureTextsAsync(null);
        if (all.Count == 0) return (0, 0);
        var allIds = all.Select(t => t.Id).ToHashSet();
        var remaining = GetRemainingIds(allIds);
        return (remaining.Count, all.Count);
    }

    public async Task<List<(string BookId, string BookTitle, int Count)>> GetAvailableBooksAsync()
    {
        return await _db.GetLiteratureBookSummariesAsync();
    }

    // --- Shuffle bag persistence ---

    private List<int> GetRemainingIds(HashSet<int> validIds)
    {
        var stored = Preferences.Get(AllBooksKey, "");
        if (string.IsNullOrEmpty(stored)) return new List<int>();

        return stored
            .Split(',', StringSplitOptions.RemoveEmptyEntries)
            .Select(s => int.TryParse(s, out var id) ? id : -1)
            .Where(id => id >= 0 && validIds.Contains(id))
            .ToList();
    }

    private void SaveRemainingIds(List<int> ids)
    {
        Preferences.Set(AllBooksKey, string.Join(",", ids));
    }

    private List<int> Shuffle(List<int> list)
    {
        var copy = new List<int>(list);
        for (int i = copy.Count - 1; i > 0; i--)
        {
            int j = _rng.Next(i + 1);
            (copy[i], copy[j]) = (copy[j], copy[i]);
        }
        return copy;
    }
}
