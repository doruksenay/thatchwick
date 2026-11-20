using System;
using System.Collections.Generic;
using Thatchwick.Core.Animals;
using Thatchwick.Core.Characters;
using Thatchwick.Core.Localization;
using Thatchwick.Core.World;

namespace Thatchwick.Core.Life
{
    // Easter egg sahneleri (Project belgesi 55, kullanıcı 20.11.2026): Johan Wick ve atsız şövalye haritada yürür;
    // öküz gidip bakar, kedi kutuya girer, köpek ular, kurbağaya taç gelir; kedi süsü yere düşürür, karakter yerine koyar.
    public sealed partial class LifeSystems
    {
        /// <summary>Hayvan pozları, efektler, karakter gösterileri ve yere düşen süsler (çizim oyun katmanında).</summary>
        public SceneBook Scenes { get; } = new SceneBook();
        /// <summary>Haritada yürüyen easter egg ziyaretçileri.</summary>
        public OddWalkers Walkers { get; } = new OddWalkers();

        public const double JohanPetDays = 0.03, JohanWreckDays = 0.015;
        public const string JohanFlag = "walker.johan";

        private int _johanDogId, _johanCount;
        private readonly List<int> _johanCrops = new List<int>();

        private void WireOddScenes()
        {
            Scenes.Now = () => Clock.TotalDays;
            Walkers.Ready += OnWalkerReady;
        }

        /// <summary>Ziyaretçilerin geldiği saat (gün kesri): Johan Wick kuşlukta, şövalye öğleden sonra.</summary>
        public const double JohanArrives = 0.42, KnightArrives = 0.55;
        private readonly List<(double at, Action run)> _sceneQueue = new List<(double, Action)>();

        /// <summary>Sahneyi bugünün verilen saatine erteler (saat geçtiyse sıradaki adımda oynar). Kayda yazılmaz.</summary>
        private void AtDayTime(double dayFraction, Action run) => _sceneQueue.Add((Math.Floor(Clock.TotalDays) + dayFraction, run));

        internal void OddScenesTick(double gameDays)
        {
            if (_sceneQueue.Count > 0)
            {
                double now = Clock.TotalDays;
                for (int i = _sceneQueue.Count - 1; i >= 0; i--)
                {
                    if (now < _sceneQueue[i].at) continue;
                    var run = _sceneQueue[i].run;
                    _sceneQueue.RemoveAt(i);
                    run();
                }
            }
            Walkers.Tick(gameDays);
        }

        // ---- yardımcılar ----

        private static readonly GridPos[] Around4 = { new GridPos(1, 0), new GridPos(-1, 0), new GridPos(0, 1), new GridPos(0, -1) };

        /// <summary>Hedefin yanında durulacak yürünebilir karo (kendisi yürünebilirse kendisi).</summary>
        private GridPos? StandNear(GridPos target, bool allowSelf)
        {
            var map = World.Map;
            if (allowSelf && map.InBounds(target) && map.IsWalkable(target)) return target;
            foreach (var d in Around4)
            {
                var p = new GridPos(target.X + d.X, target.Y + d.Y, target.Z);
                if (map.InBounds(p) && map.IsWalkable(p) && !map.IsWater(p)) return p;
            }
            return null;
        }

        /// <summary>Ziyaretçinin girip çıkacağı kenar karoları: bölgeli haritada çiftliğin, değilse haritanın kenarı.</summary>
        private List<GridPos> EdgeTiles()
        {
            var map = World.Map;
            int x0 = 0, y0 = 0, x1 = map.Width - 1, y1 = map.Height - 1;
            if (Regions.IsRegional(map)) { var f = Regions.Farm; x0 = f.X; y0 = f.Y; x1 = f.X + f.W - 1; y1 = f.Y + f.H - 1; }
            var list = new List<GridPos>();
            for (int x = x0; x <= x1; x++)
                foreach (int y in new[] { y0, y1 })
                { var p = new GridPos(x, y); if (map.IsWalkable(p) && !map.IsWater(p)) list.Add(p); }
            for (int y = y0 + 1; y < y1; y++)
                foreach (int x in new[] { x0, x1 })
                { var p = new GridPos(x, y); if (map.IsWalkable(p) && !map.IsWater(p)) list.Add(p); }
            return list;
        }

