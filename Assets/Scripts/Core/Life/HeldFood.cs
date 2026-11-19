using Thatchwick.Core.Characters;
using Thatchwick.Core.Content;
using Thatchwick.Core.Jobs;
using Thatchwick.Core.World;

namespace Thatchwick.Core.Life
{
    /// <summary>
    /// Yemek yerken elde görünen yiyecek: işin etkileşiminin harcadığı yiyecek (elma yiyorsa elma). Oyun katmanı bunun ikonunu
    /// yemek animasyonunda elin üstüne çizer. Yiyecek harcamayan iş (kuyudan su içmek gibi) ya da yiyecek olmayan malzeme null.
    /// </summary>
    public static class HeldFood
    {
        public static string Of(Job job, WorldState world, ContentDatabase content)
        {
            if (job == null || world == null || job.Kind != JobKind.UseItem || string.IsNullOrEmpty(job.InteractionId)) return null;
            var item = world.GetItem(job.ItemId);
            if (item == null) return null;
            foreach (var i in item.Def.Interactions)
            {
                if (i.Id != job.InteractionId) continue;
                if (i.Need != NeedType.Hunger) return null;
                // Meyve sepeti, ekmek sepeti gibi doldurulan dekor: içinden yenecek olan.
                if (i.EatContents) return content != null ? Build.DecorSystem.NextToEat(item, content) : null;
                string id = i.ConsumeItem;
                if (id == null && i.Consume.Count > 0) id = i.Consume[0].ItemId;
                if (id == null) return null;
                if (content != null && content.Items.TryGet(id, out var d) && !d.HasTag("food")) return null;
                return id;
            }
            return null;
        }
    }
}
