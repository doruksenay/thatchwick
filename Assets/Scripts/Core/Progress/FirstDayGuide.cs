using System.Collections.Generic;

namespace Thatchwick.Core.Progress
{
    /// <summary>
    /// İlk gün rehberi (öğretici): yeni oyuncuya sırayla ne yapacağını gösteren kısa liste. Durum ayrı tutulmaz,
    /// hesabın "ilk kez" kayıtlarından ve sayaçlarından okunur (AccountProgress); böylece kayda yeni alan gerekmez.
    /// Bitince ya da oyuncu kapatınca "tutorial:closed" ilk kez kaydı düşülür ve rehber bir daha görünmez.
    /// </summary>
    public static class FirstDayGuide
    {
        public const string ClosedKey = "tutorial:closed";

        public sealed class Step
        {
            /// <summary>Metin anahtarı: ui.guide.&lt;Id&gt;</summary>
            public string Id;
            /// <summary>Bunlardan biri "ilk kez" yapıldıysa ya da sayacı sıfırdan büyükse adım tamamdır. Sonu * ise önek.</summary>
            public string[] Activities;
        }

        public static readonly IReadOnlyList<Step> Steps = new List<Step>
        {
            new Step { Id = "chop", Activities = new[] { "chop" } },
            new Step { Id = "eat", Activities = new[] { "eat*", "drink*", "feed" } },
            new Step { Id = "farm", Activities = new[] { "till", "sow", "water", "harvest" } },
            new Step { Id = "build", Activities = new[] { "furnish", "room", "plant", "template" } },
            new Step { Id = "fire", Activities = new[] { "fire", "light", "stoke" } },
            new Step { Id = "talk", Activities = new[] { "chat", "contract", "barter" } },
        };

        public static bool IsDone(Step step, AccountProgress account)
        {
            if (step == null || account == null) return false;
            foreach (var a in step.Activities)
            {
                if (!a.EndsWith("*", System.StringComparison.Ordinal))
                {
                    if (account.HasFirst("first:" + a) || account.Counter("act:" + a) > 0) return true;
                    continue;
                }
                string prefix = a.Substring(0, a.Length - 1);
                foreach (var f in account.Firsts) if (f.StartsWith("first:" + prefix, System.StringComparison.Ordinal)) return true;
                foreach (var kv in account.Counters) if (kv.Value > 0 && kv.Key.StartsWith("act:" + prefix, System.StringComparison.Ordinal)) return true;
            }
            return false;
        }

        public static int DoneCount(AccountProgress account)
        {
            int n = 0;
            foreach (var s in Steps) if (IsDone(s, account)) n++;
            return n;
        }

        /// <summary>Rehber gösterilsin mi: kapatılmadıysa ve bitmediyse.</summary>
        public static bool IsActive(AccountProgress account) =>
            account != null && !account.HasFirst(ClosedKey) && DoneCount(account) < Steps.Count;

        /// <summary>Hepsi bittiyse rehberi kapatır (bitirme ödülü: "ilk kez" XP'si). Kapattıysa true.</summary>
        public static bool CloseIfComplete(AccountProgress account) =>
            account != null && !account.HasFirst(ClosedKey) && DoneCount(account) >= Steps.Count && account.FirstTime(ClosedKey);

        /// <summary>Oyuncu rehberi kapattı.</summary>
        public static void Dismiss(AccountProgress account) { if (account != null && !account.HasFirst(ClosedKey)) account.FirstTime(ClosedKey); }
    }
}
