using System.Collections.Generic;
using Thatchwick.Core.Characters;
using Thatchwick.Core.Content;
using Thatchwick.Core.Jobs;
using Thatchwick.Core.World;

namespace Thatchwick.Core.Life
{
    public sealed class ActivityInfo
    {
        /// <summary>Simge anahtarı (arayüz bir sprite'a eşler): sleep, eat, warm, social, fun, chop, farm, cook, craft, work, walk, care, love, idle, dead.</summary>
        public string Icon;
        public string Text;
        /// <summary>"çünkü acıktı" gibi; oyuncu komutunda "emir üzerine".</summary>
        public string Reason;
        public NeedType? Need;
        public bool Walking;
        /// <summary>Zincirin şu anki adımı (belge 60); arayüz metni "chain.step.&lt;StepId&gt;" anahtarından. Zincirsiz işte null.</summary>
        public string StepId;
        /// <summary>Adımın kare anahtarı; karesi yoksa görünüm Icon'a düşer.</summary>
        public string StepPose;
        public string Full => string.IsNullOrEmpty(Reason) ? Text : $"{Text} — {Reason}";
    }

    /// <summary>Bir işin ne olduğunu ve nedenini okunur metne çevirir (eylem balonu, kuyruk şeridi).</summary>
    public static class ActivityDescriber
    {
        public static string ReasonText(NeedType? reason)
        {
            switch (reason)
            {
                case NeedType.Hunger: return "çünkü acıktı";
                case NeedType.Energy: return "çünkü yorgun";
                case NeedType.Warmth: return "çünkü üşüyor";
                case NeedType.Social: return "çünkü yalnız";
                case NeedType.Fun: return "çünkü canı sıkıldı";
                case NeedType.Hygiene: return "çünkü kirlendi";
                case NeedType.Bladder: return "çünkü sıkıştı";
                default: return "emir üzerine";
            }
        }

        public static string NeedIcon(NeedType need)
        {
            switch (need)
            {
                case NeedType.Energy: return "sleep";
                case NeedType.Hunger: return "eat";
                case NeedType.Warmth: return "warm";
                case NeedType.Social: return "social";
                default: return "fun";
            }
        }

        private static string SkillIcon(SkillType? skill)
        {
            switch (skill)
            {
                case SkillType.Woodcutting: return "chop";
                case SkillType.Farming: return "farm";
                case SkillType.Cooking: return "cook";
                case SkillType.Carpentry:
                case SkillType.Leatherwork:
                case SkillType.Weaving: return "craft";
                case SkillType.AnimalHusbandry: return "farm";
                case SkillType.Herbalism: return "cook";
                default: return "work";
            }
        }

        public static ActivityInfo Describe(Character c, JobRunner runner, WorldState world)
        {
            if (c.IsDead) return new ActivityInfo { Icon = "dead", Text = "Öldü" };
            if (c.Stage == AgeStage.Baby) return new ActivityInfo { Icon = "idle", Text = "Bebek" };
            var job = runner?.Queue.Current;
            if (job == null)
            {
                if (c.IsPregnant) return new ActivityInfo { Icon = "care", Text = "Hamile, dinleniyor" };
                return new ActivityInfo { Icon = "idle", Text = "Boşta" };
            }
            var info = DescribeJob(job, world);
            info.Walking = job.Kind == JobKind.MoveTo || !runner.IsUsingItem;
            if (runner.IsFetchingWater) { info.Icon = "work"; info.Text = "Kovayla su taşıyor (" + info.Text + ")"; }
            var step = runner.CurrentStep;
            if (step != null) { info.StepId = step.Id; info.StepPose = step.Pose; }
            return info;
        }

