using System;
using Thatchwick.Core.Characters;
using Thatchwick.Core.Content;
using Thatchwick.Core.Localization;
using Thatchwick.Core.World;

namespace Thatchwick.Core.Life
{
    /// <summary>
    /// Taşıma çekirdeği (Project belgeleri 59 §4-§7, 58d §1.2 kararları; 69 kararı "ekle"): ev ve çiftlikte ortak depo; karakter
    /// çiftlik dışında (köy yolu, köy meydanı, orman, göl, sazlık, tepe, yamaçlar, kıyı) kendi işiyle aldığı eşyayı çantasına
    /// koyar, çiftliğe dönünce çanta depoya kendiliğinden boşalır. Sınır ağırlık: taşıma gücünün 1,5 katından sonra alamaz.
    /// Maden şimdilik dışarıda sayılmaz (maden ambarı ve ray M2'nin geri kalanıyla gelir); köylüler çanta taşımaz.
    /// </summary>
    public sealed partial class LifeSystems
    {
        /// <summary>Çantadan taşan (alınamayan) toplam adet; denge ve test için.</summary>
        public int CarryOverflowTotal { get; private set; }

        private void InitCarrying()
        {
            House.Divert = DivertToBag;
            // Kayıttan gelen çantalar 1 kg/adet ile girer; içerik tablosuyla yeniden tartılır.
            foreach (var m in Population.Members) if (!m.Bag.IsEmpty) m.Bag.Recompute(UnitKg);
        }

        public float UnitKg(string itemId) => Content.Items.TryGet(itemId ?? "", out var def) ? ItemMass.Of(def) : ItemMass.Default;

        /// <summary>Karakter dışarıda mı (çanta kullanılır): bölgeli dünyada çiftlik dışındaki bilinen bir bölgede.</summary>
        public bool IsAwayFromHome(Character c)
        {
            if (c == null || !Regions.IsRegional(World.Map)) return false;
            var r = Regions.At(c.Position);
            return r != null && r != Regions.Farm;
        }

        private bool DivertToBag(ItemKey key, int count)
        {
            var c = CarryContext.Current;
            if (c == null || c.IsDead || c.IsVillager || c.Stage == AgeStage.Baby || !IsAwayFromHome(c)) return false;
            if (!Content.Items.TryGet(key.Id ?? "", out var def) || def.Placement != ItemPlacement.Resource) return false;   // mobilya teslimle gelir
            float unit = ItemMass.Of(def);
            float hard = Strength.CapacityKg(Strength.Of(c)) * LoadRules.HardCap;
            int take = Math.Min(count, c.Bag.Room(unit, hard));
            if (take > 0) c.Bag.Add(key, take, unit);
            int lost = count - take;
            if (lost > 0)
            {
                CarryOverflowTotal += lost;
                Bubble?.Invoke(c, LocText.Of("ui.carry.full"));
            }
            return true;
        }

        /// <summary>Çiftliğe dönen (ya da bölgesiz dünyadaki) karakterin çantası depoya boşalır.</summary>
        private void DrainBagsAtHome()
        {
            foreach (var c in Population.Members)
            {
                if (c.Bag.IsEmpty || c.IsVillager || IsAwayFromHome(c)) continue;
                float kg = c.Bag.TotalKg;
                foreach (var kv in c.Bag.Drain()) House.AddStored(kv.Key, kv.Value);
                if (!c.IsDead) Bubble?.Invoke(c, LocText.Of("ui.carry.emptied", ("kg", Math.Round(kg, 1))));
            }
        }
    }
}
