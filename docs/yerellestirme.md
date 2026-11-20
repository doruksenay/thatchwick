# Yerelleştirme

Karar (17.11.2026): oyun 5 dilde. **Kaynak dil İngilizce.** 1.0'da İngilizce ve Türkçe; İspanyolca, Fransızca ve Almanca sonra. Oyunun adı ve alt başlığı ("Thatchwick: Generations of a Village") hiçbir dilde çevrilmez. Tasarım notları: claude.ai Project, `21-dil-ve-yerellestirme.md`.

## Dosyalar
| Yer | Ne |
|---|---|
| `Assets/Scripts/Core/Localization/` | `Languages` (dil kodları, seçim), `LocTable` (tablo okuma), `Localizer` (metin, yer tutucu, çoğul, sayı biçimi), `LocText` (Core'dan anahtarlı metin), `ContentKeys` + `ContentLocalizer` (içerik adları) |
| `Assets/Scripts/Game/GameLocalization.cs` | Seçili dil (PlayerPrefs, yoksa cihaz dili), tabloları Resources'tan yükler. `GameLocalization.T("ui....")` |
| `Assets/Scripts/Game/LanguageMenu.cs` | Geçici dil düğmesi (sol alt). Ayarlar ekranı gelince oraya taşınır. |
| `Assets/Resources/Localization/content.<dil>.json` | Eşya, etkileşim, tarif, yapı parçası, beceri dalı, özellik, hedef, festival, köylü mesleği ve olayları, görünüm seçenekleri |
| `Assets/Resources/Localization/ui.<dil>.json` | Arayüz metinleri |
| `tools/loc/check.py` | Eksik İngilizce çeviriyi, tutmayan sayıları ve Türkçesi sonradan değişmiş (ESKİ) çevirileri listeler. `--todo` eksikleri TODO olarak ekler; `--kaynak` gözden geçirilen çevirileri kaydeder |
| `tools/loc/en_kaynak.json` | Her İngilizce çevirinin hangi Türkçe metinden yapıldığı (ESKİ denetimi için) |
| `tools/loc/flip_source.py` | Tek seferlik: içerik JSON'larını İngilizceye çevirir, Türkçeyi `content.tr.json`'a taşır |

Tablo biçimi düz JSON nesnesidir, anahtarlar sıralı, satır başına bir giriş: `{"item.egg.name": "Egg"}`.

## Anahtar şeması (içerik)
Önekli kimlikler olduğu gibi kullanılır, diğerlerine önek eklenir. Şema `ContentKeys.cs` ve `tools/loc/content_keys.py`'de birebir aynıdır.

| Metin | Anahtar |
|---|---|
| Eşya adı | `item.egg.name` |
| Etkileşim / ruh hali metni | `item.cooking_pot.act.eat_egg`, `...act.eat_egg.mood` |
| Tarif | `recipe.<id>.name` |
| Hayvan türü | `animal.cow.name` |
| Yapı parçası | `part.<id>.name` |
| Yemek etkisi | `effect.<id>.name`, `effect.<id>.desc` |
| Özellik, dal, proje, festival, hedef | `<id>.name`, `<id>.desc` (`trait.diligent.name`) |
| Alet ve kademesi | `tool.axe.name`, `tool.axe.tier.0` |
| Oda şablonu | `template.cottage.name` |
| Köylü mesleği ve olayı | `villager.orhan.role`, `villager.orhan.event.orhan_1` |
| Görünüm | `look.<liste>.<id>`: `look.hairStyles.braid`, `look.tattooPatterns.rune` (appearance.json'daki bütün seçenek listeleri; renk listeleri hex olduğu için çevrilmez) |

Köylü adları çevrilmez (dönem ve kültür kararı bekliyor, Project dokümanı 20).

## Geçiş dönemi
- İçerik JSON'larındaki metinler şimdilik **Türkçe** (`ContentLocalizer.JsonLanguage = "tr"`). Türkçe seçiliyken JSON olduğu gibi kullanılır; başka dilde önce o dilin tablosu, sonra İngilizce tablo.
- İçerik JSON'u Türkçe düzenlendiğinde: yeni metnin İngilizcesi `content.en.json`'a; mevcut bir metnin Türkçesi değiştiyse `check.py` onu ESKİ diye gösterir, İngilizcesini güncelle ve `python tools/loc/check.py --kaynak`.
- Kaynak dil geçişi (`flip_source.py --uygula`) kullanıcı onayıyla yapılacak: bütün içerik JSON metinleri İngilizceye döner, Türkçe `content.tr.json`'a taşınır; sonra `JsonLanguage` ve `check.py`'deki `JSON_LANGUAGE` "en" olur.
- Koddaki arayüz metinleri (~1.500, çoğu `GameRoot`, `BuildMode`, `ContentDatabase`, `MoodCatalog`) henüz anahtara taşınmadı; dosya dosya taşınacak. O zamana kadar yeni yazılan metinler anahtarla girer.

## Kurallar
- **Yer tutucular adlıdır:** `"{name} bought {item}"`. Kelime sırası dile göre değişir; `{0}` kullanılmaz. Süslü parantezin kendisi `{{` / `}}`.
- **Çoğul:** `key.one` / `key.other`, `Localizer.Plural(key, count)`. Türkçede sayıdan sonra isim tekil kalır; TR tablosunda yalnız `.other` yeterli.
- **Sayılar** ekranda `Localizer.Number` / `Percent` ya da yer tutucu ile (seçili dilin ondalık ayırıcısı). Kayıt dosyası kültürden bağımsız kalır.
- **Büyük/küçük harf:** ekranda `Localizer.ToUpper` (Türkçede i → İ); kimlik ve anahtar karşılaştırmasında `ToUpperInvariant` ve `StringComparison.Ordinal`.
- **Core metni çevirmez.** Oyuncuya gidecek metin `LocText.Of("key", ("arg", değer))` olarak taşınır; Game katmanı `Localizer.Resolve` ile çözer. Kayıtlara çevrilmiş metin yazılmaz, anahtar yazılır.
- **Metin uzunluğu:** Almanca ve Fransızca İngilizceden %20-35 uzun. Dar panellerde metin alanına pay bırak.
- **Font:** Pixelify Sans 5 dilin bütün harflerini içeriyor (19.11.2026 denetimi). `→` işareti fontta yok; metinde kullanma.
