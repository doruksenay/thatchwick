using System;
using System.Collections.Generic;
using System.Globalization;
using System.Text;
using Thatchwick.Core.Characters;

namespace Thatchwick.Core.Clothing
{
    /// <summary>Takımlar (belge 48 §6.9, belge 51 §2.7): karakter duruma göre kendisi giyer.</summary>
    public enum OutfitKind { Daily = 0, Work = 1, Sleep = 2, Winter = 3, Festive = 4, Maternity = 5 }

    /// <summary>
    /// Tek bir giysi ya da takı (belge 48 §6, §11.4, §12.5-12.6): renk, kenar, desen, işleme, düğme, kalite, dayanıklılık,
    /// kir, yapan, tarih, "Kendi emeğimiz", yadigâr. Envanterdeki eşyadan ayrıdır: dolapta (hane sandığında) ya da bir
    /// karakterin takımındadır.
    /// </summary>
    public sealed class ClothingInstance
    {
        public const float MaxDurability = 100f;
        public const float WornLookBelow = 50f;
        public const float NeedsPatchBelow = 30f;
        public const float DirtyAt = 30f;
        public const int FadeEveryWears = 10;
        public const int MaxQuality = 4;

        public int Id { get; set; }
        /// <summary>Giysi modelinin ya da takının kimliği (clothing.json).</summary>
        public string ModelId { get; set; }
        public bool IsJewelry { get; set; }
        public ClothColor Color { get; set; }
        public ClothColor Trim { get; set; }
        /// <summary>Desen (düz, ince çizgi, ekose ...; belge 48 §6.3); boş = düz.</summary>
        public string Pattern { get; set; } = "";
        public ClothColor PatternColor { get; set; }
        /// <summary>Seçilen kesimler (kol, yaka, boy, yırtmaç).</summary>
        public Dictionary<CutKind, string> Cuts { get; } = new Dictionary<CutKind, string>();
        /// <summary>İşleme yeri, motifi ve ipliği (yün, ipek, sırma); boş = işlemesiz.</summary>
        public string EmbroideryPlace { get; set; } = "";
        public string EmbroideryMotif { get; set; } = "";
        public string EmbroideryThread { get; set; } = "";
        /// <summary>Düğme ya da toka malzemesi (kemik, ahşap, pirinç, gümüş, cam).</summary>
        public string Buttons { get; set; } = "";
        /// <summary>Kalite 0-4 (ısı ve yağmur basamak başına +%25).</summary>
        public int Quality { get; set; }
        public float Durability { get; set; } = MaxDurability;
        public float Dirt { get; set; }
        public int Patches { get; set; }
        /// <summary>Şaplı boya solmaz; şapsız boya her 10 giyilişte bir ton açılır.</summary>
        public bool Fixed { get; set; } = true;
        public int WearCount { get; set; }
        public int MakerId { get; set; }
        public string MakerName { get; set; } = "";
        public double MadeDay { get; set; }
        /// <summary>Hanenin kendi malzemesinden dikildi (27 etiketi).</summary>
        public bool OwnLabor { get; set; }
        public bool Heirloom { get; set; }
        public string HeirloomNote { get; set; } = "";
        /// <summary>Elmasla alındı: yalnızca görünüş, işlevi yok; giyim uyumunda sayılmaz (belge 48 §12.7).</summary>
        public bool Diamond { get; set; }
        /// <summary>Karakter bu evreden çıktı ("küçüldü"); kardeşe saklanır ya da sökülür.</summary>
        public bool Outgrown { get; set; }
        /// <summary>Giyilene ayrıldı (0 = hane sandığı).</summary>
        public int OwnerId { get; set; }
        /// <summary>Hiç giyildi mi ("Yeni giysi" yalnız ilk giyişte).</summary>
        public bool EverWorn { get; set; }
        /// <summary>Malzeme (modelin malzemesi; ipek kaftan gibi seçenekli modelde değişebilir).</summary>
        public string Material { get; set; } = "";
        /// <summary>Takı malzeme basamağı: 0 ip/kemik/ahşap, 1 bakır/pirinç, 2 gümüş, 3 altın.</summary>
        public int JewelryStep { get; set; }
        public bool Gem { get; set; }
        /// <summary>Usta alameti ya da imzalı ustalık eseri.</summary>
        public string Mark { get; set; } = "";

