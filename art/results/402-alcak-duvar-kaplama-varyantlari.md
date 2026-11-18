unity-import: pending

# 402 Alçak ve yarım kaplama varyantları (bulut işçisi, şerit 1, 2026-11-18)

Yan yana sayfası: [402-yanyana.png](402-yanyana.png)

## Araç ve ayarlar
- `pixellab: false`, **0 generation**. `get_balance`: 4170,9 / 5000 (Tier 2, plan budget.md ile uyumlu).
- `tools/art/wall_variants.py` repoda yoktu; aynı yöntem `art/tmp/wall_variants.py` olarak yeniden yazıldı (327 notu): sütun bazında en alttaki opak pikselden yukarı 10 px (`low`) ya da 20 px (`half`) kalır, üst piksele +45 RGB açık kapak. Tuval ve pivot değişmedi.
- Masaüstü kopyası: yerel.

## Dosyalar (6; hepsi 102x115, pivot x=32 sw_ne / 70 se_nw, y=19 ALTTAN)
- iso_wall_finish_panel_low_sw_ne / _se_nw, panel_half_sw_ne / _se_nw
- iso_wall_finish_tapestry_low_sw_ne, tapestry_half_sw_ne

## Doğrulama
| Dosya | validate_iso | Alt kenar farkı | Palete yakın | Doygunluk | Stil |
|---|---|---|---|---|---|
| panel half se_nw | geçti | 0 | %100 | x1.05 | GEÇTİ |
| panel half sw_ne | geçti | 0 | %100 | x0.96 | GEÇTİ |
| panel low se_nw | geçti | 0 | %100 | x1.04 | GEÇTİ |
| panel low sw_ne | geçti | 0 | %100 | x0.95 | GEÇTİ |
| tapestry half sw_ne | geçti | 0 | %100 | x1.40 | UYARI (ailesinden doygun) |
| tapestry low sw_ne | geçti | 0 | %100 | x1.27 | UYARI (ailesinden doygun) |

## Sorunlar
- Duvar çaprazı olduğu için "üst kenar düz" çapraz-paralel okundu (327 ile aynı).
- Dokuma uyarısı kaynak tam görselden gelir (kırpma rengi değiştirmez).
- Öneri: betik `tools/art/wall_variants.py` olarak ayrı PR ile alınabilir.
