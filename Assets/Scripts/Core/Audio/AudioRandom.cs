using System;

namespace Thatchwick.Core.Audio
{
    /// <summary>Küçük, tohumlanabilir RNG (xorshift32). Testlerde aynı tohum aynı sonucu verir.</summary>
    public sealed class AudioRandom
    {
        private uint _s;

        public AudioRandom(int seed)
        {
            _s = (uint)seed ^ 0x9E3779B9u;
            if (_s == 0) _s = 0x6C8E9CF5u;
        }

        public uint NextUInt()
        {
            uint x = _s;
            x ^= x << 13;
            x ^= x >> 17;
            x ^= x << 5;
            _s = x;
            return x;
        }

        /// <summary>[0, 1)</summary>
        public float Value() => (NextUInt() >> 8) * (1f / 16777216f);

        public float Range(float min, float max) => min + (max - min) * Value();

        /// <summary>[min, max)</summary>
        public int Range(int min, int max) => max <= min ? min : min + (int)(NextUInt() % (uint)(max - min));

        /// <summary>FNV-1a: süreçler ve platformlar arasında sabit (string.GetHashCode değil).</summary>
        public static int StableHash(string s)
        {
            unchecked
            {
                uint h = 2166136261;
                for (int i = 0; i < s.Length; i++) { h ^= s[i]; h *= 16777619; }
                return (int)h;
            }
        }

        public static int StableHash(long id)
        {
            unchecked
            {
                ulong x = (ulong)id;
                x ^= x >> 33; x *= 0xff51afd7ed558ccdUL; x ^= x >> 33; x *= 0xc4ceb9fe1a85ec53UL; x ^= x >> 33;
                return (int)x;
            }
        }
    }
}
