using System;

namespace Thatchwick.Core
{
    /// <summary>
    /// Rastgelelik tohumları. Her oyunun bir ana tohumu vardır (kayıtta saklanır); alt sistemler bundan kendi
    /// akış tohumlarını türetir. "epoch" her yüklemede bir artar: üreteçlerin iç durumu kaydedilmediği için
    /// yüklemeden sonra aynı zar dizisi baştan başlamasın. Gün ve tohumdan yeniden üretilen şeyler (pazar teklifleri,
    /// hava) epoch'suz türetilir ki yüklemeden sonra aynı kalsın.
    /// </summary>
    public static class Seeds
    {
        /// <summary>Yeni oyun için ana tohum (hiçbir zaman 0 değildir: 0 "eski kayıt, tohum yok" demektir).</summary>
        public static int NewMaster()
        {
            unchecked
            {
                int s = Guid.NewGuid().GetHashCode() ^ (Environment.TickCount * 397);
                return s == 0 ? 1 : s;
            }
        }

        /// <summary>Ana tohumdan bir akış tohumu türetir (pozitif, belirleyici).</summary>
        public static int Derive(int master, int stream, int epoch = 0)
        {
            unchecked
            {
                uint h = (uint)master;
                h ^= (uint)stream * 0x9E3779B9u;
                h = (h ^ (h >> 16)) * 0x85EBCA6Bu;
                h ^= (uint)epoch * 0x27D4EB2Fu;
                h = (h ^ (h >> 13)) * 0xC2B2AE35u;
                h ^= h >> 16;
                int r = (int)(h & 0x7FFFFFFFu);
                return r == 0 ? 1 : r;
            }
        }
    }
}
