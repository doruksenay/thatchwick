using System;
using System.Collections.Generic;
using Thatchwick.Core.Aging;
using Thatchwick.Core.Characters;
using Thatchwick.Core.Clothing;
using Thatchwick.Core.Content;
using Thatchwick.Core.Mood;

namespace Thatchwick.Core.Life
{
    /// <summary>
    /// Yaşlılık sıkıntıları (belge 51 §3), Zihin dinçliği ve dalgınlık, kahve ve çayların etkileri (§3.3a), siyah çay.
    /// </summary>
    public sealed partial class LifeSystems
    {
        private AgingService _aging;
        public AgingService Aging => _aging ?? (_aging = CreateAging());
        /// <summary>Dalgın yaşlı işi bıraktı (oyun katmanı "Ne yapıyordum ben?" balonu çizer; ilerleme kaybolmaz).</summary>
        public event Action<Character> AbsentPause;
        /// <summary>Dalgınlık işareti balonu.</summary>
        public event Action<Character> AbsentSignal;
        private readonly Dictionary<int, long> _lateCoffee = new Dictionary<int, long>();
        private readonly Dictionary<int, double> _blackTeaAt = new Dictionary<int, double>();
        private readonly Dictionary<int, double> _dropAt = new Dictionary<int, double>();
        private readonly Random _agingRng = new Random(977);

        private AgingService CreateAging()
        {
            var a = new AgingService();
            a.Appeared += (c, cond) =>
            {
                string what = ConditionText(cond);
                Say($"{c.Name}: {what}.", NoticeLevel.Info, c.Id);
                c.AddChronicle($"{c.Name} yaşlandı: {what.ToLowerInvariant()}.");
            };
            a.Signal += c => { Say($"{c.Name} bazen sözcükleri arıyor.", NoticeLevel.Info, c.Id, transient: true); AbsentSignal?.Invoke(c); };
            return a;
        }

        /// <summary>Durum adı (bildirim için Türkçe; arayüzde aging.* anahtarları).</summary>
        public static string ConditionText(AgingCondition c)
        {
            switch (c)
            {
                case AgingCondition.Eyes: return "Gözlerim eskisi gibi değil";
                case AgingCondition.Knees: return "Dizlerim tutmuyor";
                case AgingCondition.Joints: return "Kemiklerim sızlıyor";
                case AgingCondition.Teeth: return "Dişlerim zayıfladı";
                case AgingCondition.Chill: return "Kanım çekildi";
                case AgingCondition.Sleep: return "Uykum kaçıyor";
                case AgingCondition.Hearing: return "Kulaklarım ağırlaştı";
                case AgingCondition.Tremor: return "Ellerim titriyor";
                case AgingCondition.Absent: return "Dalgınlık";
                default: return "";
            }
        }

        public static string ConditionKey(AgingCondition c) => "aging." + c.ToString().ToLowerInvariant();

        private bool ThickPiece(Character c)
        {
            foreach (var it in Clothing.Worn(c))
            {
                var m = Clothing.ModelOf(it);
                if (m != null && !it.Diamond && (m.Material == "fur" || m.Material == "felt" || m.Material == "knit" || m.Id == "wool_shawl")) return true;
            }
            return Aging.HasAid(c, AgingAids.IsKnee);
        }

        private void UpdateAging()
        {
            long day = Clock.DayIndex;
            bool coldOrRain = RainyToday || Weather.Today(Clock.TotalDays) == WeatherType.Snow;
            foreach (var c in Household())
            {
                if (c.IsDead) continue;
                if (c.ConditionSpeed == null)
                {
                    var ch = c;
                    c.ConditionSpeed = s => Aging.WorkSpeed(ch, s, coldOrRain || ch.AmbientTemperature < 5f, Clock.DayIndex);
                    c.ConditionXp = s => Aging.XpFactor(ch, s, Clock.DayIndex);
                }
                c.WalkFactor = Aging.WalkFactor(c, day);
                c.WarmthNeedFactor = (float)Aging.WarmthNeed(c, ThickPiece(c), false, day);
                // dalgınlık: günde bir kez, iş saatinde, uzun ve beklemeye dayanıklı bir işteyken
                if (Aging.Has(c, AgingCondition.Absent) && !c.Asleep && !c.Away)
                {
                    if (!_dropAt.TryGetValue(c.Id, out var at) || Math.Floor(at) != day) _dropAt[c.Id] = at = day + 0.35 + _agingRng.NextDouble() * 0.35;
                    if (Clock.TotalDays >= at && at > 0)
                    {
                        _dropAt[c.Id] = -day;   // bugün denendi
                        if (c.Duty != DutyKind.None && Aging.TryDrop(c, day, waitTolerant: true)) AbsentPause?.Invoke(c);
                    }
                }
            }
        }

