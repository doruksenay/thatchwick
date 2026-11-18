unity-import: pending

# 336 Çatı dokuları: sonuç (bulut işçisi, şerit 2, 2026-11-18)

Durum: **failed** (hiçbir karo teslim edilmedi). Oyun varlığı yok, Unity tarafının yapacağı bir şey yok.

Yan yana sayfası (yalnızca doğrulamayı geçen 2 sırt karosu + referanslar): [336-yanyana.png](336-yanyana.png)
Ham 16 karo (3x, 4x4; satırlar saz, şindıl, kiremit, arduvaz; sütun 0-1 eğim, 2-3 sırt): [336-ham-16-karo.png](336-ham-16-karo.png)
Döşeme testleri (8x8): [tile_ridge](336-doseme-tile.png), [slate_ridge](336-doseme-slate.png)

## Araç ve ayarlar
- `create_tiles_pro`, stil modu (002/333/334 ile aynı). `style_images` = `iso_ground_grass_a.png`, `iso_ground_dirt_a.png` (64x32, inline base64); `style_options` = renk paleti kapalı, outline/detail/shading açık (çatı tonları çimen paletinden çıkamaz).
- Tek çağrı, 8 numaralı açıklama (saz, şindıl, kiremit, arduvaz x eğim/sırt). Araç 16 karo döndürdü. Maliyet: **10 generation** (araç: 10-25, 64x32 için 10). `cost_est` 20, `cost_max` 25: sınır içinde.
- İlk çağrı parametre biçimi hatasıyla döndü (sunucuya ulaşmadı, harcama yok); düzeltilip bir kez gönderildi. Hız sınırı hatası yok.
- Tile id: `d9622e9a-01f2-4864-8e30-a7e709e1e1d5`. Masaüstü kopyası: yerel.
- `get_balance` sonrası kalan: **625,1** (paralel şeritler dahil, bu işin tek harcaması değil).

## Sonuç: neden teslim edilmedi
Ham karolar maske dışına taşıyor (31-109 opak piksel) ve maske içinde delik bırakıyor. Yalnızca maskeyle kırpıldı (içerik yeniden boyanmadı, yarı saydam pikseller opak yapıldı).

| Karo | Anlam | Maske içi boş piksel | validate_iso |
|---|---|---|---|
| 0 | saz eğim | 1 | BAŞARISIZ |
| 1 | saz eğim | 35 | BAŞARISIZ |
| 2 | saz sırt | 1 | BAŞARISIZ |
| 3 | saz sırt | 2 | BAŞARISIZ |
| 4, 5 | şindıl eğim | 48, 49 | BAŞARISIZ |
| 6, 7 | şindıl sırt | 27, 37 | BAŞARISIZ |
| 8, 9 | kiremit eğim | 41, 39 | BAŞARISIZ |
| 10 | kiremit sırt | 43 | BAŞARISIZ |
| **11** | kiremit sırt | 0 | geçti |
| 12, 13 | arduvaz eğim | 18, 5 | BAŞARISIZ |
| **14** | arduvaz sırt | 0 | geçti |
| **15** | arduvaz sırt | 0 | geçti |

Geçen karoların stil denetimi (`style.py check --family yapi`):

| Dosya | Palete yakın | Ort. uzaklık | Doygunluk | Sonuç |
|---|---|---|---|---|
| tile_11 (kiremit sırt) | %70 | 14.1 | x1.69 | KALDI |
| tile_14 (arduvaz sırt) | %98 | 16.2 | x0.38 | KALDI |

Sonuç: 8 çıktının hiçbiri hem doğrulamayı hem stil denetimini geçmedi. Dolayısıyla `Assets/Art/Iso/` altına dosya konmadı. Tek deneme kuralı gereği yeniden üretim ve boşluk doldurma yapılmadı.

Döşeme testi (yalnızca geçen iki sırt karosu; sırt hattı kesintisizliği ölçülemedi çünkü eğim karoları yok): dikiş parlaklık oranı kiremit sırt 0.90, arduvaz sırt 0.94.

## Gözlemler (kullanıcı kararı için)
- Saz, şindıl ve arduvaz eğim karoları düz doku değil, **piramit (kırma çatı) şeklinde** geldi: tepe noktası ve dört yüz var. Döşenince her karo ayrı bir tepe gibi görünür; motorun "düz doku + eğim gölgesi" yaklaşımına uymaz. Yalnızca kiremit eğimi (8, 9) düze yakın.
- Sırt karoları elmasın üstüne doğru yükseliyor (kapak 3B). 2:1 maskeyle kırpılınca sırt kapağı kısmen kesilir.
- Görsel olarak saz (0-3) ve kiremit (8-11) renk/doku olarak hoş; asıl sorun şekil ve deliklerde. Kullanıcı ham sayfaya bakıp karar verebilir.

## Öneri (Sorular)
- Yeni iş: `create_tiles_pro` yerine `create_isometric_tile` ile tek tek, ya da `create_building_kit` (`layout=materials`, `roof_profile` ayarlı; 007'deki gibi 10 gen) ile çatı parçaları üretip kırpmak. Düz eğim için komutta "flat, no peak, uniform pattern, orthographic from above" vurgusu eklenebilir.
- Stil referansı olarak çimen/toprak yerine kabul edilmiş duvar karosu (325) kullanıp `style_options.color_palette` açmak, kiremit ve arduvaz doygunluk sapmasını azaltabilir.
