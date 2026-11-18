unity-import: pending

# 367 Ürün ikonları: maden hammaddeleri, işlenmiş ürünler, demetler: sonuç (durum: review)

Yan yana sayfası: `art/results/367-yanyana.png` (arayuz ailesinde kabul referansı yok, yalnızca yeni görseller; 23 ikonun hepsi dahil).

## Araç ve ayarlar
- `create_image_pixen`, 32x32, `no_background=true`, `outline: single color outline`, `detail: low detail`, her isteme "no face, no eyes". 316 ile aynı yöntem (ikon başına bir çağrı).
- 23 çağrı x 1 gen = **23 gen** (`cost_est` 23, `cost_max` 30).
- Hız sınırı: 10/10 eşzamanlı iş hatası 3 çağrıda alındı (slate_tile, torch, sheaf_wheat); harcama yapmadı, diğer işler bitince bir kez yeniden gönderildi (tarifin tek istisnası).
- `get_balance` sonrası: 4517,9 (paralel şeritlerin harcaması dahil).
- Renk indirgeme: ham çıktılar 19-64 renk; Pillow median-cut ile 16 renge (dither yok, alfa 0/255) indirildi, yeniden boyama yok. Ham hâller `art/tmp367/raw/` altında (commit edilmedi).
- **Çıktı yolu:** iş dosyası `Assets/Art/Icons/` yazıyor, ama işçi yalnızca `Assets/Art/Iso/` altına yazabilir. 316 gibi `Assets/Art/Iso/_aday/` altına konuldu; Unity tarafı taşıyabilir.
- Masaüstü kopyası: yerel.

## Dosyalar (`Assets/Art/Iso/_aday/`, hepsi 32x32 RGBA, pivot x 16, pivot y 0 alttan; ikon, pivot önemsiz)
Teslim edilen 17: icon_{charcoal, coal, iron_bloom, lime, mortar, ochre, ochre_paint, plaster, quicklime, roasted_ore, rubble, slate_stone, slate_tile, sheaf_wheat, sheaf_barley, sheaf_rye, sheaf_oats}.png

## Teslim edilmeyenler (6): `art/results/367-red/`
Görsel kontrolde (yan yana sayfası) reddedildi; tek deneme kuralı gereği yeniden üretilmedi:
- `icon_flint`: canavar yüzü çizilmiş.
- `icon_limestone`: kırmızı gözlü yüz.
- `icon_sand`: gözlüklü yüz, kum yığını okunmuyor.
- `icon_slag`: gözlü yeşil blob.
- `icon_torch`: kaidede yüz, meşale okunmuyor.
- `icon_gypsum`: şapka/kask gibi duruyor, alçıtaşı kristali okunmuyor.

## Doğrulama
| Ölçüt | Sonuç |
|---|---|
| Boyut | 32x32 (23/23) |
| Yarı saydam piksel | 0 (23/23) |
| Renk sayısı | ≤16 (indirgeme sonrası) |
| `style.py check --family arayuz` | KALDI yok; GEÇTİ: charcoal, flint, iron_bloom, mortar, ochre, plaster, roasted_ore, rubble, sand, sheaf_rye; UYARI (kilit kurulmadı / palete uzak): coal, gypsum, lime, limestone, ochre_paint, quicklime, sheaf_barley, sheaf_oats, sheaf_wheat, slag, slate_stone, slate_tile, torch |
| `validate_iso.py` | uygulanmadı (izometrik değil, UI ikonu) |
| Göz kontrolü | 17 teslim: tanınıyor; 6 ret yukarıda |

## Sorunlar / öneri
- Pixen "no face" isteğine rağmen taş/kum/cüruf/meşale ikonlarında yüz üretti (kawaii eğilimi, 316'daki sorunun aynısı). Reddedilen 6 ikon için yeniden iş önerilir: `create_image_pro` ya da `create_object_pro_flash` (daha büyük tuval, sonra 32'ye indirme) veya yalnızca nesne ("a rock", "pile of sand") ile faceless anahtar sözcükler.
- Teslim edilenlerin bazıları (lime, mortar, plaster, ochre_paint) kap/kepçe içeriyor; ürün ikonu olarak okunuyor, stil birliği orta.
