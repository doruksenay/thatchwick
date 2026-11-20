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
    public sealed partial class FamilyService : ISocialEffects
    {
        public const float BabyCareThreshold = 45f;
        /// <summary>Açlığı bu değerin üstündeki bebek tok sayılır: ikinci bir bakıcı aynı bebeğe boşuna yiyecek harcamaz.</summary>
        public const float BabyFullThreshold = 70f;
        /// <summary>Başka kattaki kişi sohbet için bu kadar karo daha uzak sayılır.</summary>
        public const int OtherLevelDistance = 12;
        public const float FeedRestore = 60f;
        public const int ChatAffection = 20;
        /// <summary>Sohbetin iki tarafa verdiği keyif.</summary>
        public const float ChatFun = 25f;
        /// <summary>İki çocuk ya da ergenin "sohbeti" kovalamacadır: ek keyif ve Atletizm XP.</summary>
        public const float ChaseFun = 10f;
        public const int ChaseXp = 6;

        public static bool IsPlaymates(Character a, Character b) =>
            a != null && b != null && (a.Stage == AgeStage.Child || a.Stage == AgeStage.Teen) && (b.Stage == AgeStage.Child || b.Stage == AgeStage.Teen);
        /// <summary>Masal: çocuğa keyif ve sosyal.</summary>
        public const float StoryFun = 40f, StorySocial = 30f;
        public const int MarriageAffection = 100;
        public const double ConceiveChance = 0.6;
        /// <summary>Sohbetin sohbet edenlerin Sosyal ihtiyacına verdiği doygunluk (Sosyal özelliğiyle çarpılır).</summary>
        public const float ChatSocialRestore = 45f;
        /// <summary>Otonom sohbet için aranan en uzak mesafe (karo).</summary>
        public const float FriendSearchRadius = 40f;

        private readonly Population _population;
        private readonly Household _house;
        private readonly ContentDatabase _content;
        private readonly Random _rng;
        private readonly Dictionary<long, int> _affection = new Dictionary<long, int>();

        /// <summary>(yapan, hedef, iş, mesaj): arayüzde gösterilir, hesap XP'sine yazılır.</summary>
        public event Action<Character, Character, SocialAction, string> Happened;
        /// <summary>Köylü evlenip haneye katıldığında.</summary>
        public event Action<Character> JoinedHousehold;

        /// <summary>Verilirse evlenme teklifi için bu da gerekir (köylü romantik ilişkisi); yoksa yalnızca Yakınlık.</summary>
        public Func<Character, Character, string> ExtraProposalCheck { get; set; }
        /// <summary>Sohbet sonrası eklenecek ek (kişiye özel) etki; (yapan, hedef).</summary>
        public event Action<Character, Character> Chatted;

        /// <summary>Hayvanla konuşma anıları (belge 55 §3.4); bağlanmamışsa "anlat" işi kapalıdır.</summary>
        public Thatchwick.Core.Life.AnimalTalkService AnimalTalk { get; set; }
        /// <summary>(anlatan, dinleyen, sonuç).</summary>
        public event Action<Character, Character, Thatchwick.Core.Life.TellResult> ToldAnimalTalk;

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

        /// <summary>Evlenecek yaşta: Genç yetişkin ya da Yetişkin (Ergen değil).</summary>
        public static bool IsAdultAge(Character c) => AgeStages.CanMarry(c.Stage);

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
                if (kv.Value > 0 && _content.Items.TryGet(kv.Key, out var d) && d.HasTag("food") && !d.HasTag("raw")) return kv.Key;
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
                    if (target.Needs.Get(NeedType.Hunger) >= BabyFullThreshold) return $"{target.Name} tok.";
                    if (FirstFood() == null) return "Yiyecek yok.";
                    return null;
                case SocialAction.Chat:
                    if (target.Stage == AgeStage.Baby) return "Bebekle sohbet edilmez.";
                    return null;
                case SocialAction.TellStory:
                    if (target.Stage != AgeStage.Child) return "Masal çocuklara anlatılır.";
                    if (actor.Stage == AgeStage.Child) return "Çocuk masal anlatamaz; dinler.";
                    return null;
                case SocialAction.TellAnimalTalk:
                    return AnimalTalk == null ? "Anlatacak böyle bir anısı yok." : AnimalTalk.CanTell(actor, target);
                case SocialAction.Propose:
                    if (!IsAdultAge(actor) || !IsAdultAge(target)) return "Evlilik için ikisi de genç yetişkin ya da yetişkin olmalı.";
                    if (actor.SpouseId.HasValue || target.SpouseId.HasValue) return "Zaten evli.";
                    if (actor.Sex == target.Sex) return "Bu çağda evlilik kadın ile erkek arasında.";
                    if (AreCloseRelatives(actor, target)) return "Yakın akrabalar evlenemez.";
                    if (Affection(actor.Id, target.Id) < MarriageAffection) return $"Yakınlık yetmiyor ({Affection(actor.Id, target.Id)}/{MarriageAffection}); önce sohbet edin.";
                    if (actor.IsVillager && target.IsVillager) return "Köylüler kendi aralarında evlenir.";
                    return ExtraProposalCheck?.Invoke(actor, target);
                case SocialAction.TryForBaby:
                    if (actor.SpouseId != target.Id) return "Yalnızca eşler çocuk isteyebilir.";
                    var woman = actor.Sex == Sex.Female ? actor : target;
                    var man = actor.Sex == Sex.Female ? target : actor;
                    if (woman.Sex != Sex.Female || man.Sex != Sex.Male) return "Çocuk için bir kadın ve bir erkek gerekir.";
                    if (woman.IsPregnant) return $"{woman.Name} zaten hamile.";
                    if (!woman.CanConceive) return $"{woman.Name} şu an hamile kalamaz.";
                    return null;
                default:
                    return CanDoSkill(actor, target, action);
            }
        }

        public double Duration(SocialAction action)
        {
            switch (action)
            {
                case SocialAction.Feed: return 0.05;
                case SocialAction.Chat: return 0.08;
                case SocialAction.Propose: return 0.05;
                case SocialAction.TellStory: return 0.08;
                case SocialAction.TellAnimalTalk: return 0.06;
                default: return SkillDuration(action);
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
                    // Ebeveynlik: seviye başına %5 daha doyurucu besleme.
                    target.Needs.Restore(NeedType.Hunger, (float)(FeedRestore * SkillNames.Factor(actor.Skills.GetLevel(SkillType.Parenting), 0.05)));
                    actor.GainSkillXp(SkillType.Parenting, 5);
                    msg = $"{actor.Name}, {target.Name} bebeği besledi";
                    break;
                case SocialAction.Chat:
                    // Kirli (hijyeni düşük) biriyle sohbet yakınlığı yarı artırır.
                    int gain = actor.Mood.Has(MoodCatalog.Dirty) || target.Mood.Has(MoodCatalog.Dirty) ? ChatAffection / 2 : ChatAffection;
                    // Gönül kazanma: seviye başına %5 daha çok yakınlık.
                    gain = (int)Math.Round(gain * SkillNames.Factor(actor.Skills.GetLevel(SkillType.Charm), 0.05));
                    // Belge 45 ortak basamak Sv 3: iki taraf da bir becerinin 3+ seviyesindeyse o konuda konuşurlar (+%50).
                    var topic = SharedTopic(actor, target);
                    if (topic.HasValue) { gain = (int)Math.Round(gain * TopicAffectionFactor); actor.GainSkillXp(topic.Value, TopicXp); target.GainSkillXp(topic.Value, TopicXp); }
                    actor.GainSkillXp(SkillType.Charm, 4);
                    target.GainSkillXp(SkillType.Charm, 2);
                    int now = Math.Min(100, Affection(actor.Id, target.Id) + gain);
                    actor.Needs.Restore(NeedType.Fun, ChatFun);
                    target.Needs.Restore(NeedType.Fun, ChatFun);
                    actor.AddMoodlet(MoodCatalog.NiceChat);
                    target.AddMoodlet(MoodCatalog.NiceChat);
                    SetAffection(actor.Id, target.Id, now);
                    foreach (var c in new[] { actor, target })
                    {
                        c.Needs.Restore(NeedType.Social, ChatSocialRestore * c.Modifiers.SocialGain);
                    }
                    Chatted?.Invoke(actor, target);
                    if (IsPlaymates(actor, target))
                    {
                        // İki çocuk/ergen sohbet yerine kovalamaca oynar: koşarlar, Atletizm büyür.
                        foreach (var c in new[] { actor, target })
                        {
                            c.GainSkillXp(SkillType.Athletics, ChaseXp);
                            c.Needs.Restore(NeedType.Fun, ChaseFun);
                        }
                        msg = $"{actor.Name} ve {target.Name} kovalamaca oynadı (yakınlık {now})";
                        break;
                    }
                    msg = now >= MarriageAffection && !actor.SpouseId.HasValue && !target.SpouseId.HasValue && IsAdultAge(actor) && IsAdultAge(target)
                        ? $"{actor.Name} ve {target.Name} çok yakınlaştı; evlenme teklif edilebilir"
                        : topic.HasValue
                            ? $"{actor.Name} ve {target.Name} {SkillNames.Name(topic.Value).ToLowerInvariant()} üzerine konuştu (yakınlık {now})"
                            : $"{actor.Name} ve {target.Name} sohbet etti (yakınlık {now})";
                    break;
                case SocialAction.TellStory:
                    // Sözlü masal: çocuğun keyfi ve sosyali dolar; anlatanın ebeveynlik ve gönül becerisi büyür.
                    target.Needs.Restore(NeedType.Fun, StoryFun);
                    target.Needs.Restore(NeedType.Social, StorySocial);
                    actor.Needs.Restore(NeedType.Social, StorySocial * 0.5f * actor.Modifiers.SocialGain);
                    actor.Needs.Restore(NeedType.Fun, ChatFun * 0.4f);
                    target.AddMoodlet(MoodCatalog.NiceChat);
                    actor.GainSkillXp(SkillType.Parenting, 4);
                    actor.GainSkillXp(SkillType.Charm, 2);
                    SetAffection(actor.Id, target.Id, Math.Min(100, Affection(actor.Id, target.Id) + ChatAffection / 2));
                    msg = $"{actor.Name}, {target.Name} için bir masal anlattı";
                    break;
                case SocialAction.TellAnimalTalk:
                {
                    // Sonuç satırı (inandı, "bizde de oldu", inanmadı) ToldAnimalTalk ile duyurulur; metin anahtarlıdır.
                    var told = AnimalTalk.Tell(actor, target);
                    if (told == null) return;
                    actor.GainSkillXp(SkillType.Charm, 3);
                    ToldAnimalTalk?.Invoke(actor, target, told);
                    return;
                }
                case SocialAction.Propose:
                    actor.SpouseId = target.Id;
                    target.SpouseId = actor.Id;
                    msg = $"{actor.Name} ve {target.Name} evlendi";
                    MoodEvents.OnMarriage(actor, target);
                    foreach (var c in new[] { actor, target })
                    {
                        if (!c.IsVillager) continue;
                        // Evlenerek gelen, eşinin neslinden sayılır (soy ağacında "1. nesil" görünmesin).
                        c.Generation = Math.Max(c.Generation, (c == actor ? target : actor).Generation);
                        c.IsVillager = false;
                        c.CanStarve = true;
                        JoinedHousehold?.Invoke(c);
                        msg += $"; {c.Name} haneye katıldı";
                    }
                    break;
                case SocialAction.TryForBaby:
                    var woman = actor.Sex == Sex.Female ? actor : target;
                    var man = actor.Sex == Sex.Female ? target : actor;
                    double chance = Math.Min(0.95, ConceiveChance * woman.Modifiers.Fertility);
                    bool ok = _rng.NextDouble() < chance && woman.TryStartPregnancy(man.Id);
                    msg = ok ? $"{woman.Name} hamile!" : $"{actor.Name} ve {target.Name} bu sefer olmadı; yine denenebilir";
                    break;
                default:
                    msg = ApplySkill(actor, target, action);
                    if (msg == null) return;
                    break;
            }
            Happened?.Invoke(actor, target, action, msg);
        }

        /// <summary>Sohbet edilecek kişi: yaşayan, bebek olmayan, en yakındaki (eşitlikte yakınlığı en yüksek olan).</summary>
        public Character FriendFor(Character character)
        {
            Character best = null;
            int bestDist = int.MaxValue;
            int bestAff = -1;
            foreach (var c in _population.Members)
            {
                if (c == character || c.IsDead || c.Stage == AgeStage.Baby) continue;
                int dist = Math.Abs(c.Position.X - character.Position.X) + Math.Abs(c.Position.Y - character.Position.Y)
                    + Math.Abs(c.Position.Z - character.Position.Z) * OtherLevelDistance;
                if (dist > FriendSearchRadius) continue;
                int aff = Affection(character.Id, c.Id);
                if (dist < bestDist || (dist == bestDist && aff > bestAff)) { best = c; bestDist = dist; bestAff = aff; }
            }
            return best;
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
