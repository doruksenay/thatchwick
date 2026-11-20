using System;
using System.Collections.Generic;
using System.Globalization;
using Thatchwick.Core.Characters;

namespace Thatchwick.Core.Aging
{
    /// <summary>Yaşlılık sıkıntıları (belge 51 §3.3). Kalıcı durumdur, moodlet değildir; hiçbiri ölümcül değildir.</summary>
    public enum AgingCondition
    {
        None = 0,
        Eyes = 1,       // Gözlerim eskisi gibi değil
        Knees = 2,      // Dizlerim tutmuyor
        Joints = 3,     // Kemiklerim sızlıyor
        Teeth = 4,      // Dişlerim zayıfladı
        Chill = 5,      // Kanım çekildi
        Sleep = 6,      // Uykum kaçıyor
        Hearing = 7,    // Kulaklarım ağırlaştı
        Tremor = 8,     // Ellerim titriyor (yalnız 3. sıkıntı)
        Absent = 9      // Dalgınlık (yalnız 3. sıkıntı; Hafif ayarda yok)
    }

    /// <summary>Ayar (karar 7-3): varsayılan Normal, yeni oyunda "Gelişmiş" altında.</summary>
    public enum AgingSetting { Off = 0, Mild = 1, Normal = 2 }

    /// <summary>Zihin dinçliği basamağı (65. günden panelde görünür; puan gösterilmez).</summary>
    public enum MindLevel { Fresh = 0, Usual = 1, Tired = 2 }

    /// <summary>Zihin etkinliği türü (gün başına her biri bir kez puan verir).</summary>
    public enum MindActivity { Reading = 0, Teaching = 1, Game = 2, Music = 3, Chat = 4, Walk = 5, Potion = 6, Coffee = 7, Sage = 8 }

    /// <summary>Çözüm eşyaları (belge 51 §3.4).</summary>
    public static class AgingAids
    {
        public const string ReadingStone = "item.reading_stone";
        public const string GlassesBone = "item.glasses_bone", GlassesBrass = "item.glasses_brass", GlassesSilver = "item.glasses_silver";
        public const string Cane = "item.cane", CaneCarved = "item.cane_carved", CaneSilver = "item.cane_silver";
        public const string KneeWool = "item.knee_warmer_wool", KneeFur = "item.knee_warmer_fur";
        public const string Tincture = "item.warming_tincture";

        public static bool IsGlasses(string id) => id == GlassesBone || id == GlassesBrass || id == GlassesSilver;
        public static bool IsCane(string id) => id == Cane || id == CaneCarved || id == CaneSilver;
        public static bool IsKnee(string id) => id == KneeWool || id == KneeFur;
        public static readonly string[] All = { ReadingStone, GlassesBone, GlassesBrass, GlassesSilver, Cane, CaneCarved, CaneSilver, KneeWool, KneeFur };
    }

    /// <summary>Bir yaşlının sıkıntı durumu.</summary>
    public sealed class AgingState
    {
        public int CharacterId;
        public double ElderSince = -1;
        public readonly List<(AgingCondition cond, double day)> Pending = new List<(AgingCondition, double)>();
        public readonly HashSet<AgingCondition> Active = new HashSet<AgingCondition>();
        public bool ThirdRolled;
        /// <summary>Dalgınlık işaret dönemi bitişi (bu gün dahil; -1 yok).</summary>
        public double SignalUntil = -1;
        public double Mind;
        public int LowSocialDays, UnhappyDays;
        public readonly HashSet<string> Aids = new HashSet<string>(StringComparer.Ordinal);
        /// <summary>Gün bayrakları: "remind", "crumble", "speak", "arm", "care", "tincture", "mind:&lt;etkinlik&gt;", "drop", "chamomile".</summary>
        public long FlagDay = -1;
        public readonly HashSet<string> Flags = new HashSet<string>(StringComparer.Ordinal);
        public bool Has(AgingCondition c) => Active.Contains(c);
    }

