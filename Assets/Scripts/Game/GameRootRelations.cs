using System.Collections.Generic;
using Thatchwick.Core.Characters;
using Thatchwick.Core.Jobs;
using Thatchwick.Core.World;
using UnityEngine;

namespace Thatchwick.Game
{
    /// <summary>
    /// İlişki sistemi oyun katmanı (Project belgeleri 67, 67a-d): kısa pozlar (muhabbet tepkileri, kavga sonu, gönül yolu
    /// sahneleri), kavganın toz bulutu (iş 739), hanenin evi ve meyhane gibi muhabbet yerleri, ilişki paneli (UI Toolkit).
    /// </summary>
    public sealed partial class GameRoot
    {
        private Ui.RelationsPanel _relationsPanel;

        /// <summary>Toz bulutu kareleri (iş 739): yoksa kavga normal pozlarla ve toz parçacığıyla oynar.</summary>
        private const string BrawlCloud = "iso_fx_brawl_cloud_0";
        private const int BrawlFrames = 8;

        private void InitRelations()
        {
            _life.PoseCue += (c, pose, secs) =>
            {
                var actor = _actors.Find(a => a.Character == c);
                if (actor == null || string.IsNullOrEmpty(pose)) return;
                actor.IdlePose = pose;
                actor.IdlePoseUntil = Time.unscaledTime + secs;
            };
            _life.FaceCue += (c, cell, until) =>
            {
                var actor = _actors.Find(a => a.Character == c);
                if (actor == null) return;
                actor.FaceCell = cell;
                actor.FaceUntil = until;
            };
            _life.ActingMember = () => Commander?.Character;   // satış ve bahsi yapan: seçili kişi
            _life.HouseholdHome = HouseholdHomeSpot;
            _life.ExtraGathering = NearInn;
            _relationsPanel = new Ui.RelationsPanel(_life, () => EnsureUi())
            {
                Household = HouseholdMembers,
                Selected = () => Commander?.Character,
                RunnerOf = c => _actors.Find(a => a.Character == c)?.Runner
            };
            _relationsPanel.Opening += () => { _familyPanel?.Close(); _minePanel?.Close(); };   // IMGUI paneli pencerenin üstüne çizilmesin
            _life.Relations.Happened += (a, b, action, text) => _relationsPanel?.Refresh();
        }

        private void ToggleRelations() => _relationsPanel?.Toggle();

        /// <summary>Hanenin toplanma yeri: oyuncu karakterinin evindeki ocağın önü; yoksa hane başının durduğu yer.</summary>
        private GridPos? HouseholdHomeSpot()
        {
            foreach (var it in _world.Items)
                if (it.Def != null && it.Def.HeatOutput > 0f && _world.Map.IsOwned(it.Position)) return NearestWalkable(it.Position, 3);
            var head = _life.HouseholdHead();
            return head?.Position;
        }

        /// <summary>Meyhane çevresi akşam muhabbet yeridir.</summary>
        private bool NearInn(GridPos pos)
        {
            double frac = _clock.DayFraction;
            if (frac < 0.6) return false;
            foreach (var kv in Places)
                if (kv.Key.StartsWith("work:inn", System.StringComparison.Ordinal) || kv.Key.StartsWith("work:tavern", System.StringComparison.Ordinal))
                    if (Mathf.Abs(kv.Value.X - pos.X) + Mathf.Abs(kv.Value.Y - pos.Y) <= 5) return true;
            return false;
        }

        /// <summary>Bu karakter toz bulutunda mı (kavga eden ya da hedefi): kavgayı başlatan ve hedefi döner.</summary>
        private (Actor brawler, Actor target) BrawlerFor(Actor actor)
        {
            foreach (var other in _actors)
            {
                var job = other.Runner.Queue.Current;
                if (job == null || job.Kind != JobKind.Social || job.Action != SocialAction.Fight) continue;
                var step = other.Runner.CurrentStep;
                if (step == null || step.Pose != "brawl") continue;
                if (other == actor || job.TargetCharacterId == actor.Character.Id)
                    return (other, _actors.Find(a => a.Character.Id == job.TargetCharacterId));
            }
            return (null, null);
        }

        /// <summary>
        /// Kavga karesi: kavgayı başlatan bulutu iki kavgacının tam ortasına çizer (pos ortaya kayar), hedef gizlenir. Bulut
        /// asset'i yoksa null döner ve iki karakter de kendi pozunda kalır; toz parçacığı yine ortadan atılır.
        /// </summary>
        private Sprite BrawlFrame(Actor actor, int seed, ref bool hide, ref Vector3 pos, int sortOrder)
        {
            var (brawler, target) = BrawlerFor(actor);
            if (brawler == null) return null;
            Vector3 mid = pos;
            if (target != null && target != brawler)
            {
                var a = brawler.Sim.At(1f); var b = target.Sim.At(1f);
                mid = new Vector3((a.x + b.x) * 0.5f, (a.y + b.y) * 0.5f, pos.z);
            }
            int frame = (int)(Mathf.Repeat(Time.time * 10f + seed, BrawlFrames));
            var cloud = IsoArt.Get(BrawlCloud + frame, 0.5f, 0.15f);
            if (cloud == null)
            {
                if (brawler == actor && _juice != null && Mathf.Repeat(Time.time * 7f + seed, 1f) < 0.15f) _juice.Emit(Juice.Kind.Dust, mid, 2, sortOrder + 1);
                return null;
            }
            if (brawler != actor) { hide = true; return null; }
            pos = mid;
            return cloud;
        }
    }
}
