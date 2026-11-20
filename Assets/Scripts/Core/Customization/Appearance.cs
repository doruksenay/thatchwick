using System;
using System.Collections.Generic;
using System.Linq;

namespace Thatchwick.Core.Customization
{
    public enum BodyBuild
    {
        Slim = 0,    // İnce
        Average = 1, // Orta
        Sturdy = 2,  // Gürbüz
        Plump = 3    // Dolgun (17.11.2026)
    }

    /// <summary>Beden çerçevesi: cinsiyetten bağımsız seçilir (karar 17.11.2026).</summary>
    public enum BodyFrame
    {
        Masculine = 0,
        Neutral = 1,
        Feminine = 2
    }

    /// <summary>Ses tınısı (doküman 14, 3B). Perde ayrı tutulur (-4..+4 yarım ton).</summary>
    public enum VoiceTimbre
    {
        Deep = 0,   // Kalın
        Warm = 1,   // Sıcak
        Clear = 2,  // Berrak
        High = 3    // İnce
    }

    /// <summary>
    /// Bir karakterin görünümü: her özellik için tür (kimlik) ve renk. Kimlikler AppearanceCatalog'daki
    /// seçeneklere karşılık gelir ("none" = yok). Katmanlı sprite, portre ve palet değişimi bu verilerden çizilir.
    /// Kalıtsal özellikler ayrıca Genome'da tutulur (gizli); burada yalnızca görünen hâl vardır.
    /// </summary>
    public sealed class Appearance
    {
        public const string None = "none";
        public const int MinPitch = -4;
        public const int MaxPitch = 4;

        // --- Beden ---
        public BodyBuild Build { get; set; } = BodyBuild.Average;
        public BodyFrame Frame { get; set; } = BodyFrame.Neutral;
        /// <summary>Boy kaldırıldı (karar 17.11.2026): her zaman 100. Eski kayıt uyumu için alan duruyor.</summary>
        public int HeightPercent { get; set; } = 100;
        public Rgb SkinColor { get; set; } = new Rgb(232, 195, 158);

        // --- Yüz (çoğu portrede görünür) ---
        public string FaceShapeId { get; set; } = "oval";
        public string EyeShapeId { get; set; } = "almond";
        public Rgb EyeColor { get; set; } = new Rgb(90, 60, 40);
        /// <summary>İki renkli göz: sağ göz rengi. Null ise iki göz aynı.</summary>
        public Rgb? EyeColorRight { get; set; }
        public string BrowId { get; set; } = "natural";
        /// <summary>Kaş rengi saçı izler (varsayılan) ya da ayrı seçilir.</summary>
        public bool BrowFollowsHair { get; set; } = true;
        public Rgb BrowColor { get; set; } = new Rgb(90, 60, 35);
        public string NoseId { get; set; } = "straight";
        public string MouthId { get; set; } = "neutral";
        public string EarsId { get; set; } = "normal";
        /// <summary>Kirpik (yalnız portre).</summary>
        public string LashesId { get; set; } = "normal";

        // --- Saç ve sakal (modüler, belge 43b: kütle + ön + eklenti + süs; bıyık + sakal + favori) ---
        /// <summary>Saç kütlesi: uzunluk ve doku (arka ve yanlar). Eski tam stil kimlikleri Normalize'da kütle + eklentiye çevrilir.</summary>
        public string HairStyleId { get; set; } = "short";
        public Rgb HairColor { get; set; } = new Rgb(90, 60, 35);
        /// <summary>Saçın önü: kâkül, ayrık, alın açık ("none" = alın açık).</summary>
        public string BangsId { get; set; } = None;
        /// <summary>Saç eklentisi: örgü, topuz, at kuyruğu. Kütle yeterince uzun olmalı (OptionDef.Level).</summary>
        public string HairExtraId { get; set; } = None;
        /// <summary>Saç süsü: kurdele, firkete, çiçek vb.</summary>
        public string HairOrnamentId { get; set; } = None;
        public Rgb HairOrnamentColor { get; set; } = new Rgb(140, 50, 45);
        public string MustacheId { get; set; } = None;
        public string BeardId { get; set; } = None;
        /// <summary>Favori (yanak yanı sakal).</summary>
        public string SideburnsId { get; set; } = None;
        /// <summary>Kirli sakal dokusu 0-3 (betikle çizilir, 0 gen).</summary>
        public int StubbleLevel { get; set; }
        public const int MaxStubble = 3;
        public Rgb FacialHairColor { get; set; } = new Rgb(90, 60, 35);

        // --- Geçici saç bakımı (belge 59a §6, belge 65): yalnız görünüş; genoma ve çocuğa geçmez, yıkanınca gider ---
        /// <summary>Saç tonu (<see cref="HairTints"/>: henna, chamomile, walnut, purple). Null = yok.</summary>
        public string HairTintId { get; set; }
        /// <summary>Saç yağı parlaklığı.</summary>
        public bool HairShine { get; set; }
        public bool HasHairTreatment => HairTintId != null || HairShine;
        /// <summary>Görünen saç rengi: kırlaşma (0..1), üstüne ton ve yağ. Kaş ve sakal doğal rengi izler.</summary>
        public Rgb ShownHairColor(double grey = 0.0) => HairTints.Apply(AgingLook.GrayedHair(HairColor, grey), HairTintId, HairShine);
        /// <summary>Yıkanma: ton ve yağ gider.</summary>
        public bool WashHair()
        {
            if (!HasHairTreatment) return false;
            HairTintId = null;
            HairShine = false;
            return true;
        }

