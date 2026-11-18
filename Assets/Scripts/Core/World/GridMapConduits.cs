using System;
using System.Collections.Generic;

namespace Thatchwick.Core.World
{
    /// <summary>
    /// Su yolu (zemin katta, zeminin altında ya da üstünde ayrı katman). Açık olanlar (ark, oluk) yalnızca dışarıda,
    /// kışın donar; borular toprağın ve döşemenin altından geçer, eve girer, donmaz. Dönem: hepsi Orta Çağ köyünde ya da
    /// manastır/kale su işlerinde bilinen yöntemler (toprak ark, taş ark, ahşap oluk, oyma kütük boru, künk, kurşun boru).
    /// </summary>
    public enum ConduitType : byte
    {
        None = 0,
        /// <summary>Toprak ark: kazılmış hendek. Ucuz, az su taşır, yanındaki ekini sular, donar.</summary>
        Ditch = 1,
        /// <summary>Taş ark: taş döşeli kanal. Çok su taşır, yanındaki ekini sular, donar.</summary>
        StoneChannel = 2,
        /// <summary>Ahşap oluk: ayaklar üstünde yükseltilmiş oluk; yolun üstünden geçer, ekin sulamaz, donar.</summary>
        Flume = 3,
        /// <summary>Kütük boru: ortası oyulmuş kütük. Gömülü, eve girer, az su taşır.</summary>
        WoodPipe = 4,
        /// <summary>Künk: fırında pişmiş toprak boru. Gömülü, orta su taşır.</summary>
        ClayPipe = 5,
        /// <summary>Kurşun boru: en çok suyu taşır; dikine çıkabilir (üst kat tesisatı, su terazisi ister).</summary>
        LeadPipe = 6
    }

    /// <summary>Su yolu türlerinin kuralları (tek yerde: yapı, ağ hesabı ve arayüz buradan okur).</summary>
    public static class Conduits
    {
        public static readonly ConduitType[] All =
            { ConduitType.Ditch, ConduitType.StoneChannel, ConduitType.Flume, ConduitType.WoodPipe, ConduitType.ClayPipe, ConduitType.LeadPipe };

        /// <summary>Açık su yolu: dışarıda, donar, su çarkını döndürür.</summary>
        public static bool IsOpen(ConduitType t) => t == ConduitType.Ditch || t == ConduitType.StoneChannel || t == ConduitType.Flume;
        public static bool IsPipe(ConduitType t) => t == ConduitType.WoodPipe || t == ConduitType.ClayPipe || t == ConduitType.LeadPipe;

        /// <summary>Kapalı odaya (evin içine, döşemenin altına) girebilir mi?</summary>
        public static bool AllowedIndoor(ConduitType t) => IsPipe(t);
        /// <summary>Yolla aynı karoda olabilir mi (boru yolun altından, oluk üstünden geçer)?</summary>
        public static bool AllowedOnRoad(ConduitType t) => t == ConduitType.Flume || IsPipe(t);
        /// <summary>Dondurucu soğukta su taşımaz.</summary>
        public static bool Freezes(ConduitType t) => IsOpen(t);
        /// <summary>Üst kattaki tesisata dikine su çıkarabilir (su terazisi gerekir).</summary>
        public static bool IsRiser(ConduitType t) => t == ConduitType.LeadPipe;

        /// <summary>Günde taşıyabildiği su (birim). Tesisatın istediği su bu darboğazlardan geçer.</summary>
        public static int Capacity(ConduitType t)
        {
            switch (t)
            {
                case ConduitType.Ditch: return 6;
                case ConduitType.StoneChannel: return 12;
                case ConduitType.Flume: return 8;
                case ConduitType.WoodPipe: return 4;
                case ConduitType.ClayPipe: return 8;
                case ConduitType.LeadPipe: return 12;
                default: return 0;
            }
        }

        /// <summary>Su taşırken bu yarıçaptaki (Chebyshev) ekinleri her sabah sular; 0 = sulamaz.</summary>
        public static int IrrigateRadius(ConduitType t) => t == ConduitType.Ditch || t == ConduitType.StoneChannel ? 1 : 0;

