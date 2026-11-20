unity-import: pending

# 593 Takı ruh hali ikonları (2): sonuç (durum: review)

Yan yana sayfası: `art/results/593-yanyana.png` (arayuz ailesinin kabul referansı yok; 0 referans + 2 yeni). Toplu önizleme x4: `art/results/593-ikonlar-x4.png` (parşömen), `art/results/593-ikonlar-x4-koyu.png` (#333333). Sıra: joy 16, fine 16, joy 32, fine 32.

## Araç ve ayarlar
- `create_image_pixen` 32x32, `no_background=true`, `outline: single color outline`, `detail: low detail`, 212/497 ile aynı; ikon başına 1 çağrı = **1 gen**, toplam **2 gen** (cost_est 2). Yön: varsayılan `parsomen`; stil eşlemesi yok. Hız sınırı hatası olmadı.
- 32x32 ham: alfa ikili, 12 renge (Pillow mediancut, dither yok). 16x16: en yakın komşu indirme, alfa ikili, 12 renk. PixelLab `reduce_colors` çağrılmadı.
- `get_balance` sonrası: 2118,7 (diğer şeritlerin harcaması dahil). Masaüstü kopyası: yerel.

## Dosyalar (`Assets/Art/Iso/_aday/`; ikon, pivot önemsiz: x 8, y 0 alttan; `_32` için x 16, y 0)
| Dosya | Boyut | Renk |
|---|---|---|
| ui_icon_moodlet_jewelry_joy.png | 16x16 | 10 |
| ui_icon_moodlet_jewelry_joy_32.png | 32x32 | 11 |
| ui_icon_moodlet_jewelry_fine.png | 16x16 | 11 |
| ui_icon_moodlet_jewelry_fine_32.png | 32x32 | 11 |

## Doğrulama
| Ölçüt | Sonuç |
|---|---|
| Yarı saydam piksel | 0 (alfa 0/255) |
| Renk ≤ 12 | geçti |
| `validate_iso.py` | ikonlara uygulanmaz; atlandı |
| Stil denetimi | KALDI yok; joy GEÇTİ (16 ve 32), fine UYARI (16: %75 palet dışı renkler; 32: %91 palete uzak; kilit kurulmadı) |
| Okunurluk (göz kontrolü) | fine: altın yüzük + kırmızı taş + kıvılcımlar net, 16x16'da da okunur. joy: yüzük okunur ama koyu kahve/bakır ağırlıklı, bakır rengi parlak değil ve 16x16'da beyaz kıvılcım neredeyse kayboluyor (32'de de küçük). Kayıp: iki ikonun ayrımı taş ve renkle yeterli |

## Sorunlar
- joy ikonunun bakırı koyu; kıvılcım zayıf. Kullanıcı beğenmezse yeniden üretim ister (tek deneme kuralı gereği tekrar denenmedi).
