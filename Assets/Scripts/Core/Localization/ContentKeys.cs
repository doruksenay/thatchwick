namespace Thatchwick.Core.Localization
{
    /// <summary>
    /// İçerik (items.json, life.json, appearance.json) metinlerinin yerelleştirme anahtarları.
    /// Kimliği zaten önekli olanlar (item., trait., branch. ...) olduğu gibi kullanılır; öneksizlere önek eklenir.
    /// Şema tools/loc/content_keys.py ile birebir aynıdır; birini değiştirirsen ötekini de değiştir.
    /// </summary>
    public static class ContentKeys
    {
        public static string Name(string prefixedId) => prefixedId + ".name";
        public static string Desc(string prefixedId) => prefixedId + ".desc";

        public static string Interaction(string itemId, string interactionId) => itemId + ".act." + interactionId;
        public static string InteractionMood(string itemId, string interactionId) => Interaction(itemId, interactionId) + ".mood";

        public static string Recipe(string recipeId) => "recipe." + recipeId + ".name";
        public static string BuildPart(string partId) => "part." + partId + ".name";
        public static string EffectName(string effectId) => "effect." + effectId + ".name";
        public static string EffectDesc(string effectId) => "effect." + effectId + ".desc";

        public static string Tool(string toolId) => "tool." + toolId + ".name";
        public static string ToolTier(string toolId, int index) => "tool." + toolId + ".tier." + index;

        /// <summary>Köylünün meslek adı. Köylü adları çevrilmez (dönem ve kültür kararına bağlı, Project dokümanı 20).</summary>
        public static string VillagerRole(string villagerId) => villagerId + ".role";
        public static string VillagerEvent(string villagerId, string eventId) => villagerId + ".event." + eventId;

        /// <summary>Görünüm seçeneği: group = appearance.json'daki liste adı (hairStyles, beards, tattooPatterns ...).</summary>
        public static string Look(string group, string optionId) => "look." + group + "." + optionId;
    }
}
