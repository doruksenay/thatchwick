using System;
using System.Collections.Generic;
using Thatchwick.Core.Characters;
using Thatchwick.Core.Content;
using Thatchwick.Core.Jobs;
using Thatchwick.Core.World;

namespace Thatchwick.Core.Work
{
    /// <summary>
    /// Hasat emirleri (Project belgeleri 58-B, 59 karar 1-2): olgun ekine tek dokunuş, parmağı sürükleyerek ekleme,
    /// "tarlayı biç" ve tırpan. İş sırası kısa olduğu için (JobQueue) bekleyen ekinler burada tutulur; karakter bir ekini
    /// biçince sıradaki en yakın ekin kendiliğinden emredilir. Karakter başka bir işe geçerse emir düşer.
    /// </summary>
    public sealed class HarvestOrders
    {
        public const string HarvestId = "harvest";
        /// <summary>Sararmış ekini sökme (59 §3): tek dokunuş ve sürükleme hasatla aynı çalışır.</summary>
        public const string UprootId = "uproot";
        public const string ScytheTool = "scythe";
        /// <summary>"Tarlayı biç" (bitişik olgun ekinlerin hepsi) bu Çiftçilik seviyesinde açılır; altında tek karo biçilir.</summary>
        public const int FieldLevel = 3;
        /// <summary>Tırpan bir biçişte en çok bu kadar komşu karoyu da biçer.</summary>
        public const int ScytheExtraTiles = 2;
        /// <summary>Tırpanla biçilen ekin aileleri: tahıl, lif ve ot (kök, yaprak ve baklagil orakla ya da elle toplanır).</summary>
        public static readonly string[] ScytheFamilies = { "tahil", "lif", "fig", "ot" };

        private readonly WorldState _world;
        private readonly Dictionary<int, List<int>> _pending = new Dictionary<int, List<int>>();
        private bool _reaping;

        /// <summary>Karaktere iş verir (oyun katmanı iş sırasına ekler); false dönerse emir düşer.</summary>
        public Func<Character, Job, bool> Command { get; set; }
        /// <summary>Hanede tırpan var mı (alet kademesi ≥ 1).</summary>
        public Func<bool> HasScythe { get; set; }
        /// <summary>Tırpanın yan karoları biçmesi için hasadı uygular (InteractionEffects.Apply).</summary>
        public Action<Character, ItemInstance, InteractionDef> Apply { get; set; }
        /// <summary>Tırpan yan karoları da biçti (karakter, biçilen karo sayısı).</summary>
        public event Action<Character, int> Reaped;
        /// <summary>Emir bitti: sırada ekin kalmadı (karakter, biçilen toplam ekin).</summary>
        public event Action<Character, int> Finished;

        private readonly Dictionary<int, int> _done = new Dictionary<int, int>();

        public HarvestOrders(WorldState world)
        {
            _world = world ?? throw new ArgumentNullException(nameof(world));
        }

        public static InteractionDef HarvestOf(ItemDef def)
        {
            if (def == null) return null;
            foreach (var i in def.Interactions) if (i.Id == HarvestId) return i;
            return null;
        }

        public static InteractionDef UprootOf(ItemDef def)
        {
            if (def == null) return null;
            foreach (var i in def.Interactions) if (i.Id == UprootId) return i;
            return null;
        }

        /// <summary>Tek dokunuşla sökülebilir mi: sararmış tarla ekini ve "uproot" etkileşimi var.</summary>
        public static bool IsWitheredCrop(ItemInstance item) =>
            item != null && item.IsPlant && item.Def.IsCrop && item.Withered && UprootOf(item.Def) != null;

        /// <summary>Tek dokunuşluk iş: olgunsa hasat, sararmışsa sökme; ikisi de değilse null.</summary>
        public static string WorkOf(ItemInstance item) => IsRipeCrop(item) ? HarvestId : IsWitheredCrop(item) ? UprootId : null;

        /// <summary>Tek dokunuşla biçilebilir mi: olgun tarla ekini ve "harvest" etkileşimi var.</summary>
        public static bool IsRipeCrop(ItemInstance item) =>
            item != null && item.IsPlant && item.Def.IsCrop && item.IsGrown && HarvestOf(item.Def) != null;

        public static bool IsScythable(ItemDef def) =>
            def != null && def.CropFamily != null && Array.IndexOf(ScytheFamilies, def.CropFamily) >= 0;

        /// <summary>
        /// Başlangıç ekinine bitişik (dört yön, aynı kat) aynı işi bekleyen bütün ekinler: başlangıç olgunsa olgunlar, sararmışsa
        /// sararmışlar; başlangıç da içinde, yakından uzağa.
        /// </summary>
        public static List<ItemInstance> RipeField(WorldState world, ItemInstance start, int max = 400)
        {
            var result = new List<ItemInstance>();
            string work = WorkOf(start);
            if (world == null || work == null) return result;
            var seen = new HashSet<int> { start.Id };
            var queue = new Queue<ItemInstance>();
            queue.Enqueue(start);
            while (queue.Count > 0 && result.Count < max)
            {
                var cur = queue.Dequeue();
                result.Add(cur);
                foreach (var n in Neighbours(world, cur.Position))
                    if (WorkOf(n) == work && seen.Add(n.Id)) queue.Enqueue(n);
            }
            return result;
        }

