---
id: "451"
title: "Var olan eşyalara ikinci renk maskesi (8)"
status: running
claimed_at: 2026-11-19T21:31Z
approved_by: user        # otomatik akış (art/budget.md); karar: kullanıcı 19.11.2026 ("bunların hepsini yapalım", varyant çeşitliliği), Project belgesi 45
priority: 20
family: esya
style_gate: false
cost_est: 0
cost_max: 1
depends_on: ["450"]
max_attempts: 1
pixellab: false
outputs:
  - Assets/Art/Iso/<eşyanın görsel adı>_accent.png (her satır için bir tane; görsel `_aday` içindeyse maske de oraya)
created: 2026-11-19
---
# 451 Var olan eşyalara ikinci renk maskesi (8)

## Amaç
Belge 45 §10: oyuncu sandalyenin minderini, sandığın kulpunu, masanın ayağını ayrı boyar. Maske olmadan ikinci renk yalnızca kartta görünür.

## Tarif
PixelLab yok. Her eşyanın var olan görselini aç, ikinci renge ait bölgenin rengini seç (gerekirse `--box` ile alanı daralt) ve `python tools/art/accent_mask.py <görsel> --key "#RRGGBB" [--key ...] [--box x0,y0,x1,y1]` çalıştır. Görseli olmayan eşyayı atla ve raporla.

| items.json id | Görsel | Ayrılacak bölge |
|---|---|---|
| `item.table_wood` | `iso_item_table_wood_2x1_se_nw` | ayak ve kenar |
| `item.chair_wood` | `iso_item_chair_wood_sw` | oturak (minder yeri) |
| `item.chair_carved` | `iso_item_chair_carved_se` | oturak |
| `item.chest_wood` | `iso_item_chest_wood_se` | demir kulp ve kuşak |
| `item.bed_straw` | `iso_item_bed_straw_2x1_se_nw` | yorgan |
| `item.hearth_stone` | `iso_item_hearth_stone_unlit_se` | ocak demiri |
| `item.barrel` | `(eşlemedeki görsel)` | çember |
| `item.bench_long` | `(eşlemedeki görsel)` | oturak |

## Doğrulama
`python tools/art/validate_iso.py <dosya> ...`, `python tools/art/style.py check --family esya <dosya> ...`. Dönem kuralları (CLAUDE.md, Project belgesi 20): ortaçağ sonrası icat yok, Yeni Dünya malzemesi yok.

## Notlar
- Ana görsellere dokunulmaz; yalnızca `_accent` dosyaları eklenir.

<!-- İşçi bundan sonrasına dokunmaz; kullanıcı kararı:
status: review → accepted ya da rejected yap, rejected ise nedenini buraya yaz. -->
