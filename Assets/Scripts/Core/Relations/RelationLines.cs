using System;
using System.Collections.Generic;
using Thatchwick.Core.Localization;

namespace Thatchwick.Core.Relations
{
    /// <summary>
    /// Muhabbet ve ilişki replikleri (Project belgesi 67c §5): grup başına kaç satır olduğu. Metinler
    /// ui.relations_lines.&lt;dil&gt;.json içinde "rel.line.&lt;grup&gt;.&lt;n&gt;" anahtarlarıyla (araçla üretildi; sayılar dosyayla aynı).
    /// </summary>
    public static class RelationLines
    {
        private static readonly Dictionary<string, int> Counts = new Dictionary<string, int>(StringComparer.Ordinal)
        {
            ["topic.weather"] = 10,
            ["topic.harvest"] = 10,
            ["topic.animals"] = 10,
            ["topic.food"] = 10,
            ["topic.mine"] = 10,
            ["topic.craft"] = 10,
            ["topic.children"] = 10,
            ["topic.festival"] = 10,
            ["topic.trade"] = 10,
            ["topic.hunt"] = 10,
            ["topic.home"] = 10,
            ["topic.season"] = 10,
            ["joke"] = 30,
            ["story"] = 8,
            ["song"] = 6,
            ["react.laugh"] = 5,
            ["react.gasp"] = 5,
            ["react.clap"] = 3,
            ["react.nod"] = 4,
            ["react.listen"] = 3,
            ["gasp"] = 4,
            ["chat_leave"] = 3,
            ["debunk"] = 1,
            ["gossip.fight"] = 3,
            ["gossip.argument"] = 2,
            ["gossip.sulk"] = 2,
            ["gossip.flirt"] = 3,
            ["gossip.sweethearts"] = 3,
            ["gossip.engagement"] = 3,
            ["gossip.broke_engagement"] = 2,
            ["gossip.break_up"] = 2,
            ["gossip.wedding"] = 3,
            ["gossip.infidelity"] = 3,
            ["gossip.separation"] = 2,
            ["gossip.liar"] = 2,
            ["gossip.rumor"] = 4,
            ["gossip.lost_child"] = 2,
            ["gossip.filth"] = 2,
            ["gossip.animal_talk"] = 2,
            ["gossip.word_given"] = 2,
            ["gossip.donation"] = 1,
            ["gossip.generic"] = 3,
            ["argue"] = 6,
            ["argue_back"] = 5,
            ["jab"] = 8,
            ["sulk"] = 4,
            ["apology"] = 4,
            ["accept_apology"] = 4,
            ["refuse_apology"] = 4,
            ["bayram"] = 3,
            ["cane"] = 4,
            ["witness_fight"] = 5,
            ["witness_scuffle"] = 3,
            ["separate"] = 3,
            ["shy_no"] = 4,
            ["flirt_yes"] = 5,
            ["oops"] = 4,
            ["handkerchief_offer"] = 3,
            ["handkerchief_yes"] = 3,
            ["handkerchief_no"] = 3,
            ["breakup"] = 3,
            ["asking_greet"] = 3,
            ["asking_welcome"] = 3,
            ["coffee_serve"] = 2,
            ["salty_ok"] = 3,
            ["salty_bad"] = 3,
            ["coffee_sweet"] = 2,
            ["ask_hand"] = 2,
            ["ask_yes"] = 3,
            ["ask_think"] = 2,
            ["ask_no"] = 2,
            ["engage_welcome"] = 2,
            ["ribbon"] = 2,
            ["henna_song"] = 3,
            ["henna_tears"] = 2,
            ["procession"] = 3,
            ["door_tip"] = 3,
            ["jar_broke"] = 2,
            ["jar_whole"] = 2,
            ["toast"] = 5,
            ["anniversary"] = 3,
        };

        public static int Count(string group) => group != null && Counts.TryGetValue(group, out var n) ? n : 0;

        public static IEnumerable<string> Groups => Counts.Keys;

        /// <summary>Gruptan rastgele bir satır; grup yoksa genel dedikodu satırı.</summary>
        public static LocText Pick(string group, Random rng, params (string name, object value)[] args)
        {
            int n = Count(group);
            if (n == 0) { group = "gossip.generic"; n = Count(group); }
            return LocText.Of("rel.line." + group + "." + rng.Next(n), args);
        }
    }
}
