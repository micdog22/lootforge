
using LootForge.Core;

static string? Arg(string[] args, string name)
{
    for (int i=0;i<args.Length-1;i++)
        if (args[i] == name) return args[i+1];
    return null;
}
static IEnumerable<string> Multi(string[] args, string name)
{
    for (int i=0;i<args.Length;i++)
        if (args[i] == name && i+1 < args.Length) yield return args[i+1];
}

if (args.Length == 0)
{
    Console.WriteLine("LootForge CLI");
    Console.WriteLine("Usage:");
    Console.WriteLine("  validate <config.json>");
    Console.WriteLine("  simulate <config.json> --pulls N [--seed 123] [--csv out.csv] [--level 10] [--tag t]...");
    Console.WriteLine("  template <out.json>");
    return;
}

switch (args[0])
{
    case "validate":
    {
        var path = args.ElementAtOrDefault(1) ?? throw new ArgumentException("faltou <config.json>");
        var cfg = LootConfig.LoadJson(File.ReadAllText(path));
        LootValidator.Validate(cfg);
        Console.WriteLine("OK");
        break;
    }
    case "simulate":
    {
        var path = args.ElementAtOrDefault(1) ?? throw new ArgumentException("faltou <config.json>");
        var pulls = int.TryParse(Arg(args, "--pulls"), out var p) ? p : 10000;
        var seedArg = Arg(args, "--seed");
        ulong seed = 12345UL;
        if (seedArg != null && ulong.TryParse(seedArg, out var s)) seed = s;
        var csv = Arg(args, "--csv");
        var level = int.TryParse(Arg(args, "--level"), out var lv) ? lv : (int?)null;
        var tags = Multi(args, "--tag").ToList();
        var ctx = new LootContext(level, tags.Count>0 ? tags : null);

        var cfg = LootConfig.LoadJson(File.ReadAllText(path));
        var sum = Simulator.Run(cfg, pulls, seed, ctx, csv);

        Console.WriteLine($"Pulls: {sum.Pulls}");
        Console.WriteLine($"Pity Hits: {sum.PityHits}");
        Console.WriteLine("Por raridade:");
        foreach (var kv in sum.CountsByRarity.OrderByDescending(kv => kv.Value))
            Console.WriteLine($"  {kv.Key}: {kv.Value} ({(100.0*kv.Value/sum.Pulls):F2}%)");
        Console.WriteLine("Top itens:");
        foreach (var kv in sum.CountsById.OrderByDescending(kv => kv.Value).Take(10))
            Console.WriteLine($"  {kv.Key}: {kv.Value} ({(100.0*kv.Value/sum.Pulls):F2}%)");
        if (csv != null) Console.WriteLine($"CSV: {csv}");
        break;
    }
    case "template":
    {
        var outPath = args.ElementAtOrDefault(1) ?? "loot-template.json";
        var sample = File.ReadAllText(Path.Combine(AppContext.BaseDirectory, "samples", "starter.json"));
        Directory.CreateDirectory(Path.GetDirectoryName(outPath) ?? ".");
        File.WriteAllText(outPath, sample);
        Console.WriteLine($"Gerado template em {outPath}");
        break;
    }
    default:
        Console.WriteLine($"Comando desconhecido: {args[0]}");
        break;
}
