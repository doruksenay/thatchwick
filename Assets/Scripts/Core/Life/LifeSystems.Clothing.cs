using System;
using System.Collections.Generic;
using Thatchwick.Core.Characters;
using Thatchwick.Core.Clothing;
using Thatchwick.Core.Content;
using Thatchwick.Core.World;

namespace Thatchwick.Core.Life
{
    /// <summary>
    /// Giyim bağlantıları (belge 48, 51): takım seçimi, ısı ve yağmur, kir ve eskime, dikilen giysinin örneğe dönmesi,
    /// "Hepsini yıka", giyim uyumu tepkileri, evre geçişinde küçülen giysiler ve ilk bayramlık, mühür yüzüğü.
    /// </summary>
    public sealed partial class LifeSystems
    {
        private ClothingService _clothing;
        /// <summary>Giyim sistemi; katalog ContentDatabase.Clothing'den (yoksa boş).</summary>
        public ClothingService Clothing => _clothing ?? (_clothing = CreateClothing());
        /// <summary>Köylü yorumu yapmak için en çok uzaklık (karo).</summary>
        public const int RemarkRange = 3;
        /// <summary>Yeni hane üyesine evden gelen sade giysiler verilir (oyunda açık; eski sınamalar için kapatılabilir).</summary>
        public bool StarterClothes { get; set; } = true;
        private readonly Random _remarkRng = new Random(733);

        private ClothingService CreateClothing()
        {
            var s = new ClothingService(Content.Clothing) { Moods = Content.Moods ?? Thatchwick.Core.Mood.MoodCatalog.Default };
            s.Remark += (villager, target, key) => ClothingRemark?.Invoke(villager, target, key);
            return s;
        }

        /// <summary>Köylü bir hane üyesinin giyimine yorum yaptı (konuşan, hedef, arayüz anahtarı); oyun katmanı balon çizer.</summary>
        public event Action<Character, Character, string> ClothingRemark;

        /// <summary>Hane kademesi (giyim uyumu): itibardan.</summary>
        public ClothingTier HouseholdClothingTier => ClothingService.HouseholdTier(Upbringing.Reputation.Points);

        private bool IsOutdoors(Character c)
        {
            var room = World.Map.Rooms.RoomAt(c.Position);
            return room == null || !room.IsIndoor;
        }

        private void UpdateClothing()
        {
            if (Content.Clothing == null || Content.Clothing.Count == 0) return;
            bool festival = Festivals.TodaysFestival() != null;
            bool rain = RainyToday;
            double frac = Clock.DayFraction;
            bool workHours = frac >= 0.30 && frac <= 0.75;
            var tier = HouseholdClothingTier;
            foreach (var c in Household())
            {
                if (c.IsDead || c.Away) continue;
                if (!Clothing.HasWardrobe(c) && StarterClothes) GiveStarterClothes(c);
                bool outdoors = IsOutdoors(c);
                var ctx = new ClothingContext
                {
                    Festival = festival && workHours, Working = workHours && c.Duty != DutyKind.None,
                    Raining = rain && outdoors, Outdoors = outdoors, Temperature = c.AmbientTemperature
                };
                Clothing.Dress(c, ctx);
                c.ClothingColdResist = Clothing.ColdResist(c);
                c.RainGuard = Clothing.RainGuard(c);
                Clothing.UpdateMoods(c, festival);
                if (!outdoors) Clothing.ChangeAfterRemark(c, tier);
            }
            // Giyim uyumu: köylü yanından geçerken (günde bir tepki).
            foreach (var v in Population.Members)
            {
                if (!v.IsVillager || v.IsDead) continue;
                foreach (var c in Household())
                {
                    if (c.IsDead || c.Away) continue;
                    if (Math.Abs(c.Position.X - v.Position.X) + Math.Abs(c.Position.Y - v.Position.Y) > RemarkRange) continue;
                    Clothing.TryReact(c, v, Villagers.BestAffection(v), tier, festival, Clock.TotalDays, _remarkRng.Next(6));
                }
            }
        }