        private void DailyAging()
        {
            long day = Clock.DayIndex;
            foreach (var c in Household())
            {
                if (c.IsDead) continue;
                bool lonely = c.MoodEnabled && c.Needs.Get(NeedType.Social) < MoodCatalog.NeedMoodThreshold;
                bool unhappy = c.MoodEnabled && (c.Mood.Status == MoodStatus.Unhappy || c.Mood.Status == MoodStatus.Miserable);
                bool grieving = c.MoodEnabled && c.Mood.Has(MoodCatalog.Grief);
                Aging.Daily(c, Clock.TotalDays, lonely, unhappy, grieving);
                // akşam kahvesi: ertesi gün "Kötü uyudu" (papatya o gece içildiyse yok)
                if (_lateCoffee.TryGetValue(c.Id, out var late) && late == day - 1 && !Aging.HadChamomile(c, day - 1)) c.AddMoodlet(AgingService.SleptBadly);
            }
        }

        private static bool Consumes(InteractionDef i, string itemId)
        {
            foreach (var con in i.AllConsumed()) if (con.ItemId == itemId) return true;
            return false;
        }

        /// <summary>İçecekler ve zihin etkinlikleri (belge 51 §3.3a), sert yiyecek.</summary>
        private void OnAgingInteraction(Character c, InteractionDef i)
        {
            if (c == null || i == null) return;
            long day = Clock.DayIndex;
            double frac = Clock.DayFraction;
            // kahve: günde bir fincan zihin puanı; akşam kahvesi ertesi gün uykusuzluk
            bool coffee = Consumes(i, "item.coffee");
            if (coffee)
            {
                Aging.AddMind(c, MindActivity.Coffee, day);
                if (frac >= CoffeeHabits.LateAfter) _lateCoffee[c.Id] = day;
            }
            if (Consumes(i, "item.tea_sage")) { c.AddMoodlet("eff_sage_clear"); Aging.AddMind(c, MindActivity.Sage, day); }
            if (Consumes(i, "item.tea_mint")) { c.AddMoodlet("eff_mint_fresh"); c.Needs.Restore(NeedType.Energy, 10f); }
            if (Consumes(i, "item.tea_chamomile") || Consumes(i, "item.honey_milk")) Aging.Chamomile(c, day);
            if (Consumes(i, "item.tea_linden") && frac >= 0.70) c.AddMoodlet("eff_calm_evening");
            if (Consumes(i, "item.black_tea"))
            {
                c.Needs.Restore(NeedType.Energy, 10f);
                // birlikte içilirse "Çay sohbeti" (yakın zamanda içen başka hane üyesi)
                foreach (var kv in _blackTeaAt)
                {
                    if (kv.Key == c.Id || Clock.TotalDays - kv.Value > 0.06) continue;
                    var other = Population.Find(kv.Key);
                    if (other == null || other.IsDead) continue;
                    c.AddMoodlet("eff_tea_talk"); other.AddMoodlet("eff_tea_talk");
                    c.Needs.Restore(NeedType.Social, 15f); other.Needs.Restore(NeedType.Social, 15f);
                    Aging.AddMind(c, MindActivity.Chat, day); Aging.AddMind(other, MindActivity.Chat, day);
                    break;
                }
                _blackTeaAt[c.Id] = Clock.TotalDays;
            }
            // sert yiyecek
            foreach (var con in i.AllConsumed())
                if (Aging.HardFoodPenalty(c, con.ItemId, day))
                {
                    c.AddMoodlet("eff_hard_bite");
                    if (i.Need == NeedType.Hunger && i.Restore > 0) c.Needs.Set(NeedType.Hunger, Math.Max(0f, c.Needs.Get(NeedType.Hunger) - i.Restore * (float)AgingService.HardFoodFullness));
                    break;
                }
            // zihin etkinlikleri: okuma, ders, müzik, sohbet, yürüyüş
            if (i.Skill == SkillType.Literacy) Aging.AddMind(c, MindActivity.Reading, day);
            else if (i.Skill == SkillType.Teaching || i.Skill == SkillType.Parenting) Aging.AddMind(c, MindActivity.Teaching, day);
            else if (i.Skill == SkillType.Music) Aging.AddMind(c, MindActivity.Music, day);
            else if (i.Skill == SkillType.Charm) Aging.AddMind(c, MindActivity.Chat, day);
            if (Consumes(i, AgingAids.Tincture)) Aging.Tincture(c, day);
        }

