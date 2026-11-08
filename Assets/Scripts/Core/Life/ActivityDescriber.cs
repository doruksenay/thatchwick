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
                case SkillType.Carpentry: return "craft";
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
                    if (!job.Reason.HasValue && inter.RestoresNeed) info.Need = inter.Need;
                    break;
                case JobKind.Social:
                    info.Icon = "social";
                    switch (job.Action)
                    {
                        case SocialAction.Feed: info.Icon = "care"; info.Text = "Bebeği besliyor"; info.Reason = "çünkü bebek aç"; break;
                        case SocialAction.Chat: info.Text = "Sohbet ediyor"; break;
                        case SocialAction.Propose: info.Icon = "love"; info.Text = "Evlenme teklif ediyor"; break;
                        default: info.Icon = "love"; info.Text = "Çocuk istiyor"; break;
                    }
                    break;
                default:
                    info.Icon = "idle"; info.Text = "?";
                    break;
            }
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