        private static IEnumerable<ItemInstance> Neighbours(WorldState world, GridPos p)
        {
            yield return world.GetItemAt(new GridPos(p.X + 1, p.Y, p.Z));
            yield return world.GetItemAt(new GridPos(p.X - 1, p.Y, p.Z));
            yield return world.GetItemAt(new GridPos(p.X, p.Y + 1, p.Z));
            yield return world.GetItemAt(new GridPos(p.X, p.Y - 1, p.Z));
        }

        public int PendingCount(Character c) => c != null && _pending.TryGetValue(c.Id, out var list) ? list.Count : 0;

        public bool IsPending(int itemId)
        {
            foreach (var kv in _pending) if (kv.Value.Contains(itemId)) return true;
            return false;
        }

        /// <summary>
        /// Olgun ekine tek dokunuş: Çiftçilik <see cref="FieldLevel"/> ve üstünde bitişik olgun ekinlerin hepsi, altında yalnızca o
        /// ekin emredilir. Önceki emrin yerine geçer. Emredilen ekin sayısını döndürür.
        /// </summary>
        public int OrderFrom(Character c, ItemInstance start)
        {
            if (c == null || WorkOf(start) == null) return 0;
            Cancel(c);
            var field = c.Skills.GetLevel(SkillType.Farming) >= FieldLevel ? RipeField(_world, start) : new List<ItemInstance> { start };
            var list = new List<int>();
            foreach (var it in field) list.Add(it.Id);
            int ordered = list.Count;
            _pending[c.Id] = list;
            _done[c.Id] = 0;
            if (!StartNext(c, start.Position)) { Cancel(c); return 0; }
            return ordered;
        }

        /// <summary>Sürükleyerek ekleme: emir varsa sonuna eklenir, yoksa bu ekinle başlar. Eklendiyse true.</summary>
        public bool Add(Character c, ItemInstance item)
        {
            if (c == null || WorkOf(item) == null) return false;
            if (!_pending.TryGetValue(c.Id, out var list) || list.Count == 0)
            {
                Cancel(c);
                _pending[c.Id] = new List<int> { item.Id };
                _done[c.Id] = 0;
                if (!StartNext(c, item.Position)) { Cancel(c); return false; }
                return true;
            }
            if (list.Contains(item.Id)) return false;
            list.Add(item.Id);
            return true;
        }

        public void Cancel(Character c)
        {
            if (c == null) return;
            _pending.Remove(c.Id);
            _done.Remove(c.Id);
        }

        /// <summary>
        /// Bir etkileşim uygulandı (InteractionEffects.AppliedOn): hasatsa tırpan yan karoları da biçer ve sıradaki ekin emredilir;
        /// başka bir işse karakterin hasat emri düşer.
        /// </summary>
        public void OnApplied(Character c, ItemInstance item, InteractionDef interaction)
        {
            if (c == null || item == null || interaction == null || _reaping) return;
            bool uproot = interaction.Id == UprootId && item.Def.IsCrop;
            bool harvest = (interaction.Id == HarvestId || uproot) && item.Def.IsCrop;
            if (!harvest)
            {
                if (JobRunner.IsWork(interaction)) Cancel(c);
                return;
            }

            int extra = uproot ? 0 : ReapNeighbours(c, item);
            if (extra > 0) Reaped?.Invoke(c, extra);

            if (!_pending.TryGetValue(c.Id, out var list)) return;
            list.Remove(item.Id);
            _done[c.Id] = (_done.TryGetValue(c.Id, out var d) ? d : 0) + 1 + extra;
            if (!StartNext(c, item.Position))
            {
                int total = _done.TryGetValue(c.Id, out var t) ? t : 0;
                Cancel(c);
                if (total > 1) Finished?.Invoke(c, total);
            }
        }

        // Tırpan: tahıl, lif ve otta biçilen karonun dört komşusundan en çok iki olgun ekin aynı vuruşta biçilir.
        private int ReapNeighbours(Character c, ItemInstance harvested)
        {
            if (Apply == null || HasScythe == null || !HasScythe() || !IsScythable(harvested.Def)) return 0;
            int reaped = 0;
            _reaping = true;
            try
            {
                foreach (var n in Neighbours(_world, harvested.Position))
                {
                    if (reaped >= ScytheExtraTiles) break;
                    if (!IsRipeCrop(n) || !IsScythable(n.Def)) continue;
                    Apply(c, n, HarvestOf(n.Def));
                    foreach (var kv in _pending) kv.Value.Remove(n.Id);
                    reaped++;
                }
            }
            finally { _reaping = false; }
            return reaped;
        }

        // Sıradaki en yakın geçerli ekini emreder; hiç kalmadıysa ya da emir verilemediyse false.
        private bool StartNext(Character c, GridPos from)
        {
            if (Command == null || !_pending.TryGetValue(c.Id, out var list)) return false;
            while (list.Count > 0)
            {
                int best = -1, bestDist = int.MaxValue;
                for (int i = list.Count - 1; i >= 0; i--)
                {
                    var it = _world.GetItem(list[i]);
                    if (WorkOf(it) == null) { list.RemoveAt(i); if (best > i) best--; continue; }
                    int dist = Math.Abs(it.Position.X - from.X) + Math.Abs(it.Position.Y - from.Y) + (it.Position.Z == from.Z ? 0 : 1000);
                    if (dist < bestDist) { bestDist = dist; best = i; }
                }
                if (best < 0) return false;
                int id = list[best];
                if (Command(c, Job.UseItem(id, WorkOf(_world.GetItem(id))))) return true;
                list.RemoveAt(best);   // emredilemedi (ulaşılamıyor, sıra dolu): sıradakini dene
            }
            return false;
        }
    }
}
