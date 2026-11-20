namespace Thatchwick.Core.Localization
{
    /// <summary>
    /// Core'un oyuncuya göstereceği metni dile bağlamadan taşıması için: anahtar ve adlı argümanlar.
    /// Core metni çevirmez (dil bir sunum ayrıntısıdır ve kayıt dile bağlı kalmamalıdır); Game katmanı
    /// <see cref="Localizer.Resolve"/> ile ekranda çözer. Argüman olarak başka bir <see cref="LocText"/> verilebilir
    /// (ör. eşya adı: <c>LocText.Of("item.egg.name")</c>).
    ///
    /// Geçiş dönemi: henüz anahtara taşınmamış metinler <see cref="Plain"/> ile olduğu gibi taşınır.
    /// </summary>
    public readonly struct LocText
    {
        public string Key { get; }
        public (string name, object value)[] Args { get; }
        public long? Count { get; }

        /// <summary>Çevrilmeyen hazır metin (oyuncunun verdiği ad, geçiş dönemindeki eski metin).</summary>
        public string Literal { get; }

        private LocText(string key, (string name, object value)[] args, long? count, string literal)
        {
            Key = key;
            Args = args ?? new (string name, object value)[0];
            Count = count;
            Literal = literal;
        }

        public bool IsEmpty => Key == null && Literal == null;

        public static LocText Of(string key, params (string name, object value)[] args) => new LocText(key, args, null, null);

        /// <summary>Sayıya göre tekil/çoğul seçilen metin (bkz. <see cref="Localizer.Plural"/>).</summary>
        public static LocText Counted(string key, long count, params (string name, object value)[] args) => new LocText(key, args, count, null);

        public static LocText Plain(string text) => new LocText(null, null, null, text ?? "");

        public override string ToString() => Literal ?? Key ?? "";
    }
}