        /// <summary>Siyah çay ergen ve üstü (belge 51 §3.3a); kahve kuralı CoffeeHabits'te.</summary>
        private string DrinkBlocker(Character c, Thatchwick.Core.World.ItemInstance it, InteractionDef i)
        {
            if (c == null || i == null) return null;
            if ((c.Stage == AgeStage.Baby || c.Stage == AgeStage.Child) && Consumes(i, "item.black_tea")) return "Çocuk çay içmez.";
            return null;
        }

        // ---- aile etkileşimleri (§3.5) ----

        /// <summary>Koluna gir: çocuk ve üstü aile üyesi yaşlıyla yürür; yürüme kaybı yarıya, iki tarafa keyif.</summary>
        public bool ArmInArm(Character helper, Character elder)
        {
            if (helper == null || elder == null || helper.Stage == AgeStage.Baby || elder.Stage != AgeStage.Elder) return false;
            Aging.ArmInArm(elder, Clock.DayIndex);
            Aging.AddMind(elder, MindActivity.Walk, Clock.DayIndex);
            helper.AddMoodlet(MoodCatalog.NiceChat); elder.AddMoodlet(MoodCatalog.NiceChat);
            return true;
        }

        /// <summary>Lokmasını ufala (genç yetişkin ve üstü): o gün sert yiyecek normal.</summary>
        public bool CrumbleBite(Character helper, Character elder)
        {
            if (helper == null || elder == null || !AgeStages.IsAdultContentAge(helper.Stage)) return false;
            Aging.CrumbleBite(elder, Clock.DayIndex);
            return true;
        }

        /// <summary>Yüksek sesle konuş: o gün sohbette yanlış anlama yok.</summary>
        public bool SpeakUp(Character helper, Character elder)
        {
            if (helper == null || elder == null) return false;
            Aging.SpeakUp(elder, Clock.DayIndex);
            return true;
        }

        /// <summary>Hatırlat: o gün dalgın yaşlı işi bırakmaz.</summary>
        public bool Remind(Character helper, Character elder)
        {
            if (helper == null || elder == null || helper == elder) return false;
            Aging.Remind(elder, Clock.DayIndex);
            Aging.AddMind(elder, MindActivity.Chat, Clock.DayIndex);
            return true;
        }

        /// <summary>Çözüm eşyasını takar (envanterde varsa); torun yaptıysa "Nineme gözlük yaptım".</summary>
        public bool UseAid(Character elder, string itemId, Character maker = null)
        {
            if (elder == null || Array.IndexOf(AgingAids.All, itemId) < 0 || !House.TryTakeItem(itemId)) return false;
            Aging.EquipAid(elder, itemId);
            if (maker != null && maker != elder && (maker.BirthMotherId.HasValue || maker.BirthFatherId.HasValue))
            {
                maker.AddMoodlet("eff_gift_glasses");
                elder.AddChronicle($"{elder.Name} için {maker.Name} bir {Content.Items.Get(itemId)?.DisplayName?.ToLowerInvariant()} yaptı.");
            }
            return true;
        }
    }
}
