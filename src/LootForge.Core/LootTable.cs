
namespace LootForge.Core;

public class LootTable
{
    private readonly List<LootEntry> _entries;
    private readonly Dictionary<string, PityRule> _pity;
    private readonly Dictionary<string, int> _counters = new(StringComparer.OrdinalIgnoreCase);

    private LootTable(List<LootEntry> entries, Dictionary<string, PityRule> pity)
    {
        _entries = entries;
        _pity = pity;
        foreach (var r in pity.Keys) _counters[r] = 0;
    }

    public static LootTable FromConfig(LootConfig cfg)
    {
        LootValidator.Validate(cfg);
        return new LootTable(cfg.Entries, cfg.PityRules);
    }

    private static bool Eligible(LootEntry e, LootContext ctx)
    {
        if (ctx.Level.HasValue)
        {
            if (e.MinLevel.HasValue && ctx.Level.Value < e.MinLevel.Value) return false;
            if (e.MaxLevel.HasValue && ctx.Level.Value > e.MaxLevel.Value) return false;
        }
        if (e.Tags is not null && e.Tags.Count > 0)
        {
            var playerTags = ctx.Tags ?? Array.Empty<string>();
            foreach (var t in e.Tags)
                if (!playerTags.Contains(t, StringComparer.OrdinalIgnoreCase)) return false;
        }
        return true;
    }

    public PullResult Pull(Rng64 rng, LootContext ctx)
    {
        // 1) Checar pity ativo
        string? forcedRarity = null;
        foreach (var (rarity, rule) in _pity)
        {
            if (_counters.TryGetValue(rarity, out var c) && c >= rule.Threshold)
            {
                forcedRarity = rarity;
                break;
            }
        }

        LootEntry? chosen = null;
        bool pityTriggered = false;

        var pool = _entries.Where(e => Eligible(e, ctx)).ToList();
        if (pool.Count == 0) throw new Exception("Nenhum item elegível para o contexto.");

        if (forcedRarity is not null)
        {
            var forcedPool = pool.Where(e => string.Equals(e.Rarity, forcedRarity, StringComparison.OrdinalIgnoreCase)).ToList();
            if (forcedPool.Count > 0)
            {
                chosen = WeightedPick(forcedPool, rng);
                pityTriggered = true;
            }
            // se não houver itens dessa raridade elegíveis, cai no sorteio normal
        }

        if (chosen is null)
            chosen = WeightedPick(pool, rng);

        // atualizar contadores
        foreach (var rar in _pity.Keys.ToList())
        {
            if (string.Equals(chosen.Rarity, rar, StringComparison.OrdinalIgnoreCase))
                _counters[rar] = 0;
            else
                _counters[rar] = _counters.TryGetValue(rar, out var c) ? c + 1 : 1;
        }

        return new PullResult(chosen.Id, chosen.Rarity, pityTriggered);
    }

    private static LootEntry WeightedPick(List<LootEntry> pool, Rng64 rng)
    {
        double sum = 0;
        for (int i = 0; i < pool.Count; i++) sum += pool[i].Weight;
        var r = rng.NextDouble() * sum;
        double acc = 0;
        for (int i = 0; i < pool.Count; i++)
        {
            acc += pool[i].Weight;
            if (r <= acc) return pool[i];
        }
        return pool[^1];
    }
}