    /// <summary>
    /// Yaşlılık sıkıntıları (belge 51 §3): yaşlılığa girişte gizli sıra (2 sıkıntı; 12. günde hâlâ yaşayana 3.), yaşam tarzı
    /// çarpanları, ayar (Kapalı / Hafif / Normal), çözüm eşyaları, aile etkileşimleri, Zihin dinçliği ve dalgınlık,
    /// kahve ve çay etkileri (§3.3a). Aynı işte iki sıkıntının cezası en çok −%40. Bunalım ve emir reddi kurallarına dokunmaz.
    /// </summary>
    public sealed class AgingService
    {
        public const double MaxPenalty = 0.40;
        public const double MindFromAgeDays = 55, MindShownFromDays = 65;
        public const double ThirdAfterDays = 12;
        public const double FreshAt = 6, TiredAt = -2;
        public const double SignalDays = 2, PreventChance = 0.25;
        public const double HardFoodFullness = 0.30;

        public AgingSetting Setting { get; set; } = AgingSetting.Normal;
        private readonly Dictionary<int, AgingState> _states = new Dictionary<int, AgingState>();
        private readonly Random _rng;

        /// <summary>Sıkıntı belirdi (bildirim ve baş üstü balon).</summary>
        public event Action<Character, AgingCondition> Appeared;
        /// <summary>Dalgınlık işareti ("Şu… neydi adı… kepçe!").</summary>
        public event Action<Character> Signal;

        public AgingService(int seed = 911) { _rng = new Random(seed); }

        public AgingState StateOf(Character c) => c != null && _states.TryGetValue(c.Id, out var s) ? s : null;
        private AgingState Get(Character c) { if (!_states.TryGetValue(c.Id, out var s)) _states[c.Id] = s = new AgingState { CharacterId = c.Id }; return s; }
        public bool Has(Character c, AgingCondition cond) => Setting != AgingSetting.Off && StateOf(c)?.Has(cond) == true;
        public IEnumerable<AgingCondition> Conditions(Character c)
        {
            var s = StateOf(c);
            if (s == null || Setting == AgingSetting.Off) yield break;
            foreach (var a in s.Active) yield return a;
        }

        // ---------------- olasılık (§3.3) ----------------

        /// <summary>Taban olasılık ve yaşam tarzı çarpanı.</summary>
        public static double Weight(AgingCondition c, Character ch)
        {
            switch (c)
            {
                case AgingCondition.Eyes:
                {
                    double w = 0.80;
                    int fine = Math.Max(ch.Skills.GetLevel(SkillType.Literacy), Math.Max(ch.Skills.GetLevel(SkillType.Tailoring), ch.Skills.GetLevel(SkillType.Smithing)));
                    return fine >= 5 ? w * 1.3 : w;
                }
                case AgingCondition.Knees:
                {
                    double w = 0.55;
                    if (ch.Skills.GetLevel(SkillType.Athletics) >= 5) w *= 0.5;
                    if (ch.Skills.GetLevel(SkillType.Mining) >= 5 || ch.Skills.GetLevel(SkillType.Woodcutting) >= 5) w *= 1.3;
                    return w;
                }
                case AgingCondition.Joints: return 0.45;
                case AgingCondition.Teeth: return 0.40;
                case AgingCondition.Chill: return 0.60;
                case AgingCondition.Sleep: return 0.40;
                case AgingCondition.Hearing: return 0.35;
                case AgingCondition.Tremor: return 0.25;
                case AgingCondition.Absent: return 0.20;
                default: return 0;
            }
        }

        private AgingCondition Pick(IEnumerable<AgingCondition> pool, Character c, Func<AgingCondition, double> extra = null)
        {
            var list = new List<(AgingCondition, double)>();
            double sum = 0;
            foreach (var p in pool)
            {
                double w = Weight(p, c) * (extra?.Invoke(p) ?? 1.0);
                if (w <= 0) continue;
                list.Add((p, w)); sum += w;
            }
            if (sum <= 0) return AgingCondition.None;
            double r = _rng.NextDouble() * sum;
            foreach (var (p, w) in list) { if (r < w) return p; r -= w; }
            return list[list.Count - 1].Item1;
        }

