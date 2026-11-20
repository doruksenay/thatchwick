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
    // Ağacın ve ağaç evin eşya görünümü (AddGreatTree) ItemViews'tadır (performans incelemesi: eşya görünümleri GameRoot'tan ayrıldı).
    public sealed partial class GameRoot
    {
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