        private List<GridPos> PathTo(GridPos from, GridPos to)
        {
            var path = Pathfinder.FindPath(World.Map, from, to);
            if (path == null) return null;
            if (path.Count > 0 && path[0] == from) path.RemoveAt(0);
            return path;
        }

        // ---- Johan Wick ----

        /// <summary>Johan Wick'i haritaya sokar: köpeğe yürür. Yol yoksa false (olay anında, görünmeden işlenir).</summary>
        private bool StartJohanWalk(Animal dog)
        {
            if (Walkers.Find(OddWalkerKind.JohanWick) != null) return true;
            var stand = StandNear(dog.Position, allowSelf: true);
            if (!stand.HasValue || stand.Value.Z != 0) return false;
            var edges = EdgeTiles();
            if (edges.Count == 0) return false;
            var toEdge = Pathfinder.FindPathToAny(World.Map, stand.Value, edges);
            if (toEdge == null || toEdge.Count < 2) return false;
            toEdge.Reverse();                      // kenardan köpeğe
            var entry = toEdge[0];
            toEdge.RemoveAt(0);
            var w = Walkers.Spawn(OddWalkerKind.JohanWick, entry);
            Walkers.Walk(w, toEdge);
            _johanDogId = dog.Id; _johanCount = 0; _johanCrops.Clear();
            return true;
        }

        /// <summary>Tellal meydanda bu kadar durur (bir oyun saati), sonra gider.</summary>
        public const double CrierStandDays = 1.0 / 24.0;
        public const string CrierAction = "cry";

        /// <summary>
        /// Tellalı şehir meydanındaki ilan tahtasının yanına çıkarır (görsel takımı iso_char_crier_r84; yoksa yer tutucu).
        /// Tahta ya da duracak karo yoksa görünmez; duyuru yine yapılır.
        /// </summary>
        private bool SpawnCrier()
        {
            if (!Regions.IsRegional(World.Map)) return false;
            if (Walkers.Find(OddWalkerKind.Crier) != null) return true;
            ItemInstance board = null;
            foreach (var it in World.Items)
                if (!it.IsDecor && it.Def.Id == "item.notice_post" && Regions.Town.Contains(it.Position)) { board = it; break; }
            if (board == null) return false;
            var stand = StandNear(board.Position, allowSelf: false);
            if (!stand.HasValue) return false;
            var w = Walkers.Spawn(OddWalkerKind.Crier, stand.Value);
            var away = new GridPos(stand.Value.X * 2 - board.Position.X, stand.Value.Y * 2 - board.Position.Y, stand.Value.Z);   // tahtaya sırtını döner
            Walkers.Act(w, CrierAction, CrierStandDays, away);
            return true;
        }

        private Animal AnimalById(int id)
        {
            if (_oddAnimals == null) return null;
            foreach (var a in _oddAnimals.Animals) if (a.Id == id && !a.IsDead) return a;
            return null;
        }

