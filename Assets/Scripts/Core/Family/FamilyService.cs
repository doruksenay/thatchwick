using System;
using System.Collections.Generic;
using Thatchwick.Core.Build;
using Thatchwick.Core.Characters;
using Thatchwick.Core.Content;
using Thatchwick.Core.Jobs;
using Thatchwick.Core.Mood;

namespace Thatchwick.Core.Family
{
    /// <summary>
    /// Aile ve ilişki kuralları (Adım 16): bebek bakımı, sohbetle yakınlık, evlenme, çocuk istemek.
    /// Yakınlık 0-100; evlenme için 100 gerekir. Yakın akraba (ebeveyn-çocuk, kardeş) evlenemez.
    /// Haneye dışarıdan evlenen köylü katılır.
    /// </summary>
    public sealed class FamilyService : ISocialEffects
    {
        public const float BabyCareThreshold = 45f;
        public const float FeedRestore = 60f;
        public const int ChatAffection = 20;
        /// <summary>Sohbetin iki tarafa verdiği keyif.</summary>
        public const float ChatFun = 25f;
        public const int MarriageAffection = 100;
        public const double ConceiveChance = 0.6;

        private readonly Population _population;
        private readonly Household _house;
        private readonly ContentDatabase _content;
        private readonly Random _rng;
        private readonly Dictionary<long, int> _affection = new Dictionary<long, int>();

        /// <summary>(yapan, hedef, iş, mesaj): arayüzde gösterilir, hesap XP'sine yazılır.</summary>
        public event Action<Character, Character, SocialAction, string> Happened;
        /// <summary>Köylü evlenip haneye katıldığında.</summary>
        public event Action<Character> JoinedHousehold;

        public FamilyService(Population population, Household house, ContentDatabase content, int seed = 1)
        {
            _population = population ?? throw new ArgumentNullException(nameof(population));
            _house = house ?? throw new ArgumentNullException(nameof(house));
            _content = content ?? throw new ArgumentNullException(nameof(content));
            _rng = new Random(seed);
        }

        public Character Find(int id) => _population.Find(id);

        private static long Key(int a, int b) => a < b ? ((long)a << 32) | (uint)b : ((long)b << 32) | (uint)a;

        public int Affection(int a, int b) => _affection.TryGetValue(Key(a, b), out var v) ? v : 0;

        public void SetAffection(int a, int b, int value) => _affection[Key(a, b)] = Math.Max(0, Math.Min(100, value));

        public IEnumerable<(int a, int b, int value)> ExportAffection()
        {
            foreach (var kv in _affection) yield return ((int)(kv.Key >> 32), (int)(kv.Key & 0xffffffff), kv.Value);
        }

        public static bool IsAdultAge(Character c) => c.Stage == AgeStage.Youth || c.Stage == AgeStage.Adult;

        /// <summary>Ebeveyn-çocuk ya da en az bir ortak ebeveyni olan kardeşler.</summary>
        public static bool AreCloseRelatives(Character a, Character b)
        {
            if (a.BirthMotherId == b.Id || a.BirthFatherId == b.Id || b.BirthMotherId == a.Id || b.BirthFatherId == a.Id) return true;
            if (a.BirthMotherId.HasValue && a.BirthMotherId == b.BirthMotherId) return true;
            if (a.BirthFatherId.HasValue && a.BirthFatherId == b.BirthFatherId) return true;
            return false;
        }

        /// <summary>Hanedeki ilk yiyecek kaynağı (food etiketli, envanterde olan); yoksa null.</summary>
        public string FirstFood()
        {
            foreach (var kv in _house.InventoryList())
                if (kv.Value > 0 && _content.Items.TryGet(kv.Key, out var d) && d.HasTag("food")) return kv.Key;
            return null;
        }

