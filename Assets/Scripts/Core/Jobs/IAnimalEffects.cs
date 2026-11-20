using Thatchwick.Core.Characters;
using Thatchwick.Core.World;

namespace Thatchwick.Core.Jobs
{
    /// <summary>Bir hayvanla yapılan iş (oyuncu ya da görevli aile üyesi).</summary>
    public enum AnimalAction
    {
        /// <summary>Tek hayvanı sev (dostluk).</summary>
        Pet = 0,
        /// <summary>Tımar et (binek): dostluk ve konfor.</summary>
        Brush = 1,
        /// <summary>Envanterdeki eyeri tak.</summary>
        Saddle = 2,
        /// <summary>Eyeri çıkar (envantere döner).</summary>
        Unsaddle = 3,
        /// <summary>Bin.</summary>
        Mount = 4,
        /// <summary>Eğit (Hayvancılık 5; köpekte Avcılık 8 "av köpeği"): hayvana komut, hayvan XP'si ve dostluk; günde bir.</summary>
        Train = 5,
        /// <summary>Ata numara öğret (Binicilik 7): at XP'si, dostluk, binicinin eğlencesi; günde bir.</summary>
        Trick = 6,
        /// <summary>Hasta hayvana bak (Hekimlik 4): iyileşme şansı seviyeyle artar.</summary>
        Treat = 7,
        /// <summary>Yarışa hazırla (Hayvancılık 9): ilk yarışmada puan +8.</summary>
        PrepContest = 8,
        /// <summary>İnsan gibi konuşmayı dene (Gönül kazanma 9; belge 55 §3): çok çok nadiren hayvan derdini anlatır.</summary>
        TalkLikeHuman = 9,
        /// <summary>Tavuğu kışkışla (belge 55, tavuk intikamı): aynı gün üçüncüsünde bütün kümes peşine düşer.</summary>
        Shoo = 10
    }

    /// <summary>Hayvan işlerinin kuralları ve etkileri (Core.Animals.AnimalCare uygular).</summary>
    public interface IAnimalEffects
    {
        /// <summary>Hayvanın şu anki karosu; yoksa (öldü, satıldı) null.</summary>
        GridPos? PositionOf(int animalId);
        /// <summary>Yapılabiliyorsa null, yoksa okunur neden.</summary>
        string CanDo(Character actor, int animalId, AnimalAction action);
        double Duration(AnimalAction action);
        void Apply(Character actor, int animalId, AnimalAction action);
    }

    /// <summary>Karoda yapılan iş (çapalama): oyuncunun ve görevlilerin tarlayı canlı modda hazırlaması.</summary>
    public enum TileAction
    {
        /// <summary>Çimi çapalayıp ekilebilir toprağa çevir.</summary>
        Till = 0,
        /// <summary>Yıpranmış yolu onar (çakıl ve kaldırım taş ister).</summary>
        Repair = 1,
        /// <summary>Dereye/gölete girip yıkan (ıslatır; kışın üşütür, yazın serinletir).</summary>
        Bathe = 2,
        /// <summary>Kıyıdan ya da buz deliğinden balık tut.</summary>
        Fish = 3,
        /// <summary>Donmuş suda buz kes.</summary>
        CutIce = 4,
        /// <summary>Gölete balık at (iş argümanı balık türü); aynı türden en az iki balık ürer.</summary>
        Stock = 5,
        /// <summary>Boş toprağa tohum serp (görevli tarlacı; mevsime ve nöbete uyan tohumu seçer).</summary>
        Sow = 6,
        /// <summary>Kazı (arsa kazısı, kat -2): karoyu kaz.</summary>
        Dig = 7,
        /// <summary>Kazıda ahşap destek kur (iş argümanı "stone": taş kemer).</summary>
        Support = 8,
        /// <summary>Kazıdan kovayla su boşalt.</summary>
        Bail = 9,
        /// <summary>Kazıya ışık koy (iş argümanı ışık eşyası).</summary>
        Light = 10,
        /// <summary>Su kenarında kavanoza su doldur (denizde deniz suyu, sazlıkta çamur da; belge 47 ek).</summary>
        JarFill = 11,
        /// <summary>Su kenarında cam kavanozla yakala (balık, karides, su bitkisi; oyuncuysa mini oyun).</summary>
        JarCatch = 12,
        /// <summary>Kedinin yere düşürdüğü süsü alıp yerine koy (belge 55, Kedi Alex).</summary>
        PutBack = 13
    }

    /// <summary>Karo işlerinin kuralları ve etkileri (Core.Work.FarmWork uygular).</summary>
    public interface ITileWork
    {
        /// <param name="arg">İşin ek bilgisi (gölete atılacak balık türü); çoğu işte null.</param>
        string CanDo(Character actor, GridPos tile, TileAction action, string arg = null);
        double Duration(Character actor, TileAction action);
        void Apply(Character actor, GridPos tile, TileAction action, string arg = null);
    }

    /// <summary>Süresi karoya bağlı karo işi (kazı: karonun sertliği, ışık, su).</summary>
    public interface ITileDuration
    {
        double Duration(Character actor, GridPos tile, TileAction action);
    }
}