        private static readonly AgingCondition[] Common =
        {
            AgingCondition.Eyes, AgingCondition.Knees, AgingCondition.Joints, AgingCondition.Teeth,
            AgingCondition.Chill, AgingCondition.Sleep, AgingCondition.Hearing
        };

        /// <summary>Yaşlılığa giriş: gizli sıra (Normal 2, Hafif 1). İlki 2-8., ikincisi 6-14. günde.</summary>
        public void OnBecameElder(Character c, double today)
        {
            if (c == null || c.IsDead || Setting == AgingSetting.Off) return;
            var s = Get(c);
            if (s.ElderSince >= 0) return;
            s.ElderSince = today;
            int n = Setting == AgingSetting.Mild ? 1 : 2;
            var taken = new HashSet<AgingCondition>();
            for (int i = 0; i < n; i++)
            {
                var pick = Pick(Common, c, p => taken.Contains(p) ? 0 : 1);
                if (pick == AgingCondition.None) break;
                taken.Add(pick);
                double day = i == 0 ? 2 + _rng.Next(7) : 6 + _rng.Next(9);
                s.Pending.Add((pick, today + day));
            }
        }

        /// <summary>3. sıkıntı: yaşlılığın 12. gününde hâlâ yaşayana (Normal ayarda); Ellerim titriyor ve Dalgınlık bu çekilişte.</summary>
        private void RollThird(Character c, AgingState s, double today)
        {
            s.ThirdRolled = true;
            if (Setting != AgingSetting.Normal) return;
            var pool = new List<AgingCondition>();
            foreach (var x in Common) if (!s.Active.Contains(x) && !PendingHas(s, x)) pool.Add(x);
            pool.Add(AgingCondition.Tremor);
            pool.Add(AgingCondition.Absent);
            var mindFactor = MindChance(MindOf(c));
            var pick = Pick(pool, c, p => p == AgingCondition.Absent ? mindFactor : 1.0);
            if (pick == AgingCondition.None) return;
            double day = today + _rng.Next(9);   // 12-20. gün
            s.Pending.Add((pick, day));
        }

        private static bool PendingHas(AgingState s, AgingCondition c) { foreach (var p in s.Pending) if (p.cond == c) return true; return false; }

        /// <summary>Gün başı: bekleyen sıkıntılar belirir; dalgınlık önce 2 gün işaret verir; zihin puanı azalır.</summary>
        public void Daily(Character c, double today, bool lonely, bool unhappy, bool grieving)
        {
            if (c == null || c.IsDead) return;
            var s = StateOf(c);
            if (s == null) { if (c.AgeDays >= MindFromAgeDays) s = Get(c); else return; }
            // Zihin dinçliğini azaltanlar (yasın ilk 3 günü sayılmaz)
            s.LowSocialDays = lonely ? s.LowSocialDays + 1 : 0;
            s.UnhappyDays = unhappy && !grieving ? s.UnhappyDays + 1 : 0;
            if (s.LowSocialDays >= 3) s.Mind -= 1;
            if (s.UnhappyDays >= 3) s.Mind -= 1;
            if (Setting == AgingSetting.Off || s.ElderSince < 0) return;
            if (!s.ThirdRolled && today - s.ElderSince >= ThirdAfterDays) RollThird(c, s, today);
            for (int i = s.Pending.Count - 1; i >= 0; i--)
            {
                var (cond, day) = s.Pending[i];
                if (today < day) continue;
                if (cond == AgingCondition.Absent)
                {
                    if (s.SignalUntil < 0) { s.SignalUntil = today + SignalDays; Signal?.Invoke(c); continue; }
                    // işaret günlerinde zihin etkinliği yapıldıysa %25 hiç gelmez
                    if (today <= s.SignalUntil) { Signal?.Invoke(c); continue; }
                }
                s.Pending.RemoveAt(i);
                if (s.Active.Add(cond)) Appeared?.Invoke(c, cond);
            }
        }

