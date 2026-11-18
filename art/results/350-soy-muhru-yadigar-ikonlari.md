unity-import: pending

# 350 Soy Mührü ve yadigâr ikonları: sonuç (durum: failed, doğrulama)

Yan yana sayfası: `art/results/350-yanyana.png` (arayuz ailesinde kabul referansı yok; yalnızca yeni görseller). Ham çıktılar: `art/results/350-red/`. Hiçbir dosya `Assets/Art/Iso/` altına teslim edilmedi.

## Araç ve ayarlar
- `create_image_pixen`, `no_background=true`, `outline: single color outline` (parıltıda `lineless`), `detail: low detail` (64'lükte `medium detail`). Gerekçe: 315'te pixen küçük ikonlarda temiz çıktı, 16x16 yalnızca pixen'de mümkün. Tek tek çağrı, 6 çağrı x 1 gen = **6 gen**, hız sınırı yok, yeniden deneme yok. `get_balance` sonrası: 4707,9 (diğer şeritlerin harcaması dahil).
- Parıltı 12x12 istendi; pixen en az 16x16 verdiğinden 16x16 üretildi, içerik zaten 12x12 sınırlı kutuda olduğu için (2,2)-(14,14) maskeyle kırpıldı (yeniden boyama yok).
- Masaüstü kopyası: yerel.

## Doğrulama
| Dosya | Boyut | Renk (opak) | Sınır | Stil denetimi | Sonuç |
|---|---|---|---|---|---|
| ui_icon_legacy_seal | 16x16 | 33 | 8 | UYARI | BAŞARISIZ (renk) |
| ui_icon_legacy_heirloom | 16x16 | 42 | 8 | UYARI | BAŞARISIZ (renk) |
| ui_icon_legacy_descendants | 16x16 | 31 | 8 | UYARI | BAŞARISIZ (renk) |
| ui_icon_legacy_married_out | 16x16 | 39 | 8 | UYARI | BAŞARISIZ (renk) |
| ui_legacy_seal_large | 64x64 | 45 | 16 | UYARI | BAŞARISIZ (renk) |
| iso_fx_heirloom_glint | 12x12 | 26 | 8 | UYARI | BAŞARISIZ (renk) |

Yarı saydam piksel hepsinde 0. Stil denetiminde KALDI yok (kilit kurulmadı, yalnızca uyarı).

## Görsel değerlendirme (göz kontrolü, 8x büyütme)
- `seal` 16: elma/kırmızı top gibi okunuyor, saz çatı ve ağaç seçilmiyor; yalnızca bordo ton tutuyor.
- `heirloom` 16: koyu bir kütle; sandık ve altın yıldız seçilmiyor. Zayıf.
- `descendants` 16: dallı ağaç ve uçlarda noktalar kısmen okunuyor, kullanılabilir.
- `married_out` 16: iç içe iki halka okunuyor, iyi.
- `large` 64: saz çatılı ağaç, kökler ve kırmızı balmumu zemin okunuyor, iyi.
- `glint`: 4 kollu altın, ortası beyaz, iyi.

## Sorunlar ve öneri
1. Tek deneme kuralı ve renk sınırı (16'lıkta ≤8, 64'te ≤16) nedeniyle teslim edilmedi. Kullanıcı kararı: ham halleri renk indirgemesiyle (`reduce_colors`, çağrı başına 0,1 gen) teslim etmek ya da kuralı gevşetmek; özellikle `large`, `glint`, `married_out`, `descendants` iyi aday.
2. `seal` ve `heirloom` 16x16 yeniden üretim ister (4-6 gen; pixen 16'da şekil tutmadı, 32x32 ya da create_map_object denenebilir).
3. Araç eksiği: renk indirgeme için yerel bir `tools/art` aracı yok (bulutta tools/art yasak); öneri olarak yazıldı.
