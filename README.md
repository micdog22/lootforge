
# LootForge (C# • .NET 8)

Ferramenta de **tabelas de loot** com **pity system** e **simulador** para balancear gacha/caixas. Projetada para jogos em C# e Unity, sem dependências externas.

## Por que é útil
- Define loot por **pesos**, **raridade**, **condições** (nível, tags) e **pity por raridade**.
- **Determinístico** com PRNG estável (SplitMix64): replays e testes reproduzíveis.
- **CLI** para validar configs e rodar **simulações** gerando CSV com métricas.
- Biblioteca **Core** para rodar no jogo com a mesma lógica da simulação.
- Testes xUnit, solução `.sln`, Dockerfile opcional, licença MIT.

## Estrutura
```
lootforge/
├─ src/LootForge.Core/           # Engine, RNG, modelos e validação
├─ src/LootForge.Cli/            # CLI: validate / simulate / template
├─ tests/LootForge.Tests/        # xUnit
├─ samples/                      # Config de exemplo (JSON)
├─ .github/workflows/dotnet.yml
├─ LootForge.sln
└─ README.md
```

## Modelo de config (JSON)

```json
{
  "name": "Starter Banner",
  "pityRules": {
    "legendary": { "threshold": 50 },
    "rare": { "threshold": 10 }
  },
  "entries": [
    { "id": "sword_common", "rarity": "common", "weight": 70 },
    { "id": "bow_common", "rarity": "common", "weight": 30 },
    { "id": "staff_rare", "rarity": "rare", "weight": 4, "minLevel": 3 },
    { "id": "blade_legend", "rarity": "legendary", "weight": 1, "tags": ["limited"] }
  ]
}
```

- `pityRules`: por **raridade**, define `threshold` de garantia. Se o jogador ficar N pulls sem obter aquela raridade, o **próximo pull** garante uma recompensa daquela raridade (se existir item elegível).
- Condições opcionais por item: `minLevel`, `maxLevel`, `tags` (requeridas).
- `weight` é relativo entre itens elegíveis da mesma tabela.

## CLI

```bash
# validar config
dotnet run --project src/LootForge.Cli -- validate samples/starter.json

# simular 100k pulls com seed fixa, gerando CSV
dotnet run --project src/LootForge.Cli -- simulate samples/starter.json --pulls 100000 --seed 42 --csv dist/out.csv --level 5 --tag limited
```

O CSV conterá: `pull,entryId,rarity,pityHit` e um sumário no stdout com taxas observadas.

## Uso no jogo (C# / Unity)

```csharp
using LootForge.Core;

// carregar config
var cfg = LootConfig.LoadJson(File.ReadAllText("samples/starter.json"));
var table = LootTable.FromConfig(cfg);

// contexto do jogador
var ctx = new LootContext(level: 5, tags: new []{"limited"});

// RNG determinístico
var rng = new Rng64(seed: 123456);

// executar pull
var result = table.Pull(rng, ctx);
// result.EntryId / result.Rarity / result.PityTriggered
```

## Roadmap
- Variantes por pool (ex.: rate-up), probabilidade condicional por pity parcial.
- Simulador produzindo histogramas CSV e métricas de streaks por raridade.
- Serialização binária compacta para mobile.

## Licença
MIT.
