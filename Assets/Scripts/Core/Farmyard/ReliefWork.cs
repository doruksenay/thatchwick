using System;
using System.Collections.Generic;
using Thatchwick.Core.Characters;
using Thatchwick.Core.Jobs;
using Thatchwick.Core.World;

namespace Thatchwick.Core.Farmyard
{
    /// <summary>
    /// Açıkta tuvalet (belge 57b §2, karar K4). Hela, lazımlık ve tuvalet olarak kullanılan ağaç yokken son çare:
    /// karakter önce bir siperin (çalı, ağaç, kaya, büyük eşya) dibini arar, yoksa açığa yapar. Yerde pislik kalır;
    /// gören biri varsa yanına gelip azarlar.
    /// </summary>
    public sealed class ReliefWork : ITileWork
    {
        private readonly WorldState _world;
        private readonly FilthService _filth;

        public ReliefWork(WorldState world, FilthService filth)
        {
            _world = world ?? throw new ArgumentNullException(nameof(world));
            _filth = filth ?? throw new ArgumentNullException(nameof(filth));
        }

        private YardConfig Config => _filth.Config;

        /// <summary>Haritadaki insanlar (hane ve o an arsada olan köylüler); tanık buradan aranır.</summary>
        public Func<IEnumerable<Character>> People { get; set; }

        /// <summary>
        /// Tuvalet yapıldı: (yapan, bırakılan yığın, siper arkasında mıydı, gören kişi ya da null).
        /// Ruh hali ve azar bu olaydan bağlanır.
        /// </summary>
        public event Action<Character, ItemInstance, bool, Character> Relieved;

        public string CanDo(Character actor, GridPos tile, TileAction action, string arg = null) =>
            action != TileAction.Relieve ? "Bu iş şu an yapılamaz." : null;

        public double Duration(Character actor, TileAction action) => Config.ReliefDays;

        public void Apply(Character actor, GridPos tile, TileAction action, string arg = null)
        {
            if (action != TileAction.Relieve || actor == null) return;
            actor.Needs.Restore(NeedType.Bladder, Config.ReliefRestore);
            Finish(actor, actor.Position, HasCoverNear(actor.Position));
        }

        /// <summary>Ağaç ya da çalı dibindeki eski "relieve" etkileşimi de aynı sonucu verir (siper var).</summary>
        public void OnCoveredRelief(Character actor) { if (actor != null) Finish(actor, actor.Position, true); }

        private void Finish(Character actor, GridPos at, bool covered)
        {
            var pile = _filth.Drop(PileKind.Human, at);
            var witness = FindWitness(actor, covered ? Config.WitnessRadiusCovered : Config.WitnessRadiusOpen);
            Relieved?.Invoke(actor, pile, covered, witness);
        }

        // ---------- siper ----------

        /// <summary>Arkasına saklanılacak kadar büyük bir şey: ağaç, çalı, kaya, "cover" etiketli ya da geniş eşya.</summary>
        public bool IsCover(ItemInstance item)
        {
            if (item == null || item.IsDecor || item.IsRug || item.IsWallMounted || item.IsCeiling) return false;
            if (_filth.IsPile(item)) return false;
            if (item.Def.HasTag("cover")) return true;
            if (item.IsPlant) return !item.Def.IsCrop && item.IsGrown;
            return item.Def.Width * item.Def.Height >= 2;
        }

        public bool HasCoverNear(GridPos p)
        {
            for (int dy = -1; dy <= 1; dy++)
                for (int dx = -1; dx <= 1; dx++)
                {
                    if (dx == 0 && dy == 0) continue;
                    if (IsCover(_world.GetItemAt(new GridPos(p.X + dx, p.Y + dy, p.Z)))) return true;
                }
            return false;
        }

        private bool Outdoor(GridPos p) { var room = _world.Map.Rooms.RoomAt(p); return room == null || !room.IsIndoor; }

        private bool Usable(GridPos p)
        {
            var map = _world.Map;
            if (!map.InBounds(p) || !map.IsWalkable(p) || map.IsWater(p) || !Outdoor(p)) return false;
            return _world.GetItemAt(p) == null;
        }

        /// <summary>
        /// Açıkta tuvalet için karo: yakındaki siper dibi öncelikli; yoksa en yakın açık karo. Dışarı çıkış yoksa
        /// (kapalı odada kalmış) karakterin durduğu yer.
        /// </summary>
        public GridPos? FindSpot(Character c)
        {
            if (c == null) return null;
            var origin = c.Position;
            int r = Config.CoverSearchRadius;
            GridPos? best = null; int bestScore = int.MaxValue;
            for (int dy = -r; dy <= r; dy++)
                for (int dx = -r; dx <= r; dx++)
                {
                    var p = new GridPos(origin.X + dx, origin.Y + dy, origin.Z);
                    if (!Usable(p)) continue;
                    int d = Math.Max(Math.Abs(dx), Math.Abs(dy));
                    int score = d * 2 - (HasCoverNear(p) ? r * 2 : 0);   // siper dibi, uzak da olsa açık karonun önündedir
                    if (score < bestScore) { bestScore = score; best = p; }
                }
            return best ?? origin;
        }

        // ---------- tanık ----------

        public Character FindWitness(Character actor, int radius)
        {
            if (People == null || actor == null) return null;
            bool actorOut = Outdoor(actor.Position);
            var actorRoom = _world.Map.Rooms.RoomAt(actor.Position);
            Character best = null; int bestD = int.MaxValue;
            foreach (var w in People())
            {
                if (w == null || w == actor || w.IsDead || w.Asleep || w.Away || w.Stage == AgeStage.Baby) continue;
                if (w.Position.Z != actor.Position.Z) continue;
                int d = Math.Max(Math.Abs(w.Position.X - actor.Position.X), Math.Abs(w.Position.Y - actor.Position.Y));
                if (d > radius || d >= bestD) continue;
                // Duvar arkasındaki görmez: ikisi de açıkta ya da aynı odada olmalı.
                bool wOut = Outdoor(w.Position);
                if (actorOut != wOut) continue;
                if (!actorOut && _world.Map.Rooms.RoomAt(w.Position) != actorRoom) continue;
                best = w; bestD = d;
            }
            return best;
        }

        /// <summary>Azar repliğinin anahtarı (belge 57b §2): görenin yaşına ve yakınlığına göre; +18 paketinde sert sürüm.</summary>
        public static string ScoldLineKey(Character witness, Character actor, bool sameHousehold, bool adultPack, int roll)
        {
            if (witness == null) return null;
            if (witness.Stage == AgeStage.Child) return "core.yard.scold.child";
            if (adultPack && witness.Stage != AgeStage.Teen) return "core.yard.scold.adult." + (1 + Math.Abs(roll) % 3);
            if (witness.Stage == AgeStage.Elder) return "core.yard.scold.elder";
            if (sameHousehold && Math.Abs(roll) % 2 == 0) return "core.yard.scold.family";
            return "core.yard.scold." + (1 + Math.Abs(roll) % 4);
        }
    }
}
