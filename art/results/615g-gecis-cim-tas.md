unity-import: pending

# 615g Geçiş seti: çim ↔ taş döşeme (KISMİ: 13/28 karo, iş `failed`)

- Araç: `create_tiles_pro`, `tile_feature=tileset`, isometric 64, `tile_view_angle=30`, `tile_depth_ratio=0`, `outline_mode=segmentation` (004e/613g ayarları). Palet 8 renk: grass_a (`#75b560 #639d54 #528647 #76af64`) + `iso_floor_cobble_a`'dan taş (`#8c8881 #71726c #7f7d76 #5b5a59`). Birinci zemin çim.
- Çeşit a: tile id `6510f680-67fc-41fc-a5a0-c8676d2f3b07`, seed 6151, boundary_seed 61, amplitude 0,25, 10 gen.
- Çeşit b: tile id `35770915-1dc5-437c-a93a-fd28ccd53de4`, seed 6152, boundary_seed 72, amplitude **0,30** (araç en çok 0,3 kabul ediyor; tarifteki 0,35 verilemedi, 004e'deki sapmayla aynı), 10 gen.
- Toplam 20 generation (cost_est 22 altında). Hız sınırı hatası yok. `get_balance` sonrası: 9809 (paralel şeritler dahil).
- Maske eşlemesi 004e ile aynı (bit0 üst, 1 sağ, 2 alt, 3 sol; 1 = taş; PixelLab set bit = çim, NW=8 NE=4 SW=2 SE=1). Ham karolar 64x32 tam dikdörtgen geldi, yalnızca grass_a alfa maskesiyle kırpıldı (64 maske dışı piksel), içerik boyanmadı.
- Masaüstü kopyası: yerel
- Yan yana: art/results/615g-yanyana.png (teslim edilen 13 karo)

## Teslim edilen (13 dosya, 64x32, pivot x=32 y=16 alttan)
`Assets/Art/Iso/iso_trans_grass_stone_{01..07,09..14}_a.png`

## Teslim edilmeyen (art/results/615g-red/)
- **Çeşit b (14 karo): reddedildi.** Çim bölgesi oluşmadı: tam çim karosu (tile_15) bile taş zemin üstüne çim kırıntısı; bütün karolar taş ağırlıklı, geçiş okunmuyor. Ayrıca doygunluk x0,17-0,34 (UYARI "soluk").
- **08_a (tile_13 çeşit a): reddedildi.** Yalnızca SW köşesi taş olmalıydı; karo büyük ölçüde taş (çim oranı %14, beklenen ~%75). Şekil sorunu, `fit` ile çözülmez.

## Doğrulama (teslim edilenler)
| Kontrol | Sonuç |
|---|---|
| `validate_iso.py --kind ground` | 13/13 geçti (maske dışı 0, delik 0, 8 renk) |
| `style.py check --family zemin` | 7 GEÇTİ (01,02,04,05,06,12,13), 6 UYARI (03,07,09,10,11,14: "ailesinden soluk", x0,33-0,56; taş gri olduğu için beklenen), KALDI yok |
Gölge pişirilmemiş, ışık sol üstten. Yön gözle doğrulandı (01 üstte taş, 02 sağda, 04 altta).

## Sorunlar
- Set eksik (08 yok, b çeşidi yok): kod yalnızca a çeşidiyle ve 08 olmadan bağlanamaz. Tek deneme kuralı gereği yeniden denenmedi.
- Çeşit a'nın tam taş karosu (tile_0) %72 yosunlu/yeşil; dolgu olarak teslim edilmedi (dolgu 612/346'dan).
- Döşeme testi ve 12 karoluk kıvrılan önizleme yapılmadı (geçiş karoları tek başına döşenmez).
- Öneri: 08 ve b çeşidi için yeni iş (farklı seed, palet 4+4 yerine daha belirgin çim/taş ayrımı, ör. daha koyu taş).