        private void OnWalkerReady(OddWalker w)
        {
            if (w.Kind == OddWalkerKind.HorselessKnight || w.Kind == OddWalkerKind.Crier) { w.Gone = true; return; }
            switch (w.Phase)
            {
                case 0:   // köpeğin yanına vardı: sever
                {
                    var dog = AnimalById(_johanDogId);
                    w.Phase = 1;
                    if (dog != null) { Walkers.Act(w, "pet", JohanPetDays, dog.Position); return; }
                    goto case 1;
                }
                case 1:   // sevdi: tarlaya döner
                {
                    var dog = AnimalById(_johanDogId);
                    if (dog != null) dog.Friendship = Math.Min(100f, dog.Friendship + 5f);
                    var crops = new List<ItemInstance>();
                    foreach (var item in World.Items) if (item.IsPlant && item.Def.IsCrop) crops.Add(item);
                    int n = Math.Min(crops.Count, JohanCropsMin + OddEvents.Pick(JohanCropsMax - JohanCropsMin + 1));
                    for (int i = 0; i < n; i++) { int k = OddEvents.Pick(crops.Count); _johanCrops.Add(crops[k].Id); crops.RemoveAt(k); }
                    w.Phase = 2;
                    NextJohanCrop(w);
                    return;
                }
                case 2:   // ekinin yanına vardı: dağıtır
                {
                    var crop = _johanCrops.Count > 0 ? World.GetItem(_johanCrops[0]) : null;
                    w.Phase = 3;
                    if (crop != null) { Walkers.Act(w, "wreck", JohanWreckDays, crop.Position); return; }
                    goto case 3;
                }
                case 3:   // dağıttı: sıradaki
                {
                    if (_johanCrops.Count > 0)
                    {
                        if (World.GetItem(_johanCrops[0]) != null) { World.RemoveItem(_johanCrops[0]); _johanCount++; }
                        _johanCrops.RemoveAt(0);
                    }
                    w.Phase = 2;
                    NextJohanCrop(w);
                    return;
                }
                default:  // çıkışa vardı
                    w.Gone = true;
                    return;
            }
        }

        private void NextJohanCrop(OddWalker w)
        {
            while (_johanCrops.Count > 0)
            {
                var crop = World.GetItem(_johanCrops[0]);
                var stand = crop != null ? StandNear(crop.Position, allowSelf: true) : null;
                var path = stand.HasValue ? PathTo(w.Position, stand.Value) : null;
                if (path != null) { Walkers.Walk(w, path); return; }
                // Ulaşamadığı ekini uzaktan da olsa dağıtır (olayın sonucu aynı kalsın).
                if (crop != null) { World.RemoveItem(crop.Id); _johanCount++; }
                _johanCrops.RemoveAt(0);
            }
            // Bitti: sikkeyi bırakır, çıkışa yürür.
            FinishJohan(AnimalById(_johanDogId)?.Name);
            w.Phase = 4;
            var edges = EdgeTiles();
            var exit = edges.Count > 0 ? Pathfinder.FindPathToAny(World.Map, w.Position, edges) : null;
            if (exit != null && exit.Count > 1) { exit.RemoveAt(0); Walkers.Walk(w, exit); }
            else w.Gone = true;
        }

        /// <summary>Sikke, satır ve o gece beklenen kurt baskınının silinmesi.</summary>
        private void FinishJohan(string dogName)
        {
            OddEvents.Clear(JohanFlag);
            dogName = dogName ?? FirstAnimal("animal.dog")?.Name;
            if (Content.Items.Contains(RelicCoinItem)) House.AddItem(RelicCoinItem, 1);
            Odd(OddityEvents.JohanWick, _johanCount > 0 ? "field" : "nofield", 0, ("animal", dogName ?? ""), ("count", _johanCount));
            if (Wolves != null && Wolves.RaidInDays == 0)
            {
                Wolves.Restore(-1, Wolves.HuntedThisSeason);
                OddityLine?.Invoke(LocText.Of("core.oddity.ev.johan_wick.wolves"), 0);
            }
        }

        // ---- atsız şövalye ----

