
using System.Runtime.CompilerServices;

namespace LootForge.Core;

// SplitMix64 PRNG: rápido, simples, determinístico.
public struct Rng64
{
    private ulong _state;
    public Rng64(ulong seed) => _state = seed;

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    private ulong NextUInt64()
    {
        ulong z = (_state += 0x9E3779B97F4A7C15UL);
        z = (z ^ (z >> 30)) * 0xBF58476D1CE4E5B9UL;
        z = (z ^ (z >> 27)) * 0x94D049BB133111EBUL;
        return z ^ (z >> 31);
    }

    public double NextDouble()
    {
        // 53 bits precision in [0,1)
        return (NextUInt64() >> 11) * (1.0 / (1UL << 53));
    }

    public int NextInt(int minInclusive, int maxExclusive)
    {
        if (maxExclusive <= minInclusive) return minInclusive;
        var span = (ulong)(maxExclusive - minInclusive);
        var val = NextUInt64() % span;
        return (int)val + minInclusive;
    }
}
