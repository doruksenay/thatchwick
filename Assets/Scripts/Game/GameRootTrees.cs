using Thatchwick.Core.Audio;
using Thatchwick.Core.Jobs;
using Thatchwick.Core.Content;
using Thatchwick.Core.Life;
using Thatchwick.Core.Time;
using Thatchwick.Core.World;
using Thatchwick.Game.Audio;
using UnityEngine;

namespace Thatchwick.Game
{
    // Ulu ağaçlar ve ağaç evler (Project belgesi 52): ağacın mevsim görseli, üstündeki ağaç ev katmanı (yapım evresi,
    // harap, kar), ağaç evdeki karakterin gizlenmesi (içi görünmez) ve sesler. Görseller art/queue 492-495 ile gelir;
    // gelene kadar yer tutucu çizilir.
    public sealed partial class GameRoot
    {
        // Görsel ölçüleri (art/queue 492-493): 2x2 ağaç tuvali 192x256, pivot (96, 40); 3x3 çınar 256x320, pivot (128, 56).
        private const float GreatTreePivotX2 = 96f, GreatTreePivotY2 = 40f, GreatTreePivotX3 = 128f, GreatTreePivotY3 = 56f;

        private static string SeasonKey(Season s) => s switch
        {
            Season.Spring => "spring", Season.Summer => "summer", Season.Autumn => "autumn", _ => "winter"
        };

        private static string Species(ItemDef def) => def.Id.StartsWith("item.great_") ? def.Id.Substring("item.great_".Length) : "oak";

        private void AddGreatTree(ItemInstance item, int level)
        {
            bool big = item.Width >= 3;
            float px = big ? GreatTreePivotX3 : GreatTreePivotX2, py = big ? GreatTreePivotY3 : GreatTreePivotY2;
            var center = Iso.GridToWorld(item.Position.X + item.Width * 0.5f, item.Position.Y + item.Height * 0.5f, level);
            int order = Iso.SortOrder(center.y, 1, level);
            string species = Species(item.Def);
            var art = IsoArt.Get($"iso_greattree_{species}_{SeasonKey(_clock.Season)}_se", px, py)
                      ?? IsoArt.Get($"iso_greattree_{species}_summer_se", px, py);
            SpriteRenderer tree;
            if (art != null) tree = AddItemSprite(item, center, Color.white, 1f, order, art);
            else
            {
                // Yer tutucu: büyük, koyu yeşil ağaç (eski ağaç kaplamasıyla), mevsime göre renk.
                var tint = _clock.Season == Season.Autumn ? new Color(0.55f, 0.42f, 0.18f) : _clock.Season == Season.Winter ? new Color(0.45f, 0.42f, 0.38f) : new Color(0.24f, 0.42f, 0.2f);
                tree = AddItemSprite(item, center, tint, big ? 2.6f : 2.1f, order, SpriteMaterials.Tree);
                tree.color = tint;
            }
            var sway = SpriteMaterials.Sway;
            if (sway != null && art != null) tree.sharedMaterial = sway;
            AddShadowObject(center + new Vector3(0.35f, -0.1f, 0f), big ? 2.6f : 2.0f, level);

            var site = _life?.Trees?.SiteOf(item);
            if (site == null || !site.HasHouse) return;
            string key = TreehouseArtKey(site, GreatTrees.IsInn(item.Def));
            var over = IsoArt.Get(key, px, py);
            if (over != null) AddItemSprite(item, center, Color.white, 1f, order + 1, over);
            else
            {
                // Yer tutucu: tacın içinde kahverengi bir elmas (yapımda soluk, harapta gri).
                var c = !site.IsBuilt ? new Color(0.62f, 0.5f, 0.34f, 0.7f) : site.IsRuined ? new Color(0.45f, 0.43f, 0.4f) : new Color(0.5f, 0.33f, 0.18f);
                var sr = AddItemSprite(item, center + new Vector3(0f, big ? 2.2f : 1.7f, 0f), c, 0.5f + 0.08f * (int)site.Kind, order + 1, SpriteMaterials.Diamond);
                sr.color = c;
            }
            if (_clock.Season == Season.Winter && site.IsBuilt)
            {
                var snow = IsoArt.Get(big ? "iso_treehouse_snow_big_se" : "iso_treehouse_snow_se", px, py);
                if (snow != null) AddItemSprite(item, center, Color.white, 1f, order + 2, snow);
            }
        }

        /// <summary>Ağaç ev katmanının görsel adı: yapım evresi (1-3), harap, tür; kasaba çınarında aşevi.</summary>
        private static string TreehouseArtKey(TreehouseSite site, bool inn)
        {
            if (inn) return "iso_treehouse_inn_se";
            if (!site.IsBuilt)
            {
                int stage = site.Needed <= 0 ? 1 : 1 + Mathf.Clamp((int)(3 * site.Progress / site.Needed), 0, 2);
                return $"iso_treehouse_site_{stage}_se";
            }
            string kind = TreehouseSystem.KindKey(site.Kind);
            return site.IsRuined ? $"iso_treehouse_{kind}_ruin_se" : $"iso_treehouse_{kind}_se";
        }

