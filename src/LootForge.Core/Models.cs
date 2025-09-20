
namespace LootForge.Core;

public record LootContext(int? Level = null, IReadOnlyCollection<string>? Tags = null);

public record PityRule(int Threshold);

public record LootEntry(
    string Id,
    string Rarity,
    double Weight,
    int? MinLevel = null,
    int? MaxLevel = null,
    IReadOnlyCollection<string>? Tags = null
);

public record LootConfig(string Name, List<LootEntry> Entries, Dictionary<string, PityRule> PityRules)
{
    public static LootConfig LoadJson(string json)
    {
        var cfg = System.Text.Json.JsonSerializer.Deserialize<LootConfig>(json, new System.Text.Json.JsonSerializerOptions
        {
            PropertyNameCaseInsensitive = true
        });
        if (cfg is null) throw new Exception("Config inválida.");
        return cfg;
    }
}

public record PullResult(string EntryId, string Rarity, bool PityTriggered);