        /// <summary>İşaret günlerinde bir zihin etkinliği yapıldı: %25 olasılıkla dalgınlık hiç gelmez.</summary>
        public bool TryPreventAbsent(Character c, double? roll = null)
        {
            var s = StateOf(c);
            if (s == null || s.SignalUntil < 0) return false;
            for (int i = 0; i < s.Pending.Count; i++)
            {
                if (s.Pending[i].cond != AgingCondition.Absent) continue;
                if ((roll ?? _rng.NextDouble()) >= PreventChance) return false;
                s.Pending.RemoveAt(i);
                s.SignalUntil = -1;
                return true;
            }
            return false;
        }

        // ---------------- gün bayrakları ----------------

        private bool Flag(Character c, string f, long day)
        {
            var s = StateOf(c);
            return s != null && s.FlagDay == day && s.Flags.Contains(f);
        }

        private void SetFlag(Character c, string f, long day)
        {
            var s = Get(c);
            if (s.FlagDay != day) { s.FlagDay = day; s.Flags.Clear(); }
            s.Flags.Add(f);
        }

        // ---------------- zihin dinçliği (§3.3a) ----------------

        /// <summary>Zihin etkinliği: 55. günden itibaren, etkinlik başına günde bir puan; yaşlılıkta ×2. İşaret dönemindeyse önleme zarı.</summary>
        public bool AddMind(Character c, MindActivity a, long day)
        {
            if (c == null || c.AgeDays < MindFromAgeDays) return false;
            string f = "mind:" + (int)a;
            if (Flag(c, f, day)) return false;
            SetFlag(c, f, day);
            var s = Get(c);
            s.Mind += c.Stage == AgeStage.Elder ? 2 : 1;
            if (s.SignalUntil >= 0 && day <= s.SignalUntil) TryPreventAbsent(c);
            return true;
        }

        public MindLevel MindOf(Character c)
        {
            double m = StateOf(c)?.Mind ?? 0;
            return m >= FreshAt ? MindLevel.Fresh : m <= TiredAt ? MindLevel.Tired : MindLevel.Usual;
        }

        /// <summary>Panelde "Zihin" satırı 65. günden itibaren görünür.</summary>
        public bool ShowMind(Character c) => c != null && c.AgeDays >= MindShownFromDays;

        public static double MindChance(MindLevel m) => m == MindLevel.Fresh ? 0.25 : m == MindLevel.Tired ? 1.5 : 1.0;
        public static double DropChance(MindLevel m) => m == MindLevel.Fresh ? 0.05 : m == MindLevel.Tired ? 0.20 : 0.15;

        // ---------------- dalgınlık: işi bırakma (§3.3a) ----------------

        public const string Awake = "eff_coffee", AwakeFine = "eff_coffee_fine", ClearMind = "eff_sage_clear", Mahmur = "eff_mahmur", Headache = "eff_headache", SleptBadly = "eff_slept_badly";

        /// <summary>
        /// Bugün işi bırakma olasılığı: dinçliğe göre %5/%15/%20; Uyanık ×0,5 ya da Berrak zihin −%30 (güçlüsü, toplanmaz);
        /// Mahmur ve Baş ağrısında ×1,5; Kötü uyudu +5 puan. Hatırlat denmişse ya da bugün bıraktıysa 0.
        /// </summary>
        public double AbsentDropChance(Character c, long day)
        {
            if (!Has(c, AgingCondition.Absent) || Flag(c, "remind", day) || Flag(c, "drop", day)) return 0;
            double p = DropChance(MindOf(c));
            bool awake = c.Mood.Has(Awake) || c.Mood.Has(AwakeFine), clear = c.Mood.Has(ClearMind);
            if (awake || clear) p *= Math.Min(awake ? 0.5 : 1.0, clear ? 0.7 : 1.0);
            if (c.Mood.Has(Mahmur) || c.Mood.Has(Headache)) p *= 1.5;
            if (c.Mood.Has(SleptBadly)) p += 0.05;
            return Math.Min(1.0, p);
        }