        /// <summary>
        /// Eski tek alanlı sakal/bıyık (kayıt ve eski kod uyumu). Okuma: sakal varsa sakal, yoksa bıyık.
        /// Yazma: eski kimliği yeni iki alana çevirir ("mustache" bıyık, diğerleri sakal).
        /// </summary>
        public string FacialHairId
        {
            get => BeardId != None ? BeardId : MustacheId;
            set
            {
                if (string.IsNullOrEmpty(value) || value == None) { BeardId = None; MustacheId = None; }
                else if (value == "mustache") { MustacheId = "mustache"; BeardId = None; }
                else { BeardId = value; }
            }
        }

        // --- İşaretler (ten detayı, iz, dövme, takı, makyaj) ---
        private List<Mark> _marks = new List<Mark>();
        public List<Mark> Marks => _marks;

        /// <summary>
        /// Eski tek yuvalı yüz işareti (kayıt ve eski kod uyumu). Okuma: ilk ten detayı ya da seçilmiş iz.
        /// Yazma: eski seçimleri işaretlere çevirir ("scar" yanakta iz, diğerleri yüzde ten detayı).
        /// </summary>
        public string FaceMarkId
        {
            get
            {
                var m = Marks.FirstOrDefault(x => (x.Kind == MarkKind.SkinDetail && x.Zone == "face") || (x.Kind == MarkKind.Scar && x.Source == MarkSource.Chosen));
                if (m == null) return None;
                return m.Kind == MarkKind.Scar ? "scar" : m.Id;
            }
            set
            {
                Marks.RemoveAll(x => (x.Kind == MarkKind.SkinDetail && x.Zone == "face") || (x.Kind == MarkKind.Scar && x.Source == MarkSource.Chosen && x.Zone == "cheek"));
                if (string.IsNullOrEmpty(value) || value == None) return;
                if (value == "scar") Marks.Add(new Mark { Kind = MarkKind.Scar, Id = "scratch", Zone = "cheek", Fade = 2 });
                else Marks.Add(new Mark { Kind = MarkKind.SkinDetail, Id = value, Zone = "face" });
            }
        }

        // --- Kıyafet ---
        public string OutfitId { get; set; } = "tunic";
        public Rgb OutfitColor { get; set; } = new Rgb(160, 110, 70);
        /// <summary>Kemer, kenar ve pantolon rengi.</summary>
        public Rgb TrimColor { get; set; } = new Rgb(70, 50, 35);
        public string PatternId { get; set; } = "plain";
        public string BottomId { get; set; } = "trousers";
        public string ShoesId { get; set; } = "boots";
        public string OuterId { get; set; } = None;
        public Rgb OuterColor { get; set; } = new Rgb(90, 70, 50);
        public string HatId { get; set; } = None;
        public Rgb HatColor { get; set; } = new Rgb(120, 90, 60);

        // --- Ses ---
        public VoiceTimbre Voice { get; set; } = VoiceTimbre.Warm;
        public int VoicePitch { get; set; }
        public string LaughId { get; set; } = "chuckle";

        /// <summary>Görünen kaş rengi.</summary>
        public Rgb EffectiveBrowColor => BrowFollowsHair ? HairColor : BrowColor;

        public Appearance Clone()
        {
            var c = (Appearance)MemberwiseClone();
            // MemberwiseClone listeyi paylaşır; derin kopya.
            c._marks = new List<Mark>(_marks.Count);
            foreach (var m in _marks) c._marks.Add(m.Clone());
            return c;
        }

        /// <summary>Önbellek anahtarı: aynı görünüm aynı metni verir.</summary>
        public string Key() =>
            $"{(int)Build}|{(int)Frame}|{SkinColor}|{FaceShapeId}|{EyeShapeId}|{EyeColor}|{EyeColorRight}|{BrowId}|{EffectiveBrowColor}|{NoseId}|{MouthId}|{EarsId}" +
            $"|{LashesId}|{HairStyleId}|{HairColor}|{BangsId}|{HairExtraId}|{HairOrnamentId}|{HairOrnamentColor}|{MustacheId}|{BeardId}|{SideburnsId}|{StubbleLevel}|{FacialHairColor}|{HairTintId}|{(HairShine ? 1 : 0)}" +
            $"|{OutfitId}|{OutfitColor}|{TrimColor}|{PatternId}|{BottomId}|{ShoesId}|{OuterId}|{OuterColor}|{HatId}|{HatColor}" +
            $"|{string.Join(",", Marks.Select(m => m.Key()))}";
    }
}
