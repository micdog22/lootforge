
namespace LootForge.Core;

public static class LootValidator
{
    public static void Validate(LootConfig cfg)
    {
        if (cfg.Entries.Count == 0) throw new Exception("Config sem entries.");
        foreach (var e in cfg.Entries)
        {
            if (string.IsNullOrWhiteSpace(e.Id)) throw new Exception("Entry id vazio.");
            if (e.Weight <= 0) throw new Exception($"Weight inválido em {e.Id}.");
            if (string.IsNullOrWhiteSpace(e.Rarity)) throw new Exception($"Rarity vazio em {e.Id}.");
            if (e.MinLevel.HasValue && e.MaxLevel.HasValue && e.MinLevel > e.MaxLevel)
                throw new Exception($"MinLevel > MaxLevel em {e.Id}.");
        }
        foreach (var (rarity, rule) in cfg.PityRules)
        {
            if (rule.Threshold <= 0) throw new Exception($"Threshold inválido do pity {rarity}.");
        }
    }
}