        public static int Price(ConduitType t)
        {
            switch (t)
            {
                case ConduitType.Ditch: return 1;
                case ConduitType.StoneChannel: return 3;
                case ConduitType.Flume: return 3;
                case ConduitType.WoodPipe: return 2;
                case ConduitType.ClayPipe: return 3;
                case ConduitType.LeadPipe: return 6;
                default: return 0;
            }
        }

        /// <summary>Envanterde varsa fiyat yerine harcanan malzeme (karo başına 1).</summary>
        public static string Material(ConduitType t)
        {
            switch (t)
            {
                case ConduitType.StoneChannel: return "item.cut_stone";
                case ConduitType.Flume: return "item.plank";
                case ConduitType.WoodPipe: return "item.pipe_wood";
                case ConduitType.ClayPipe: return "item.pipe_clay";
                case ConduitType.LeadPipe: return "item.pipe_lead";
                default: return null;
            }
        }

        /// <summary>Açıldığı hesap seviyesi.</summary>
        public static int Level(ConduitType t)
        {
            switch (t)
            {
                case ConduitType.Flume:
                case ConduitType.WoodPipe: return 2;
                case ConduitType.StoneChannel: return 3;
                case ConduitType.ClayPipe: return 4;
                case ConduitType.LeadPipe: return 6;
                default: return 1;
            }
        }

        public static string Name(ConduitType t)
        {
            switch (t)
            {
                case ConduitType.Ditch: return "Toprak ark";
                case ConduitType.StoneChannel: return "Taş ark";
                case ConduitType.Flume: return "Ahşap oluk";
                case ConduitType.WoodPipe: return "Kütük boru";
                case ConduitType.ClayPipe: return "Künk";
                case ConduitType.LeadPipe: return "Kurşun boru";
                default: return "Su yolu yok";
            }
        }

        /// <summary>Kısa açıklama (yapı modu ipucu).</summary>
        public static string Describe(ConduitType t)
        {
            string where = IsPipe(t) ? "gömülü, eve girer, donmaz" : t == ConduitType.Flume ? "yükseltilmiş, yolun üstünden geçer, donar" : "açık havada, donar";
            string irrigate = IrrigateRadius(t) > 0 ? ", yanındaki ekini sular" : "";
            string riser = IsRiser(t) ? ", üst kata çıkar" : "";
            return $"{Name(t)}: günde {Capacity(t)} su, {where}{irrigate}{riser}";
        }
    }

    public sealed partial class GridMap
    {
        /// <summary>Su yolu döşendi ya da kalktı.</summary>
        public event Action ConduitsChanged;

        private ConduitType[] _conduit;
        private int _conduitCount;

        /// <summary>Haritada en az bir su yolu var mı.</summary>
        public bool HasConduits => _conduitCount > 0;

        public ConduitType GetConduit(GridPos p) => _conduit != null && RoadPlane(p) ? _conduit[PlaneIndex(p)] : ConduitType.None;

        /// <summary>Su yolu döşer ya da kaldırır (yalnızca zemin kat düzlemi).</summary>
        public void SetConduit(GridPos p, ConduitType type)
        {
            if (!RoadPlane(p)) throw new ArgumentOutOfRangeException(nameof(p), $"{p}: su yolu yalnızca zemin katta.");
            if (_conduit == null) _conduit = new ConduitType[Width * Height];
            int i = PlaneIndex(p);
            var old = _conduit[i];
            if (old == type) return;
            if (old != ConduitType.None) _conduitCount--;
            if (type != ConduitType.None) _conduitCount++;
            _conduit[i] = type;
            ConduitsChanged?.Invoke();
        }

        /// <summary>Kayıt ve ağ hesabı: su yolu olan karolar.</summary>
        public IEnumerable<(GridPos pos, ConduitType type)> ConduitTiles()
        {
            if (_conduit == null || _conduitCount == 0) yield break;
            for (int y = 0; y < Height; y++)
                for (int x = 0; x < Width; x++)
                {
                    var t = _conduit[y * Width + x];
                    if (t != ConduitType.None) yield return (new GridPos(x, y), t);
                }
        }
    }
}
