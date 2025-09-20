
namespace LootForge.Core;

public static class Simulator
{
    public static SimulationSummary Run(LootConfig cfg, int pulls, ulong seed, LootContext ctx, string? csvPath = null)
    {
        var table = LootTable.FromConfig(cfg);
        var rng = new Rng64(seed);
        var countsById = new Dictionary<string, int>(StringComparer.OrdinalIgnoreCase);
        var countsByRarity = new Dictionary<string, int>(StringComparer.OrdinalIgnoreCase);
        int pityHits = 0;

        using var csv = csvPath is null ? null : new StreamWriter(csvPath, false, System.Text.Encoding.UTF8);
        if (csv is not null) csv.WriteLine("pull,entryId,rarity,pityHit");

        for (int i = 1; i <= pulls; i++)
        {
            var res = table.Pull(rng, ctx);
            countsById[res.EntryId] = countsById.TryGetValue(res.EntryId, out var c) ? c + 1 : 1;
            countsByRarity[res.Rarity] = countsByRarity.TryGetValue(res.Rarity, out var cr) ? cr + 1 : 1;
            if (res.PityTriggered) pityHits++;
            if (csv is not null) csv.WriteLine($"{i},{res.EntryId},{res.Rarity},{(res.PityTriggered ? 1 : 0)}");
        }

        return new SimulationSummary(pulls, countsById, countsByRarity, pityHits);
    }
}

public record SimulationSummary(
    int Pulls,
    Dictionary<string, int> CountsById,
    Dictionary<string, int> CountsByRarity,
    int PityHits
);