        public bool Torn => !IsJewelry && Durability <= 0f;
        public bool IsDirty => !IsJewelry && Dirt >= DirtyAt;
        public bool NeedsPatch => !IsJewelry && Durability < NeedsPatchBelow && Durability > 0f;
        public bool LooksWorn => !IsJewelry && Durability < WornLookBelow;
        public bool IsEmbroidered => !string.IsNullOrEmpty(EmbroideryMotif);
        /// <summary>Takının etki basamağı (taşlı +1).</summary>
        public int EffectiveStep => Math.Min(4, JewelryStep + (Gem ? 1 : 0));

        /// <summary>Çizimde görünen renk: yıpranmış (50 altı) giysi bir ton solgun.</summary>
        public ClothColor ShownColor => LooksWorn ? DyePalette.Fade(Color) : Color;

        public string CutsKey()
        {
            if (Cuts.Count == 0) return "";
            var sb = new StringBuilder();
            foreach (CutKind k in Enum.GetValues(typeof(CutKind)))
                if (Cuts.TryGetValue(k, out var v)) { if (sb.Length > 0) sb.Append(';'); sb.Append((int)k).Append('=').Append(v); }
            return sb.ToString();
        }

        public void ParseCuts(string s)
        {
            Cuts.Clear();
            if (string.IsNullOrEmpty(s)) return;
            foreach (var part in s.Split(';'))
            {
                int eq = part.IndexOf('=');
                if (eq <= 0) continue;
                if (int.TryParse(part.Substring(0, eq), NumberStyles.Integer, CultureInfo.InvariantCulture, out int k) && Enum.IsDefined(typeof(CutKind), k))
                    Cuts[(CutKind)k] = part.Substring(eq + 1);
            }
        }

        /// <summary>Açık kesimli mi (kayık ya da derin yaka, yüksek yırtmaç).</summary>
        public bool HasAdultCut()
        {
            foreach (var kv in Cuts) if (ClothingAge.IsAdultCutOption(kv.Key, kv.Value)) return true;
            return false;
        }
    }

    /// <summary>Bir takım: yuva başına bir giysi, takılar ayrı.</summary>
    public sealed class Outfit
    {
        public Dictionary<ClothingSlot, int> Slots { get; } = new Dictionary<ClothingSlot, int>();
        public List<int> Jewelry { get; } = new List<int>();
        public bool IsEmpty => Slots.Count == 0 && Jewelry.Count == 0;

        public IEnumerable<int> All()
        {
            foreach (var v in Slots.Values) yield return v;
            foreach (var j in Jewelry) yield return j;
        }

        public bool Contains(int id) => Slots.ContainsValue(id) || Jewelry.Contains(id);
        public void Remove(int id)
        {
            ClothingSlot? hit = null;
            foreach (var kv in Slots) if (kv.Value == id) { hit = kv.Key; break; }
            if (hit.HasValue) Slots.Remove(hit.Value);
            Jewelry.Remove(id);
        }
    }

    /// <summary>Karakterin dolabı: takımlar, yağmur pelerini, gebelik takımının açıklığı.</summary>
    public sealed class Wardrobe
    {
        public int CharacterId { get; }
        public Dictionary<OutfitKind, Outfit> Outfits { get; } = new Dictionary<OutfitKind, Outfit>();
        /// <summary>Yağmurda o anki takımın dış yuvasına giren giysi (oyuncu seçer); 0 = yok.</summary>
        public int RainCloak { get; set; }
        /// <summary>Gebelikte Gebelik takımı Günlük yerine kendiliğinden giyilir (oyuncu kapatabilir).</summary>
        public bool AutoMaternity { get; set; } = true;
        /// <summary>Şu an giyilen takım (son güncelleme).</summary>
        public OutfitKind Current { get; set; }

        public Wardrobe(int characterId) { CharacterId = characterId; }

        public Outfit Get(OutfitKind k)
        {
            if (!Outfits.TryGetValue(k, out var o)) Outfits[k] = o = new Outfit();
            return o;
        }

        public bool Has(OutfitKind k) => Outfits.TryGetValue(k, out var o) && !o.IsEmpty;
    }
}