        public static ActivityInfo DescribeJob(Job job, WorldState world)
        {
            var info = new ActivityInfo { Need = job.Reason, Reason = ReasonText(job.Reason) };
            switch (job.Kind)
            {
                case JobKind.MoveTo:
                    info.Icon = "walk"; info.Text = "Yürüyor"; info.Reason = job.Reason.HasValue ? info.Reason : null; info.Walking = true;
                    break;
                case JobKind.UseItem:
                    var item = world?.GetItem(job.ItemId);
                    InteractionDef inter = null;
                    if (item != null) foreach (var i in item.Def.Interactions) if (i.Id == job.InteractionId) inter = i;
                    if (item == null || inter == null) { info.Icon = "idle"; info.Text = "Bir işe hazırlanıyor"; break; }
                    info.Text = $"{inter.DisplayName} ({item.Def.DisplayName})";
                    info.Icon = inter.RestoresNeed ? NeedIcon(inter.Need) : SkillIcon(inter.Skill);
                    if (inter.Id == "water" || inter.Id == "water_pot") info.Icon = "water";   // 018c sulama
                    // Hasat ve meyve toplama kendi pozunu oynar (belge 59; kare gelene kadar oyun katmanı çapa pozuna düşer).
                    if (item.Def.IsCrop && inter.RequiresGrown && inter.RemovesItem && inter.Produce.Count > 0) info.Icon = "reap";
                    else if (item.Def.IsCrop && inter.RequiresWithered) info.Icon = "bend";   // sararmış ekini sökme
                    else if (inter.TakesFruit && item.IsPlant) info.Icon = "pick";
                    if (!job.Reason.HasValue && inter.RestoresNeed) info.Need = inter.Need;
                    break;
                case JobKind.Social:
                    info.Icon = "social";
                    switch (job.Action)
                    {
                        case SocialAction.Feed: info.Icon = "care"; info.Text = "Bebeği besliyor"; info.Reason = "çünkü bebek aç"; break;
                        case SocialAction.Chat: info.Text = "Sohbet ediyor"; break;
                        case SocialAction.TellStory: info.Text = "Masal anlatıyor"; break;
                        case SocialAction.TellAnimalTalk: info.Text = "Hayvanın konuştuğunu anlatıyor"; break;
                        case SocialAction.Propose: info.Icon = "love"; info.Text = "Evlenme teklif ediyor"; break;
                        case SocialAction.TryForBaby: info.Icon = "love"; info.Text = "Çocuk istiyor"; break;
                        default: info.Text = (Thatchwick.Core.Family.FamilyService.SkillSocial(job.Action)?.Label ?? "Sosyal iş") + " (beceri)"; break;
                    }
                    break;
                case JobKind.Animal:
                    info.Icon = "work";
                    switch (job.AnimalAction)
                    {
                        case AnimalAction.Brush: info.Text = "Atı tımar ediyor"; break;
                        case AnimalAction.Saddle: info.Text = "Eyerliyor"; break;
                        case AnimalAction.Unsaddle: info.Text = "Eyeri çıkarıyor"; break;
                        case AnimalAction.Mount: info.Icon = "walk"; info.Text = "Ata biniyor"; break;
                        case AnimalAction.Train: info.Text = "Hayvanı eğitiyor"; break;
                        case AnimalAction.Trick: info.Text = "Ata numara öğretiyor"; break;
                        case AnimalAction.Treat: info.Text = "Hasta hayvana bakıyor"; break;
                        case AnimalAction.PrepContest: info.Text = "Hayvanı yarışmaya hazırlıyor"; break;
                        case AnimalAction.TalkLikeHuman: info.Icon = "love"; info.Text = "Hayvanla insan gibi konuşmayı deniyor"; break;
                        default: info.Icon = "love"; info.Text = "Hayvanı seviyor"; break;
                    }
                    break;
                case JobKind.Tile:
                    info.Icon = "work";
                    switch (job.TileAction)
                    {
                        case TileAction.Repair: info.Text = "Yolu onarıyor"; break;
                        case TileAction.Bathe: info.Icon = "fun"; info.Text = "Suda yıkanıyor"; break;
                        case TileAction.Fish: info.Icon = "fish"; info.Text = "Kıyıda balık tutuyor"; break;
                        case TileAction.CutIce: info.Text = "Buz kesiyor"; break;
                        case TileAction.Dig: info.Icon = "mine"; info.Text = "Kazıyor"; break;
                        case TileAction.Support: info.Icon = "mine_support"; info.Text = "Destek kuruyor"; break;
                        case TileAction.Bail: info.Icon = "mine_bucket"; info.Text = "Kovayla su boşaltıyor"; break;
                        case TileAction.Light: info.Text = "Kandil yakıyor"; break;
                        case TileAction.Sow: info.Icon = "sow"; info.Text = "Ekiyor"; break;
                        case TileAction.Till: info.Icon = "farm"; info.Text = "Çapalıyor"; break;
                        case TileAction.PutBack: info.Icon = "pickup"; info.Text = "Düşen eşyayı yerine koyuyor"; break;
                        case TileAction.Stock: info.Text = "Gölete balık bırakıyor"; break;
                        case TileAction.Relieve: info.Text = "Sıkıştı: açıkta tuvaletini yapıyor"; break;
                        default: info.Text = "Çalışıyor"; break;
                    }
                    break;
                case JobKind.Breakdown:
                    info.Icon = "idle"; info.Text = string.IsNullOrEmpty(job.Note) ? "Bunalımda" : job.Note; info.Reason = "bunalımda";
                    break;
                default:
                    info.Icon = "idle"; info.Text = "?";
                    break;
            }
            if (job.IsDuty) info.Reason = "görevi olduğu için";
            else if (job.Kind != JobKind.Breakdown && !string.IsNullOrEmpty(job.Note)) info.Reason = job.Note;
            return info;
        }

        /// <summary>Kuyruk şeridi: şu anki iş ve sıradakiler.</summary>
        public static List<ActivityInfo> Queue(JobRunner runner, WorldState world)
        {
            var list = new List<ActivityInfo>();
            if (runner == null) return list;
            foreach (var j in runner.Queue.Jobs) list.Add(DescribeJob(j, world));
            return list;
        }
    }
}
