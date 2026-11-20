unity-import: pending

# 614g Geçiş seti: çim ↔ kum (14 maske x 2 çeşit)

- Araç: `create_tiles_pro`, `tile_feature=tileset`, isometric 64, `tile_view_angle=30`, `tile_depth_ratio=0`, `outline_mode=segmentation`, 8 renkli palet (grass_a + sand_a, 613g ile aynı), `boundary_amplitude=0.3` (araç üst sınırı; tarifteki b için 0,35 kabul edilmiyor, 004e'deki sapmayla aynı).
- 2 paralel çağrı, her biri 10 generation, toplam 20 (cost_est 22, cost_max 34 içinde). Hız sınırı hatası yok.
  - a: tile 53f6af75-a83d-43f8-b4ac-37d203933d13, seed 6131, boundary_seed 51 (613g ile aynı ayarlar).
  - b: tile 2fe4e13d-1a2c-482b-9b8b-cbcfcb7d8eb0, seed 6132, boundary_seed 52.
- Maske eşlemesi 004e/613g ile aynı (NN = (!N&8?1:0)|(!N&4?2:0)|(!N&1?4:0)|(!N&2?8:0); 1 = kum). Aynalama/döndürme yok. Karolar yalnızca grass_a elmas maskesiyle kırpıldı (karo başına 64 maske dışı piksel silindi).
- `get_balance` sonrası: 9819 (paralel şeritler dahil).
- Masaüstü kopyası: yerel
- Yan yana: art/results/614g-yanyana.png

## Dosyalar (64x32, pivot x=32, pivot y=16 alttan)
`Assets/Art/Iso/iso_trans_grass_sand_NN_{a,b}.png`, NN 01-14. Teslim edilen 24 yeni dosya: a için 02, 04, 06, 08-14; b için 01-14 tamamı.
**Teslim edilmeyen 4 dosya:** a 01, 03, 05, 07 zaten var (613g pilotu, `cp -n` üzerine yazmaz).

## Doğrulama
28/28 `validate_iso.py --kind ground` geçti; `style.py check --family zemin` 28 GEÇTİ, UYARI/KALDI yok (palete yakın %100, doygunluk x0,94-1,19). Gölge pişirilmemiş, ışık sol üstten (gözle bakılmadı).

## Sorunlar
- **a çeşidi karışık:** 613g'deki 4 karo aynı tohumla bile yeniden üretimde birebir aynı çıkmadı (PixelLab deterministik değil). Diskteki a 01/03/05/07 eski çağrıdan, öteki a karoları yeni çağrıdan; köşe geçişleri karolar arasında uyuşmayabilir. Öneri: kabulde bu 4 dosyayı yeni a setiyle değiştirmek (ham karolar commit edilmedi; gerekirse yeni iş) ya da yalnızca b çeşidini kullanmak (tutarlı tam set).
- Döşeme testi, 12 karoluk kıvrılan önizleme ve a/b çeşit karışımı dikiş testi yapılmadı (613g notlarıyla aynı); kod önizlemesinde bakılmalı.
