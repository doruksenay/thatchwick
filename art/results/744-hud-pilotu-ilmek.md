unity-import: pending
# 744 HUD pilotu, yön A (ilmek): KISMİ teslim

Araç: `create_ui_asset` x2 (256x192; araç en az 192 px istiyor, tarifteki 256x128 ve 128x64 yapılamadı; palette_arayuz 128 renk, max_colors 32, seed 0) = 20 gen; `create_image_pixflux` 128x64 = 1 gen. Toplam 21 gen. get_balance sonrası: 756,6 (paralel şeritler de harcıyor).
Yan yana: art/results/744-yanyana.png. Stil denetimi: hepsi GEÇTİ (palete %100; sheet3 %92, ort. uzaklık 9.1).

## Teslim edilenler (Assets/Art/Iso/_aday/)
| Dosya | Boyut | Pivot x / y (alttan) | Not |
|---|---|---|---|
| ui_hud2_ilmek_sheet1/2/3.png | 256x192 / 256x192 / 128x64 | - | ham sayfalar |
| ui_hud2_ilmek_btn_build, _down, btn_book, _down | 32x32 | 16 / 0 | sayfa 2'den kesildi (zemin flood-fill ile silindi, içerik boyanmadı). Bordo ilmek halkası yok. btn_book'ta rozet bölgesi maskelendi (sağ üstte çentik) |
| ui_hud2_ilmek_badge.png | 14x14 | 7 / 0 | tarifteki 10x10 değil |

## Teslim EDİLMEYENLER
- sheet1: araç ayrı parça yerine tek büyük panel çizdi. clock, money, tracker, frame, bar, bar_fill_*: üretilmedi.
- sheet3: ikonlar tanınmaz. need_*, speed_*, season_* teslim edilmedi. 9-slice testi ve ikon x3 sayfası yapılmadı.
- Tek deneme kuralı gereği yeniden denenmedi.

## Sorular
`create_ui_asset` çok parçalı sayfa için `pieces` koordinatları ister; ikonlar için `create_image_pixen` (16/20/24 px, 1 gen) ve parça başına ayrı çağrı daha uygun. Karar kullanıcıda.
Masaüstü kopyası: yerel.