        /// <summary>Beklemeye dayanıklı uzun bir işte (pişirme ve kazan hariç) bugün durur mu; durursa günün tek bırakması.</summary>
        public bool TryDrop(Character c, long day, bool waitTolerant, double? roll = null)
        {
            if (!waitTolerant) return false;
            double p = AbsentDropChance(c, day);
            if (p <= 0 || (roll ?? _rng.NextDouble()) >= p) return false;
            SetFlag(c, "drop", day);
            return true;
        }

        // ---------------- aile etkileşimleri (§3.5) ----------------

        public void Remind(Character elder, long day) { if (elder != null) SetFlag(elder, "remind", day); }
        public void CrumbleBite(Character elder, long day) { if (elder != null) SetFlag(elder, "crumble", day); }
        public void SpeakUp(Character elder, long day) { if (elder != null) SetFlag(elder, "speak", day); }
        public void ArmInArm(Character elder, long day) { if (elder != null) SetFlag(elder, "arm", day); }
        /// <summary>Yaşlıya bak (Hekimlik 9): o gün bütün sıkıntı eksileri yarıya iner.</summary>
        public void Care(Character elder, long day) { if (elder != null) SetFlag(elder, "care", day); }
        /// <summary>Isıtan tentür: o gün sızıyı ve diz kaybını siler.</summary>
        public void Tincture(Character elder, long day) { if (elder != null) SetFlag(elder, "tincture", day); }
        /// <summary>Papatya çayı ya da ballı süt: o gece uyku normal, "Kötü uyudu" silinir.</summary>
        public void Chamomile(Character c, long day) { if (c != null) { SetFlag(c, "chamomile", day); c.Mood.Remove(SleptBadly); } }
        public bool HadChamomile(Character c, long day) => Flag(c, "chamomile", day);

        // ---------------- çözüm eşyaları ----------------

        public void EquipAid(Character c, string itemId) { if (c != null && itemId != null) Get(c).Aids.Add(itemId); }
        public void RemoveAid(Character c, string itemId) { StateOf(c)?.Aids.Remove(itemId); }
        public bool HasAid(Character c, Func<string, bool> pred)
        {
            var s = StateOf(c);
            if (s == null) return false;
            foreach (var a in s.Aids) if (pred(a)) return true;
            return false;
        }
        public bool Glasses(Character c) => !c.Asleep && HasAid(c, AgingAids.IsGlasses);   // uykuda gözlük çıkar
        public bool Cane(Character c) => HasAid(c, AgingAids.IsCane);

        // ---------------- eksiler (§3.3, tavan §3.3a sınırlar) ----------------

        /// <summary>Göz işi: okuma, yazma, dikiş, nakış, kuyumculuk, oyma.</summary>
        public static bool IsFineWork(SkillType s) =>
            s == SkillType.Literacy || s == SkillType.Tailoring || s == SkillType.Smithing || s == SkillType.Carpentry || s == SkillType.Weaving || s == SkillType.Pottery;

        /// <summary>El işi (sızıda −%15): zanaatler.</summary>
        public static bool IsHandWork(SkillType s) => SkillNames.IsCraft(s) || s == SkillType.Cooking || s == SkillType.Baking;

        private double Scale(Character c, double penalty, long day)
        {
            if (Setting == AgingSetting.Mild) penalty *= 0.5;
            if (Flag(c, "care", day)) penalty *= 0.5;
            return penalty;
        }

        /// <summary>İş hızı çarpanı (sıkıntılar toplamı en çok −%40).</summary>
        public double WorkSpeed(Character c, SkillType skill, bool coldOrRain, long day)
        {
            if (Setting == AgingSetting.Off || StateOf(c) == null) return 1.0;
            double pen = 0;
            if (Has(c, AgingCondition.Eyes) && IsFineWork(skill) && !Glasses(c))
                pen += skill == SkillType.Literacy && HasAid(c, a => a == AgingAids.ReadingStone) ? 0.10 : 0.30;
            if (Has(c, AgingCondition.Joints) && coldOrRain && IsHandWork(skill) && !Flag(c, "tincture", day)) pen += 0.15;
            pen = Math.Min(MaxPenalty, Scale(c, pen, day));
            return 1.0 - pen;
        }

