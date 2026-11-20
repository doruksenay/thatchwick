using UnityEngine;
using Thatchwick.Core.Characters;
using Thatchwick.Core.Jobs;
using Thatchwick.Core.World;

namespace Thatchwick.Game
{
    /// <summary>
    /// Avlu sisteminin oyun katmanı (belge 57-57e): sinekler, azar balonu, basma tepkisi, kedinin gömmesi, arabalar paneli
    /// ve efekt yoğunluğu ayarı. Kurallar Core.Farmyard ve LifeSystems.Farmyard'da.
    /// </summary>
    public sealed partial class GameRoot
    {
        private const string FxPref = "fx_intensity";
        private CartPanel _cartPanel;
        /// <summary>Efekt yoğunluğu: 0 kapalı, 1 az, 2 tam (sinek, damla gibi ortam efektleri).</summary>
        private int _fxLevel = 2;
        private float FxScale => _fxLevel <= 0 ? 0f : _fxLevel == 1 ? 0.5f : 1f;

        private void InitYard()
        {
            _fxLevel = Mathf.Clamp(PlayerPrefs.GetInt(FxPref, 2), 0, 2);
            _life.AdultPack = () => Thatchwick.Core.Content.ContentPacks.Adult;   // küfürlü azar yalnızca +18 paketinde
            _life.Scolded += (witness, actor, key) =>
            {
                Say(witness, T(key, ("name", actor.Name)));
                // Gören yanına gelir: boşta ise yapanın yanındaki karoya yürür.
                var view = _actors.Find(a => a.Character == witness);
                if (view != null && view.Runner != null && view.Runner.IsIdle) view.Runner.Command(Job.MoveTo(NearestWalkable(actor.Position, 2)));
            };
            _life.SteppedInIt += (c, pile) =>
            {
                Say(c, T("yard.say.stepped"));
                var at = Iso.GridToWorld(pile.Position.X + 0.5f, pile.Position.Y + 0.5f, pile.Position.Z);
                _juice?.Emit(Juice.Kind.Soil, at, 3, Iso.SortOrder(at.y, 2, pile.Position.Z));
            };
            _life.CatBuried += cat =>
            {
                // Kedi eşeler, toprak savrulur (ayrı animasyon karesi gelene kadar; art/queue 506).
                var at = Iso.GridToWorld(cat.Position.X + 0.5f, cat.Position.Y + 0.5f, cat.Position.Z);
                _juice?.Emit(Juice.Kind.Soil, at, 5, Iso.SortOrder(at.y, 2, cat.Position.Z));
            };
            _cartPanel = new CartPanel(_life, _animals, _content)
            {
                Family = FamilyForCarts,
                Day = () => _clock.DayIndex
            };
            _cartPanel.Message += m => ShowMessage(m, 4f);
        }

        private System.Collections.Generic.IEnumerable<Character> FamilyForCarts()
        {
            foreach (var m in _life.Population.Members)
                if (!m.IsDead && !m.IsVillager && !m.Away) yield return m;
        }

        /// <summary>
        /// Yatmış tahıl (belge 57d §2): fırtınanın yatırdığı ekin yana devrik çizilir. Satır kaydıran shader (`_Lodge`) ve
        /// yatmış kareler (art/queue 515) gelene kadarki ara görünüm; hasada kadar öyle kalır.
        /// </summary>
        private void UpdateLodgedCrops()
        {
            if (_plants == null || _items == null) return;
            foreach (var kv in _plants.LodgedCrops)
            {
                if (!_items.Plants.TryGetValue(kv.Key, out var pv) || pv.View == null) continue;
                pv.View.localRotation = Quaternion.Euler(0f, 0f, -38f);
            }
        }

        private void CycleFx()
        {
            _fxLevel = (_fxLevel + 1) % 3;
            PlayerPrefs.SetInt(FxPref, _fxLevel);
        }

        /// <summary>Sinekler: sinekli her koku kaynağının üstünde, kademesine göre 2-6 sinek döner. Gece konarlar, yağmurda ve soğukta yoktur.</summary>
        private void UpdateYardFx(float dt)
        {
            UpdateLodgedCrops();
            if (_life == null || _juice == null || _fxLevel <= 0) return;
            var filth = _life.Filth;
            if (filth.Sources.Count == 0 || filth.FliesLanded) return;
            float scale = FxScale;
            foreach (var s in filth.Sources)
            {
                int flies = filth.FlyCount(s);
                if (flies <= 0) continue;
                // Bir sinek ortalama 1,9 sn yaşar: sayıyı korumak için saniyede flies / 1,9 sinek doğar.
                if (Random01() >= dt * flies * scale / 1.9f) continue;
                var at = Iso.GridToWorld(s.Position.X + 0.5f, s.Position.Y + 0.5f, s.Position.Z);
                _juice.Emit(Juice.Kind.Fly, at, 1, Iso.SortOrder(at.y, 4, s.Position.Z));
            }
        }
    }
}