        /// <summary>Karakter ağaç evin içinde (oyun, gözcülük, kulübede uyku, aşevi): çizilmez, sesi ve balonu kalır.</summary>
        private bool InsideTreehouse(Actor actor)
        {
            if (!actor.Runner.IsUsingItem) return false;
            var job = actor.Runner.Queue.Current;
            if (job == null || job.ItemId == 0) return false;
            var item = _world.GetItem(job.ItemId);
            if (item == null || !GreatTrees.IsGreatTree(item.Def)) return false;
            foreach (var i in item.Def.Interactions)
                if (i.Id == job.InteractionId) return TreehouseSystem.Hides(i);
            return false;
        }

        /// <summary>Ulu ağacın ek menüsü: kabuktaki adlar, çocuklara ağaç ev yasağı (ebeveyn kararı, belge 52 §6).</summary>
        private void AddTreehouseOptions(ItemInstance tree)
        {
            var site = _life.Trees.SiteOf(tree);
            if (site != null && site.CarvedNames.Count > 0)
            {
                string names = string.Join(", ", site.CarvedNames);
                _menuOptions.Add(new MenuOption { Label = GameLocalization.T("ui.treehouse.carved", ("names", names)), Run = () => ShowMessage(GameLocalization.T("ui.treehouse.carved", ("names", names)), 5f) });
            }
            var commander = Commander?.Character;
            if (commander == null || GreatTrees.IsInn(tree.Def)) return;
            // Hazine seçimi (Saz Damlı Kulübe): çocuk ne saklayacağını seçer.
            var stashI = FindInteraction(tree, "stash");
            if (site != null && site.IsUsable && site.Kind == TreehouseKind.Hut && site.Stash == null && stashI != null && TreehouseSystem.IsMinor(commander))
            {
                foreach (var id in _life.Trees.StashCandidates())
                {
                    string itemId = id, name = _content.Items.TryGet(id, out var d) ? d.DisplayName : id;
                    _menuOptions.Add(new MenuOption
                    {
                        Label = GameLocalization.T("ui.treehouse.stash_item", ("item", name)),
                        Blocked = Commander.Runner.WhyCannotStart(tree, stashI),
                        Run = () => { _life.Trees.ChooseStash(commander.Id, itemId); Commander.Runner.Command(Job.UseItem(tree.Id, stashI.Id)); }
                    });
                }
            }
            if (!TreehouseSystem.IsAdult(commander)) return;
            // Ebeveyn yasağı: çocuk başına.
            foreach (var m in _population.Members)
            {
                if (m.IsDead || m.IsVillager || !TreehouseSystem.IsMinor(m)) continue;
                var child = m;
                bool banned = _life.Trees.IsBanned(child);
                _menuOptions.Add(new MenuOption
                {
                    Label = GameLocalization.T(banned ? "ui.treehouse.unban_child" : "ui.treehouse.ban_child", ("name", child.Name)),
                    Run = () =>
                    {
                        _life.Trees.SetBanned(child, !banned);
                        ShowMessage(GameLocalization.T(banned ? "ui.treehouse.unbanned_child" : "ui.treehouse.banned_child", ("name", child.Name)));
                    }
                });
            }
        }

        private static Thatchwick.Core.Content.InteractionDef FindInteraction(ItemInstance item, string id)
        {
            foreach (var i in item.Def.Interactions) if (i.Id == id) return i;
            return null;
        }

        private long _feastWalkDay = -1;

        /// <summary>Mevsimin son akşamı: hanenin (oyuncunun yönettiği dışındaki) üyeleri çınarın altındaki sofraya yürür.</summary>
        private void GatherForFeast()
        {
            if (_life == null || !_life.Trees.FeastGatherNow || _feastWalkDay == _clock.DayIndex) return;
            _feastWalkDay = _clock.DayIndex;
            var tile = _life.Trees.PlaceTile("feast");
            if (!tile.HasValue) return;
            foreach (var a in _actors)
            {
                var c = a.Character;
                if (c.IsDead || c.IsVillager || c.Away || a == Commander || c.Stage == Thatchwick.Core.Characters.AgeStage.Baby || c.InBreakdown) continue;
                a.Runner.Command(Job.MoveTo(NearestWalkable(tile.Value, 6)));
            }
            ShowMessage(GameLocalization.T("ui.treehouse.feast_gather"), 4f);
        }

        private static void TreeSfx(string key) { var a = AudioService.Instance; if (a != null) a.Play(key); }

        /// <summary>Ağaç işi bitince ses: çekiç, ip merdiven, çocuk gülüşü, tabak, akçe.</summary>
        private static void TreeSound(InteractionDef i)
        {
            switch (i?.TreeAction)
            {
                case TreeActions.Found: case TreeActions.Build: case TreeActions.Repair: TreeSfx(SoundKeys.TreehouseBuild); break;
                case TreeActions.Play: case TreeActions.Siege: TreeSfx(SoundKeys.TreehouseKids); break;
                case TreeActions.Lookout: case TreeActions.Sleep: case TreeActions.Stash: case TreeActions.Dig: TreeSfx(SoundKeys.TreehouseLadder); break;
                case TreeActions.Dine: case TreeActions.Charity: TreeSfx(SoundKeys.InnDishes); break;
                case TreeActions.Donate: TreeSfx(SoundKeys.InnDonate); break;
            }
        }
    }
}