        /// <summary>XP çarpanı: göz sıkıntısında Okuryazarlık −%50 (gözlük ya da okuma taşıyla yok), kulakta Gönül kazanma −%25.</summary>
        public double XpFactor(Character c, SkillType skill, long day)
        {
            if (Setting == AgingSetting.Off || StateOf(c) == null) return 1.0;
            double pen = 0;
            if (skill == SkillType.Literacy && Has(c, AgingCondition.Eyes) && !Glasses(c) && !HasAid(c, a => a == AgingAids.ReadingStone)) pen += 0.5;
            if (skill == SkillType.Charm && Has(c, AgingCondition.Hearing) && !Flag(c, "speak", day)) pen += 0.25;
            return 1.0 - Math.Min(0.5, Scale(c, pen, day));
        }

        /// <summary>Kalite eksi: göz (gözlüksüz göz işi) ve titreme (ince iş) −1; ustalık eseri titremede yok.</summary>
        public int QualityPenalty(Character c, SkillType skill)
        {
            if (Setting == AgingSetting.Off) return 0;
            int q = 0;
            if (Has(c, AgingCondition.Eyes) && IsFineWork(skill) && !Glasses(c)) q++;
            if (Has(c, AgingCondition.Tremor) && IsFineWork(skill)) q++;
            return Math.Min(1, q);
        }

        public bool CanMakeMasterwork(Character c, SkillType skill) => !(Has(c, AgingCondition.Tremor) && IsFineWork(skill));

        /// <summary>Yürüme çarpanı: diz −%30; baston −%10; Koluna gir yarıya; soğukta dizlik ve tentürsüz ek kayıp yok sayılır.</summary>
        public double WalkFactor(Character c, long day)
        {
            if (!Has(c, AgingCondition.Knees) || Flag(c, "tincture", day)) return 1.0;
            double pen = Cane(c) ? 0.10 : 0.30;
            if (Flag(c, "arm", day)) pen *= 0.5;
            return 1.0 - Scale(c, pen, day);
        }

        /// <summary>Merdiven: diz −%50 (baston −%20; korkuluk yarıya).</summary>
        public double StairFactor(Character c, bool railing, long day)
        {
            if (!Has(c, AgingCondition.Knees)) return 1.0;
            double pen = Cane(c) ? 0.20 : 0.50;
            if (railing) pen *= 0.5;
            return 1.0 - Scale(c, pen, day);
        }

        /// <summary>Isınma ihtiyacı çarpanı: "Kanım çekildi" ×1,3; kışlık takımda kalın parça ya da ocak başı sederde 1.</summary>
        public double WarmthNeed(Character c, bool thickPiece, bool atHearth, long day)
        {
            if (!Has(c, AgingCondition.Chill) || thickPiece || atHearth) return 1.0;
            return 1.0 + Scale(c, 0.3, day);
        }

        /// <summary>Sert yiyecek: keyif −3 ve doyum −%30 (Lokmasını ufala denmişse normal).</summary>
        public bool HardFoodPenalty(Character c, string foodId, long day) =>
            Has(c, AgingCondition.Teeth) && !Flag(c, "crumble", day) && HardFoods.Contains(foodId ?? "");

        public static readonly HashSet<string> HardFoods = new HashSet<string>(StringComparer.Ordinal)
        {
            "item.bread_rye", "item.pastirma", "item.nut", "item.chestnut", "item.smoked_meat", "item.hardtack", "item.dried_meat"
        };

        /// <summary>Sohbette yanlış anlama (%20): yakınlık 60+ ya da Yüksek sesle konuş denmişse yok.</summary>
        public bool Misheard(Character c, double affinity, long day, double? roll = null) =>
            Has(c, AgingCondition.Hearing) && affinity < 60 && !Flag(c, "speak", day) && (roll ?? _rng.NextDouble()) < 0.20;

        /// <summary>Uyku kalitesi: ×0,8 (papatya ya da ballı sütle normal).</summary>
        public double SleepQuality(Character c, long day) => Has(c, AgingCondition.Sleep) && !HadChamomile(c, day) ? 1.0 - Scale(c, 0.2, day) : 1.0;

