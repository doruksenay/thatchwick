---
id: "001"
title: Çimen karosu a
status: blocked
approved_by: auto
cost_est: 10
cost_max: 15
depends_on: []
max_attempts: 1
pixellab: true
outputs:
  - Assets/Art/Iso/iso_ground_grass_a.png
created: 2026-11-08
---
# 001 Çimen karosu a

## Amaç
Zemin setinin stil referansı. Onaylanırsa 002 (çimen b/c, toprak) ve 003 (geçiş seti) bundan türer. Tur 3'te `create_isometric_tile` ile yan yüzlü, gürültülü ve dikişli çıktı gelmişti (başarısız); Tier 1 ile Pro araç denenecek.

## Tarif
- Araç: `create_tiles_pro`, bağımsız karolar.
- Ayarlar: isometric, `tile_size=64`, `tile_view_angle=30`, `tile_depth_ratio=0` (yan yüz yok), `tile_flat_top_px=2`, `outline_mode=segmentation`, `max_colors=5`.
- Açıklama: "flat short grass, medieval village ground, muted green, 3-5 tones, no flowers, no noise, no shadow, top face only".
- Aday setten (en çok 16 karo) tek bir en iyisini seç: düz, az tonlu, gürültüsüz, kenarlarda taşan tutam yok. Seçimin gerekçesini yaz.
- Kaynak: `docs/gorsel-yonerge.md` (64x32 elmas, tam 2:1, anti-alias yok, saydam arka plan, gölge pişirilmez).

## Doğrulama
- `python tools/art/validate_iso.py <dosya> --kind ground --max-colors 6`
- Tam 2:1 değilse yalnızca elmas maskesiyle kırp, raporda belirt. Maske yetmeyecek kadar bozuksa `failed`.
- Döşeme testi: 8x8 elmas ızgarada (32,16 adımıyla) döş, PNG kaydet, komşu karo dikişinde ortalama parlaklık sıçramasını (dikiş / karo içi oranı) sayıyla yaz. Tur 3 sonucu 1,75-1,80 idi (başarısız); hedef 1,25'in altı, aşılırsa nedenini yaz.

## Notlar
Dosya adı: `iso_ground_grass_a.png`. Pivot: elmas merkezi (32,16). Seçilmeyen adayları Assets'e koyma.

## Sorular
BLOKE (2026-11-08, Unity/geliştirme sohbeti): 001 işi zemin oturumu tarafından (eski brief düzeniyle) yapıldı: iso_ground_grass_a.png = aday 9 olarak teslim edildi (Assets/Art/Iso). Kullanıcı kabulü bekleniyor; çift harcama olmasın diye bloke. Zemin oturumu bitince kullanıcı durumu accepted/rejected yapar ya da pending'e geri alır.