        public string CanDo(Character actor, Character target, SocialAction action)
        {
            if (actor == null || target == null) return "Kişi bulunamadı.";
            if (actor.IsDead || target.IsDead) return "Ölüyle bu yapılamaz.";
            if (actor == target) return "Kendisiyle yapamaz.";
            if (actor.Stage == AgeStage.Baby) return "Bebek bunu yapamaz.";
            switch (action)
            {
                case SocialAction.Feed:
                    if (target.Stage != AgeStage.Baby) return "Yalnızca bebek beslenir; büyükler kendisi yer.";
                    if (actor.Stage == AgeStage.Child) return "Çocuk bebeği besleyemez.";
                    if (FirstFood() == null) return "Yiyecek yok.";
                    return null;
                case SocialAction.Chat:
                    if (target.Stage == AgeStage.Baby) return "Bebekle sohbet edilmez.";
                    return null;
                case SocialAction.Propose:
                    if (!IsAdultAge(actor) || !IsAdultAge(target)) return "Evlilik için ikisi de genç ya da yetişkin olmalı.";
                    if (actor.SpouseId.HasValue || target.SpouseId.HasValue) return "Zaten evli.";
                    if (actor.Sex == target.Sex) return "Bu çağda evlilik kadın ile erkek arasında.";
                    if (AreCloseRelatives(actor, target)) return "Yakın akrabalar evlenemez.";
                    if (Affection(actor.Id, target.Id) < MarriageAffection) return $"Yakınlık yetmiyor ({Affection(actor.Id, target.Id)}/{MarriageAffection}); önce sohbet edin.";
                    if (actor.IsVillager && target.IsVillager) return "Köylüler kendi aralarında evlenir.";
                    return null;
                case SocialAction.TryForBaby:
                    if (actor.SpouseId != target.Id) return "Yalnızca eşler çocuk isteyebilir.";
                    var woman = actor.Sex == Sex.Female ? actor : target;
                    var man = actor.Sex == Sex.Female ? target : actor;
                    if (woman.Sex != Sex.Female || man.Sex != Sex.Male) return "Çocuk için bir kadın ve bir erkek gerekir.";
                    if (woman.IsPregnant) return $"{woman.Name} zaten hamile.";
                    if (!woman.CanConceive) return $"{woman.Name} şu an hamile kalamaz.";
                    return null;
                default:
                    return "Bilinmeyen iş.";
            }
        }

        public double Duration(SocialAction action)
        {
            switch (action)
            {
                case SocialAction.Feed: return 0.05;
                case SocialAction.Chat: return 0.08;
                case SocialAction.Propose: return 0.05;
                default: return 0.1;
            }
        }

        public void Apply(Character actor, Character target, SocialAction action)
        {
            if (CanDo(actor, target, action) != null) return;   // bu arada koşul bozulduysa sessizce vazgeç
            string msg;
            switch (action)
            {
                case SocialAction.Feed:
                    var food = FirstFood();
                    _house.TryTakeItem(food);
                    target.Needs.Restore(NeedType.Hunger, FeedRestore);
                    msg = $"{actor.Name}, {target.Name} bebeği besledi";
                    break;
                case SocialAction.Chat:
                    // Kirli (hijyeni düşük) biriyle sohbet yakınlığı yarı artırır.
                    int gain = actor.Mood.Has(MoodCatalog.Dirty) || target.Mood.Has(MoodCatalog.Dirty) ? ChatAffection / 2 : ChatAffection;
                    int now = Math.Min(100, Affection(actor.Id, target.Id) + gain);
                    actor.Needs.Restore(NeedType.Fun, ChatFun);
                    target.Needs.Restore(NeedType.Fun, ChatFun);
                    actor.AddMoodlet(MoodCatalog.NiceChat);
                    target.AddMoodlet(MoodCatalog.NiceChat);
                    SetAffection(actor.Id, target.Id, now);
                    msg = now >= MarriageAffection && !actor.SpouseId.HasValue && !target.SpouseId.HasValue && IsAdultAge(actor) && IsAdultAge(target)
                        ? $"{actor.Name} ve {target.Name} çok yakınlaştı; evlenme teklif edilebilir"
                        : $"{actor.Name} ve {target.Name} sohbet etti (yakınlık {now})";
                    break;
                case SocialAction.Propose:
                    actor.SpouseId = target.Id;
                    target.SpouseId = actor.Id;
                    msg = $"{actor.Name} ve {target.Name} evlendi";
                    MoodEvents.OnMarriage(actor, target);
                    foreach (var c in new[] { actor, target })
                    {
                        if (!c.IsVillager) continue;
                        c.IsVillager = false;
                        c.CanStarve = true;
                        JoinedHousehold?.Invoke(c);
                        msg += $"; {c.Name} haneye katıldı";
                    }
                    break;
                case SocialAction.TryForBaby:
                    var woman = actor.Sex == Sex.Female ? actor : target;
                    var man = actor.Sex == Sex.Female ? target : actor;
                    bool ok = _rng.NextDouble() < ConceiveChance && woman.TryStartPregnancy(man.Id);
                    msg = ok ? $"{woman.Name} hamile!" : $"{actor.Name} ve {target.Name} bu sefer olmadı; yine denenebilir";
                    break;
                default:
                    return;
            }
            Happened?.Invoke(actor, target, action, msg);
        }

        public Character DependentNeedingCare(Character carer)
        {
            if (carer.IsVillager || carer.Stage == AgeStage.Baby || carer.Stage == AgeStage.Child) return null;
            Character neediest = null;
            foreach (var c in _population.Members)
            {
                if (c.IsDead || c.IsVillager || c.Stage != AgeStage.Baby) continue;
                float hunger = c.Needs.Get(NeedType.Hunger);
                if (hunger >= BabyCareThreshold) continue;
                if (neediest == null || hunger < neediest.Needs.Get(NeedType.Hunger)) neediest = c;
            }
            return neediest;
        }
    }
}