        /// <summary>Evden gelen sade giysiler: Günlük takım (gövde, alt, ayak) ve Uyku takımı (iç gömlek); bebeğe kundak.</summary>
        public void GiveStarterClothes(Character c)
        {
            if (c == null || Clothing.HasWardrobe(c)) return;
            void Put(string model, OutfitKind kind)
            {
                var it = Clothing.Create(model, null, 0, null, Clock.TotalDays);
                if (it == null) return;
                it.EverWorn = true;
                if (Clothing.Equip(c, it, kind) != EquipError.None) Clothing.Remove(it.Id);
            }
            switch (c.Stage)
            {
                case AgeStage.Baby:
                    Put("swaddle", OutfitKind.Daily); Put("baby_bonnet", OutfitKind.Daily);
                    break;
                case AgeStage.Child:
                    Put("child_shirt", OutfitKind.Daily); Put(c.Sex == Sex.Female ? "child_skirt" : "child_drawers", OutfitKind.Daily); Put("carik", OutfitKind.Daily);
                    break;
                default:
                    if (c.Sex == Sex.Female) { Put("plain_entari", OutfitKind.Daily); Put("linen_kerchief", OutfitKind.Daily); }
                    else { Put("tunic", OutfitKind.Daily); Put("linen_drawers", OutfitKind.Daily); }
                    Put("carik", OutfitKind.Daily);
                    Put("linen_undershirt", OutfitKind.Sleep);
                    break;
            }
            Clothing.WardrobeOf(c).Get(OutfitKind.Daily);
        }

        /// <summary>Tarifle dikilen giysi envantere değil sandığa, dikenin adı ve tarihiyle girer (Kendi emeğimiz).</summary>
        private void OnClothingCrafted(Character c, InteractionDef i)
        {
            if (c == null || i == null || !i.IsCraft || Content.Clothing == null) return;
            foreach (var p in i.Produce)
            {
                if (!Content.Clothing.IsClothingItem(p.ItemId)) continue;
                for (int n = 0; n < p.Count; n++)
                {
                    if (!House.TryTakeItem(p.ItemId)) break;
                    var skill = i.Skill ?? SkillType.Tailoring;
                    int lvl = c.Skills.GetLevel(skill);
                    int q = lvl >= 9 ? 3 : lvl >= 7 ? 2 : lvl >= 4 ? 1 : 0;
                    var it = Clothing.FromItem(p.ItemId, c, Clock.TotalDays, q, ownLabor: true);
                    if (it != null && it.IsJewelry) it.JewelryStep = lvl >= 9 ? 2 : 1;
                }
            }
        }

        private void OnClothingWork(Character c, InteractionDef i)
        {
            if (c == null || i?.Skill == null) return;
            Clothing.AddDirt(c, i.Skill.Value);
        }

        /// <summary>"Hepsini yıka" (kirli sepeti): sabun varsa biri harcanır, kir tamamen gider; yoksa kül suyuyla.</summary>
        public int WashAllClothes(Character c)
        {
            bool soap = House.TryTakeItem("item.soap") || House.TryTakeItem("item.soap_olive") || House.TryTakeItem("item.bay_soap");
            int n = Clothing.WashAll(Household(), soap);
            Say(n > 0 ? $"{c?.Name ?? "Hane"} {n} giysi yıkadı{(soap ? " (sabunla)" : " (kül suyuyla)")}." : "Kirli sepeti boş.", NoticeLevel.Info, c?.Id ?? 0, transient: true);
            return n;
        }

        private void OnClothingStageChanged(Character c, AgeStage before, AgeStage after)
        {
            if (c == null || c.IsVillager || !Clothing.HasWardrobe(c)) return;
            var outgrown = Clothing.OnStageChanged(c);
            if (outgrown.Count > 0) Say($"{c.Name} {outgrown.Count} giysisinden büyüdü; sandıkta kardeşine saklanabilir.", NoticeLevel.Info, c.Id, transient: true);
            if (before == AgeStage.Child && after == AgeStage.Teen)
            {
                // İlk bayramlık hazırlanmışsa (sandıkta, Ergen'in giyebileceği bir bayram takımı) verilir.
                foreach (var it in Clothing.All)
                {
                    if (it.OwnerId != 0 && it.OwnerId != c.Id) continue;
                    var m = Clothing.ModelOf(it);
                    if (m == null || !m.Has("fest") || !ClothingAge.CanWear(m, AgeStage.Teen)) continue;
                    if (Clothing.GiveFirstFeast(c, it)) { Say($"{c.Name} ilk bayramlığını giydi: \"Büyüdüm!\"", NoticeLevel.Info, c.Id); break; }
                }
            }
            if (Clothing.Choose(c, default) == OutfitKind.Daily && !Clothing.WardrobeOf(c).Has(OutfitKind.Daily) && StarterClothes)
            {
                Clothing.WardrobeOf(c).Outfits.Clear();
                GiveStarterClothes(c);
            }
        }

        private void DailyClothing()
        {
            foreach (var c in Household()) if (!c.IsDead) Clothing.DailyWear(c);
        }
    }
}
