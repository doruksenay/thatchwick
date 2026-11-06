using System.Collections.Generic;

namespace Thatchwick.Core.Content
{
    /// <summary>Yerleştirilebilir/kullanılabilir eşya tanımı. İçerik sonradan paketlerle genişler.</summary>
    public sealed class ItemDef : IContentDef
    {
        public string Id { get; set; }
        public string DisplayName { get; set; }
        public List<InteractionDef> Interactions { get; set; } = new List<InteractionDef>();
    }
}
