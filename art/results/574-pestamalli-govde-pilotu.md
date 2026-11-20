unity-import: pending

# 574 Peştamallı gövde pilotu (erkek ve kadın, 8 yön)

Durum: review. 16/16 kare teslim edildi (`Assets/Art/Iso/_aday/`). Yan yana sayfa (2 referans + 16 yeni): [574-yanyana.png](574-yanyana.png). Giyinik durma karesiyle birebir karşılaştırma yok; sayfa aile referanslarıyla üretildi.

## Araç ve ayarlar
- `create_character_state` x2 (man_r84 `c63fe39d-b168-4732-bf11-b476172c98e8`, woman_r84 `135cb0b1-bd46-4514-8c87-0834b96ef80d`), state adı Pestamal. Gerekçe: giysi katmanı yolu (500/501) bu çalışmada kullanılamadı (500 `pending`, bekliyor); giysi gövdeyle birlikte tek karakter olarak üretildi (tarifin izin verdiği yedek yol). Yüz, oran ve kimlik kaynak karakterden korunur.
- Açıklama: tarifteki ek (peshtemal, modest, no nudity, bare shoulders and lower legs) + erkekte belden dize bordo çizgi, kadında koltuk altından dize çivit çizgi.
- Yeni karakter id: erkek `fc7d28c4-a86c-4565-88c1-cced1dd174b7`, kadın `445ff76c-f6ef-46c5-ab67-e072127bdc91`.
- Maliyet: araç 10-15 aralığı bildirdi (84 px sanat için ~15 x2 = en çok 30; `cost_est` 16, `cost_max` 32). `cost_est`'in neredeyse 2 katı ama `cost_max` içinde olduğu için çağrıldı. Paralel şeritler aynı anda harcadığından işin tam payı `get_balance` farkından ayrılamadı; üst sınır 30.
- Hız sınırı (10/10 iş): ilk iki çağrı reddedildi, 45 sn sonra bir kez yeniden denendi, geçti.
- Doygunluk: ham kareler "ailesinden çok doygun" yüzünden KALDI/UYARI (erkek x1.83-1.94, kadın x1.48-1.55). `style.py fit` ile erkek x0.60, kadın x0.77 doygunluğa indirildi, hepsi GEÇTİ. Ham hâller `art/574-red/`.
- Harcanan: ≤30. `get_balance` sonrası: 1329,7 (paralel şeritler dahil).

## Dosyalar (84x84, pivot x merkez, y alttan)
| Dosya | Opak bbox | Pivot x, y |
|---|---|---|
| iso_char_man_r84_pestamal_idle_s / se / e / ne / n / nw / w / sw | 28-48 x 76-78 | 42, 3-6 |
| iso_char_woman_r84_pestamal_idle_s / se / e / ne / n / nw / w / sw | 29-40 x 77-80 | 42, 2-5 |

Tam liste `Assets/Art/Iso/_aday/iso_char_{man,woman}_r84_pestamal_idle_{yön}.png` (16 dosya).

## Doğrulama
| Ölçüt | Sonuç |
|---|---|
| validate_iso (object) | 16/16 geçti |
| Stil denetimi (fit sonrası) | 16/16 GEÇTİ (%100 palete yakın; erkek x1.15, kadın x1.14-1.19) |
| Mahremiyet (görsel) | Göğüs, kalça ve kasık 8 yönde örtülü; erkekte belden dize, kadında koltuk altından dize |
| Gölge / ışık | Pişirilmiş gölge yok |
| Döşeme testi | Karakter için geçerli değil |

## Sorunlar
- Erkek peştamalında çizgiler zayıf: krem zemin ağırlıklı, bordo çizgi belirsiz. Kadında çivit çizgiler daha belirgin.
- Oran: kafa büyük (boy/kafa ~3,5-4); kaynak 012b karakterlerindeki sorun aynen sürüyor, bu işte değişmedi.
- Kadında üst kenar düz, omuz bandı yok.
- Masaüstü kopyası: yerel.
