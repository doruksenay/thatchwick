using System;
using System.Collections.Generic;
using Thatchwick.Core.Characters;
using Thatchwick.Core.Content;
using Thatchwick.Core.Localization;
using Thatchwick.Core.World;

namespace Thatchwick.Core.Life
{
    /// <summary>Ev sahibinin tepkisi: söylenir (kullanmaya devam) ya da kaldırır (iş kesilir).</summary>
    public sealed class TrespassReaction
    {
        public Character User, Owner;
        public ItemInstance Item;
        public bool KickedOut;
        public bool Bed;
        /// <summary>Sahibin sözü (Game çözer; "owner" ve "user" adları argüman).</summary>
        public LocText Line;
    }

    /// <summary>
    /// Başkasının evindeki ya da dükkânındaki eşyayı kullanmak (kullanıcı 20.11.2026): oyuncu ve hane üyeleri köylülerin
    /// evlerindeki ve dükkânlarındaki eşyaları kullanabilir; ör. köylünün yatağında uyuyup dinlenebilir. Ev sahibi (o binada
    /// oturan köylü) içerideyken ya da sonradan gelince tepki verir:
    /// - aralarında yakınlık yüksekse (<see cref="FriendAffection"/>) söylenir, kullanmaya izin verir; kullanan küçük bir
    ///   olumsuz ruh hali alır, yakınlık biraz düşer;
    /// - değilse kaldırır: iş kesilir (uyuyan uyanır), kullanan daha ağır bir ruh hali alır, yakınlık daha çok düşer.
    /// Dükkânın zemin katındaki ortak eşyalar (tezgâh, sıra, masa, tabure) açıkken herkesindir; yatak, üst kat, bodrum ve
    /// evlerin her şeyi özeldir. Kendiliğinden (otonomi) başkasının eşyası zaten seçilmez (JobRunner.MayUseForNeeds).
    /// Bir kullanım boyunca bir kez tepki verilir.
    /// </summary>
    public sealed class TrespassService
    {
        public const int FriendAffection = 50;
        public const int ScoldAffection = -2, KickAffection = -6;
        public const int ScoldMood = -2, KickMood = -4, KickBedMood = -5;
        public const double ScoldMoodDays = 0.3, KickMoodDays = 0.5;
        public const string ScoldMoodId = "mood.scolded_by_owner", KickMoodId = "mood.kicked_out";

        private readonly GridMap _map;
        private readonly HashSet<(int user, int item)> _reacted = new HashSet<(int, int)>();

        /// <summary>Köylünün evinin anahtarı (VillageHomes.HomeOf): "home:general", "home:3", "inn:2" ...</summary>
        public Func<Character, string> HomeOf { get; set; }
        public Func<int, int, int> Affection { get; set; }
        public Action<int, int, int> AddAffection { get; set; }

        public TrespassService(GridMap map) { _map = map ?? throw new ArgumentNullException(nameof(map)); }

        /// <summary>Karonun bulunduğu şehir binası (her kat); şehir dışında ya da bölgesiz haritada null.</summary>
        public TownBuilding BuildingAt(GridPos p)
        {
            if (!Regions.IsRegional(_map)) return null;
            foreach (var b in Regions.TownBuildings)
                if (b.Covers(p) || (p.X >= b.X0 && p.X <= b.X1 && p.Y >= b.Y0 && p.Y <= b.Y1)) return b;
            return null;
        }

        public static bool IsBed(InteractionDef inter) => inter != null && inter.Need == NeedType.Energy && inter.Restore > 0;

        /// <summary>Özel kullanım mı: evlerde her şey; dükkân ve handa yatak, depo, üst kat ve bodrum.</summary>
        public static bool IsPrivate(TownBuilding b, ItemInstance item, InteractionDef inter)
        {
            if (b == null || item == null) return false;
            if (b.Kind == "house") return true;
            if (IsBed(inter)) return true;
            if (item.Position.Z != 0) return true;
            return item.Def.Storage > 0;
        }

        /// <summary>Bu binada oturan mı: evinin anahtarı binanın sahibi; han odaları hancının binasında.</summary>
        public bool LivesIn(Character c, TownBuilding b)
        {
            if (c == null || b == null || c.IsDead || !c.IsVillager) return false;
            string home = HomeOf?.Invoke(c);
            if (string.IsNullOrEmpty(home)) return false;
            if (b.Owner != null && home == b.Owner) return true;
            return b.Kind == "inn" && VillageHomes.IsInn(home);
        }

        /// <summary>Kullanan eşyanın başında mı (ayak izinde ya da yanında, aynı kat).</summary>
        public static bool AtItem(Character user, ItemInstance item)
        {
            var a = user.Position; var b = item.Position;
            return a.Z == b.Z && Math.Abs(a.X - b.X) <= 2 && Math.Abs(a.Y - b.Y) <= 2;
        }

        /// <summary>
        /// Kullanan, başkasının eşyasının başındayken binada oturan biri içerideyse tepki verir (bir kullanımda bir kez).
        /// Tepki yoksa null. Etkiler (yakınlık, ruh hali) burada uygulanır; işi kesmek oyun katmanının.
        /// </summary>
        public TrespassReaction Check(Character user, ItemInstance item, InteractionDef inter, IEnumerable<Character> villagers)
        {
            if (user == null || item == null || user.IsVillager || user.IsDead || !AtItem(user, item)) return null;
            var b = BuildingAt(item.Position);
            if (b == null || !IsPrivate(b, item, inter) || _reacted.Contains((user.Id, item.Id))) return null;
            Character owner = null;
            foreach (var v in villagers)
            {
                if (!LivesIn(v, b)) continue;
                var p = v.Position;
                if (p.X >= b.X0 - 1 && p.X <= b.X1 + 1 && p.Y >= b.Y0 - 1 && p.Y <= b.Y1 + 1) { owner = v; break; }
            }
            if (owner == null) return null;
            _reacted.Add((user.Id, item.Id));

            bool bed = IsBed(inter);
            int aff = Affection?.Invoke(user.Id, owner.Id) ?? 0;
            bool friend = aff >= FriendAffection;
            var r = new TrespassReaction { User = user, Owner = owner, Item = item, Bed = bed, KickedOut = !friend };
            if (friend)
            {
                AddAffection?.Invoke(user.Id, owner.Id, ScoldAffection);
                user.AddMoodlet(ScoldMoodId, "Ev sahibi söylendi", ScoldMood, ScoldMoodDays);
                r.Line = LocText.Of(bed ? "core.trespass.scold_bed" : "core.trespass.scold", ("owner", owner.Name), ("user", user.Name), ("item", item.Def.Id));
            }
            else
            {
                AddAffection?.Invoke(user.Id, owner.Id, KickAffection);
                user.AddMoodlet(KickMoodId, bed ? "Yataktan kaldırıldı" : "Başkasının evinden kovuldu", bed ? KickBedMood : KickMood, KickMoodDays);
                r.Line = LocText.Of(bed ? "core.trespass.kick_bed" : "core.trespass.kick", ("owner", owner.Name), ("user", user.Name), ("item", item.Def.Id));
            }
            return r;
        }

        /// <summary>Kullanım bitti: bir sonraki kullanımda yeniden tepki verilebilir.</summary>
        public void Done(Character user, int itemId) { if (user != null) _reacted.Remove((user.Id, itemId)); }

        public void DoneAll(Character user)
        {
            if (user == null) return;
            _reacted.RemoveWhere(k => k.user == user.Id);
        }
    }
}
