unity-import: pending

# 350b Soy Mührü ve yadigâr ikonları yeniden: sonuç (review)

Yan yana sayfası: `art/results/350b-yanyana.png` (arayuz ailesinde referans yok). 32x32 hamlar: `art/results/350b-red/`. Masaüstü kopyası: yerel.

## Araç ve ayarlar
- Renk azaltma: `reduce_colors` (8 renk; 64'lükte 16), 350'nin ham görselleri, 4 çağrı x 0,1. İlk üç çağrı ortak hız sınırına (10/10 iş) takıldı; bekleyip bir kez yeniden gönderildi.
- Mühür ve yadigâr: `create_image_pixen` 32x32, no_background, single color outline, low detail, 1 gen x 2; NEAREST ile 16x16'ya indirildi, `reduce_colors` 8 renk (2 x 0,1). İkinci deneme yapılmadı.
- Toplam 2,6 gen. Kalan (hesap, başlangıç get_balance 3559,5): ~3556,9.

## Dosyalar (`Assets/Art/Iso/_aday/`)
| Dosya | Boyut | Renk | Yarı saydam | Pivot (x, y alttan) | Stil denetimi |
|---|---|---|---|---|---|
| ui_icon_legacy_seal | 16x16 | 7 | 0 | 8, 8 | UYARI (palet) |
| ui_icon_legacy_heirloom | 16x16 | 8 | 0 | 8, 8 | UYARI (palet) |
| ui_icon_legacy_descendants | 16x16 | 7 | 0 | 8, 8 | UYARI (palet) |
| ui_icon_legacy_married_out | 16x16 | 7 | 0 | 8, 8 | UYARI (palet) |
| ui_legacy_seal_large | 64x64 | 16 | 0 | 32, 32 | UYARI (palet) |
| iso_fx_heirloom_glint | 12x12 | 8 | 0 | 6, 6 | UYARI (palet) |

Kilit kurulmadı, KALDI yok. İkonlarda zemin/2:1 doğrulaması uygulanmadı.

## Göz kontrolü
- seal 16: bordo yuvarlak mühür silueti okunuyor; çatı ve ağaç seçilmiyor.
- heirloom 16: sandık biçimi okunuyor; altın yıldız belirsiz.
- descendants, married_out, large, glint: şekil korundu.

## Sorunlar
Heirloom yıldızı ve seal içi 16'da zayıf; silüet okunduğu için ikinci deneme yapılmadı. Kullanıcı kararı: kabul ya da yeniden.
