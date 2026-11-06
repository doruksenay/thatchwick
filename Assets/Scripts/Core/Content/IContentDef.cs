namespace Thatchwick.Core.Content
{
    /// <summary>Veri odaklı içerik tanımı: kararlı, benzersiz bir kimliği vardır (ör. "item.bed_straw").</summary>
    public interface IContentDef
    {
        string Id { get; }
    }
}
