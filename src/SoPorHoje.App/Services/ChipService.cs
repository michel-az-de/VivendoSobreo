using SoPorHoje.App.Constants;
using SoPorHoje.App.Models;

namespace SoPorHoje.App.Services;

public class ChipService
{
    private readonly DatabaseService _db;

    public ChipService(DatabaseService db)
    {
        _db = db;
    }

    public async Task<List<SobrietyChip>> GetChipsStatusAsync(int soberDays)
    {
        var events = await _db.GetAllChipEventsAsync();
        var earnedSet = events.ToDictionary(e => e.ChipRequiredDays, e => e);

        SobrietyChip? currentChip = null;

        var chips = ChipDefinitions.Chips.Select(c =>
        {
            var isEarned = soberDays >= c.Days;
            var chip = new SobrietyChip
            {
                RequiredDays = c.Days,
                Name = c.Name,
                Label = c.Label,
                ChipColor = c.Color,
                BgColor = c.BgColor,
                Emoji = c.Emoji,
                ShortLabel = c.ShortLabel,
                IsEarned = isEarned,
                CelebrationShown = earnedSet.ContainsKey(c.Days) && earnedSet[c.Days].CelebrationShown,
            };
            if (isEarned) currentChip = chip;
            return chip;
        }).ToList();

        if (currentChip != null)
            currentChip.IsCurrent = true;

        return chips;
    }

    public SobrietyChip? GetCurrentChip(int soberDays)
    {
        var def = ChipDefinitions.Chips.LastOrDefault(c => soberDays >= c.Days);
        if (def == default) return null;
        return new SobrietyChip
        {
            RequiredDays = def.Days,
            Name = def.Name,
            Label = def.Label,
            ChipColor = def.Color,
            BgColor = def.BgColor,
            Emoji = def.Emoji,
            ShortLabel = def.ShortLabel,
            IsEarned = true,
            IsCurrent = true,
        };
    }

    public (int Days, string Name, string Label, string Color, string BgColor, string Emoji, string ShortLabel)? GetNextChipDef(int soberDays)
    {
        var next = ChipDefinitions.Chips.FirstOrDefault(c => c.Days > soberDays);
        return next == default ? null : next;
    }

    public async Task<ChipEarnedEvent?> CheckAndRecordNewChipAsync(int soberDays)
    {
        foreach (var chip in ChipDefinitions.Chips)
        {
            if (soberDays >= chip.Days)
            {
                var existing = await _db.GetChipEventAsync(chip.Days);
                if (existing is null)
                {
                    var evt = new ChipEarnedEvent
                    {
                        ChipRequiredDays = chip.Days,
                        EarnedAt = DateTime.UtcNow,
                        CelebrationShown = false,
                    };
                    await _db.SaveChipEventAsync(evt);
                }
            }
        }

        // Return first uncelebrated chip
        var events = await _db.GetAllChipEventsAsync();
        var uncelebrated = events.FirstOrDefault(e => !e.CelebrationShown);
        return uncelebrated;
    }

    public async Task MarkCelebrationShownAsync(int requiredDays)
    {
        var evt = await _db.GetChipEventAsync(requiredDays);
        if (evt is not null)
        {
            evt.CelebrationShown = true;
            await _db.SaveChipEventAsync(evt);
        }
    }
}