        // ---------------- artılar ----------------

        /// <summary>"Dizim sızlıyor, yağmur geliyor": hanede yağış bir gün önceden bilinir.</summary>
        public bool ForetellsRain(IEnumerable<Character> members)
        {
            foreach (var m in members) if (!m.IsDead && Has(m, AgingCondition.Joints)) return true;
            return false;
        }

        /// <summary>Erkenci: 05-08 arası hafif işte ×1,25.</summary>
        public double EarlyRiser(Character c, double dayFraction, bool lightWork) =>
            Has(c, AgingCondition.Sleep) && lightWork && dayFraction >= 5.0 / 24 && dayFraction < 8.0 / 24 ? 1.25 : 1.0;

        /// <summary>Gürültüye bağışık (ağlayan bebek, meyhane sesi uykusunu bozmaz).</summary>
        public bool NoiseImmune(Character c) => Has(c, AgingCondition.Hearing);

        /// <summary>Ustanın gözü: eli titreyen usta çırağı yönetir; çırak o işte XP ×1,5.</summary>
        public double MasterEyeXp(Character master, Character apprentice, SkillType skill) =>
            master != null && apprentice != null && Has(master, AgingCondition.Tremor) && master.Skills.GetLevel(skill) >= 7 ? 1.5 : 1.0;

        // ---------------- kayıt ----------------

        public string[] Export()
        {
            var l = new List<string>();
            foreach (var s in _states.Values)
            {
                var act = new List<string>(); foreach (var a in s.Active) act.Add(((int)a).ToString(CultureInfo.InvariantCulture));
                var pen = new List<string>(); foreach (var p in s.Pending) pen.Add(((int)p.cond).ToString(CultureInfo.InvariantCulture) + "@" + p.day.ToString("R", CultureInfo.InvariantCulture));
                l.Add(string.Join("|", s.CharacterId.ToString(CultureInfo.InvariantCulture), s.ElderSince.ToString("R", CultureInfo.InvariantCulture),
                    string.Join(",", act), string.Join(",", pen), s.ThirdRolled ? "1" : "0", s.SignalUntil.ToString("R", CultureInfo.InvariantCulture),
                    s.Mind.ToString("R", CultureInfo.InvariantCulture), s.LowSocialDays.ToString(CultureInfo.InvariantCulture),
                    s.UnhappyDays.ToString(CultureInfo.InvariantCulture), string.Join(",", s.Aids)));
            }
            return l.ToArray();
        }

        public void Import(string[] data)
        {
            _states.Clear();
            if (data == null) return;
            foreach (var row in data)
            {
                var p = (row ?? "").Split('|');
                if (p.Length < 10 || !int.TryParse(p[0], NumberStyles.Integer, CultureInfo.InvariantCulture, out int id)) continue;
                var s = new AgingState { CharacterId = id };
                double.TryParse(p[1], NumberStyles.Float, CultureInfo.InvariantCulture, out s.ElderSince);
                foreach (var a in p[2].Split(',')) if (int.TryParse(a, out int ai) && Enum.IsDefined(typeof(AgingCondition), ai)) s.Active.Add((AgingCondition)ai);
                foreach (var e in p[3].Split(','))
                {
                    int at = e.IndexOf('@');
                    if (at > 0 && int.TryParse(e.Substring(0, at), out int ci) && double.TryParse(e.Substring(at + 1), NumberStyles.Float, CultureInfo.InvariantCulture, out double d))
                        s.Pending.Add(((AgingCondition)ci, d));
                }
                s.ThirdRolled = p[4] == "1";
                double.TryParse(p[5], NumberStyles.Float, CultureInfo.InvariantCulture, out s.SignalUntil);
                double.TryParse(p[6], NumberStyles.Float, CultureInfo.InvariantCulture, out s.Mind);
                int.TryParse(p[7], out s.LowSocialDays);
                int.TryParse(p[8], out s.UnhappyDays);
                foreach (var a in p[9].Split(',')) if (a.Length > 0) s.Aids.Add(a);
                _states[id] = s;
            }
        }
    }
}
