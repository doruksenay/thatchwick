using Thatchwick.Core.Content;

namespace Thatchwick.Core.World
{
    /// <summary>Haritaya yerleştirilmiş bir eşya (ItemDef'in örneği).</summary>
    public sealed class ItemInstance
    {
        public int Id { get; }
        public ItemDef Def { get; }
        public GridPos Position { get; }

        public ItemInstance(int id, ItemDef def, GridPos position)
        {
            Id = id;
            Def = def;
            Position = position;
        }
    }
}