        /// <summary>Şövalyeyi köy yolunun bir ucundan öbürüne yürütür; yol yoksa false (yalnızca satır).</summary>
        private bool StartKnightWalk()
        {
            var map = World.Map;
            if (!Regions.IsRegional(map) || !Regions.Exists(map, Regions.Road)) return false;
            var r = Regions.Road;
            GridPos? from = null, to = null;
            for (int dy = 0; dy < r.H && (!from.HasValue || !to.HasValue); dy++)
            {
                int y = r.Y + (r.H / 2 + (dy % 2 == 0 ? dy / 2 : -(dy / 2 + 1)) + r.H) % r.H;
                var a = new GridPos(r.X + 1, y); var b = new GridPos(r.X + r.W - 2, y);
                if (!from.HasValue && map.IsWalkable(a) && !map.IsWater(a)) from = a;
                if (!to.HasValue && map.IsWalkable(b) && !map.IsWater(b)) to = b;
            }
            if (!from.HasValue || !to.HasValue) return false;
            var path = PathTo(from.Value, to.Value);
            if (path == null || path.Count == 0) return false;
            var w = Walkers.Spawn(OddWalkerKind.HorselessKnight, from.Value);
            Walkers.Walk(w, path);
            return true;
        }

        // ---- hayvan sahneleri ----

        /// <summary>Hayvanı eşyanın yanına yürütür ve varınca poz verdirir (öküz bakışı, kutu kedisi).</summary>
        private bool SendAnimalToItem(Animal a, ItemInstance item, string pose, double days)
        {
            if (a == null || item == null || _oddAnimals == null) return false;
            var stand = StandNear(item.Position, allowSelf: false);
            if (!stand.HasValue || !_oddAnimals.SendTo(a, stand.Value, days)) return false;
            Scenes.SetPose(a.Id, pose, stand.Value, days + 0.25, item.Id);   // yol payı: poz yalnızca varıp durunca görünür
            return true;
        }

        /// <summary>Oyun katmanı için: hayvan şu an poz veriyor mu.</summary>
        public AnimalPose PoseOf(Animal a) => a == null ? null : Scenes.PoseOf(a.Id, a.Position, a.NextTile.HasValue);

        // ---- kedinin düşürdüğü süs ----

        /// <summary>
        /// Kedi, arsadaki bir masa ya da raf üstü süsü yere iter. Eşya kaybolmaz ve bozulmaz: dünyadaki yeri, kalitesi ve
        /// yadigâr kaydı aynen durur; yalnızca yerde çizilir ve bir karakter "Yerine koy" diyene kadar öyle kalır.
        /// </summary>
        public KnockedItem KnockDecor()
        {
            foreach (var it in World.Items)
            {
                if (!it.IsDecor || Scenes.IsKnocked(it.Id)) continue;
                var parent = World.GetItem(it.ParentId.Value);
                if (parent == null || parent.IsDecor || parent.IsWallMounted || parent.IsCeiling || !World.Map.IsOwned(parent.Position)) continue;
                var floor = StandNear(parent.Position, allowSelf: false);
                if (!floor.HasValue || Scenes.KnockedAt(floor.Value) != null) continue;
                Scenes.Knock(it.Id, floor.Value);
                World.NotifyChanged(it.Id);
                return Scenes.KnockedOf(it.Id);
            }
            return null;
        }

        public string PutBackBlocker(GridPos tile) => Scenes.KnockedAt(tile) == null ? "Burada yere düşmüş bir şey yok." : null;

        /// <summary>Karakter yerdeki süsü alıp eski yerine koyar ("Yerine koy" işi bitince).</summary>
        public bool PutBackKnocked(Character c, GridPos tile)
        {
            var k = Scenes.KnockedAt(tile);
            if (k == null) return false;
            Scenes.PutBack(k.ItemId);
            World.NotifyChanged(k.ItemId);
            var item = World.GetItem(k.ItemId);
            OddityLine?.Invoke(LocText.Of("core.oddity.ev.alex_decor.put_back", ("who", c?.Name ?? ""), ("item", item?.Def.Id ?? "")), c?.Id ?? 0);
            return true;
        }

        private void PruneKnocked() => Scenes.Prune(id => { var it = World.GetItem(id); return it != null && it.IsDecor; });
    }
}
