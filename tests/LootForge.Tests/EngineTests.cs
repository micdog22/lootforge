
using LootForge.Core;
using Xunit;
using FluentAssertions;

namespace LootForge.Tests;

public class EngineTests
{
    private LootConfig SimpleConfig() => new LootConfig(
        "test",
        new List<LootEntry>
        {
            new("a","common", 1),
            new("b","common", 3)
        },
        new Dictionary<string, PityRule>()
    );

    [Fact]
    public void Weights_Should_Approximate_Ratios()
    {
        var cfg = SimpleConfig();
        var sum = Simulator.Run(cfg, 100000, 1, new LootContext(), null);
        double pa = sum.CountsById["a"] / (double)sum.Pulls;
        double pb = sum.CountsById["b"] / (double)sum.Pulls;
        (pb/pa).Should().BeInRange(2.5, 3.5);
    }

    [Fact]
    public void Pity_Should_Guarantee_Rarity_Eventually()
    {
        var cfg = new LootConfig(
            "pity",
            new List<LootEntry>
            {
                new("c","common", 99),
                new("r","rare",   1)
            },
            new Dictionary<string, PityRule> { ["rare"] = new(10) }
        );
        var table = LootTable.FromConfig(cfg);
        var rng = new Rng64(123);
        var ctx = new LootContext();
        bool sawRare = false;
        int maxGap = 0, currentGap = 0;
        for (int i=0;i<1000;i++)
        {
            var res = table.Pull(rng, ctx);
            if (res.Rarity == "rare") { sawRare = true; currentGap = 0; }
            else { currentGap++; maxGap = Math.Max(maxGap, currentGap); }
        }
        sawRare.Should().BeTrue();
        maxGap.Should().BeLessOrEqualTo(11); // 10 threshold + 1 pull for garantia
    }
}
