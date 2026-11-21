unity-import: pending

# 900 HUD meşe düzeltmeleri

- İş: 900, bulut işçisi (şerit 1), 2026-11-21. Yön: `mese2`.
- Araç: parça başına `create_image_pixflux` (tracker, card, money, btn_build; `seed=0`, `color_image` = `palette_arayuz.json` 128 renk, kılavuz `init_image`) ve `create_image_pixen` 16x16 (badge ve ihtiyaç ikonları). Tracker/btn_build `no_background=true`; card ve money `no_background=false` + kenara bağlı tek renkli zemin silme (810 dersi; içerik boyanmadı).
- Kılavuzlar `art/tmp/900/` (commit edilmez). Tracker kılavuzu: 6 px uçlar + 24. sütunun tekrarı. Card kılavuzu: eski card, krem x10-122 y10-102 boyanmış. Money kılavuzu: elle çizilmiş tek satır iki yuva, 10x10 opak ikon yeri. Btn_build kılavuzu: yaprak kremle örtülü, gri üçgen mala çizilmiş.
- Harcanan: **18 generation** (tracker 2, card 1, money 1, btn_build 1, pixen: badge 2, hunger 1, energy 1, warmth 2, fun 2, hygiene 1, social 2, toilet 2). Tavan 25.
- Kalan (get_balance): **9734 / 10000** (Tier 3, yenilenme 2026-12-20; diğer şeritlerin harcaması dahil).
- Yan yana: `art/results/900-yanyana.png`, `900-yanyana-card.png` (eski/yeni card), stil sayfası `art/results/900-stil-yanyana.png` (arayüz ailesinde referans yok: 0 referans; yalnızca money ve card dahil). 9-slice testi: `art/results/900-9slice-test.png`. İkonlar x1/x2/x3: `art/results/900-ikonlar-x3.png`. Teslim edilmeyen ham çıktılar: `art/results/900-red/`.
- Masaüstü kopyası: yerel.

## Teslim edilenler (`Assets/Art/Iso/_aday/`)
| Dosya | Boyut | Pivot x | Pivot y (alttan) | Not |
|---|---|---|---|---|
| ui_hud2_mese2_money.png | 72x20 | 36 | 0 | Tek satır, iki yuva yan yana, 10x10 opak koyu ikon yeri, saydam delik yok |
| ui_hud2_mese2_tracker_v2.png | 48x16 | 24 | 0 | 9-slice köşe 6. Orta bölge Python ile sabitlendi (aşağıda) |
| ui_hud2_mese2_card_v2.png | 132x112 | 66 | 0 | Çerçeve dört kenarda 9-10 px; iç alan %99,98 düz krem (x10-122 y10-102) |
| ui_hud2_mese2_btn_build_v2.png | 32x32 | 16 | 1 | Demir çekiç + gri mala ağızlı çapraz alet; yaprak yok |
| ui_hud2_mese2_btn_build_down_v2.png | 32x32 | 16 | 2 | Python: iç daire 0,72 çarpanıyla koyulaştırılıp palete oturtuldu, 1 px aşağı, alt satır silindi |
| ui_hud2_mese2_btn_book_down_v2.png | 32x32 | 16 | 2 | Aynı yöntem, `btn_book`tan |
| ui_hud2_mese2_need_hunger_v2.png | 12x12 | 6 | 0 | Somun ekmek |
| ui_hud2_mese2_need_energy_v2.png | 12x12 | 6 | 0 | Hilal |
| ui_hud2_mese2_need_warmth_v2.png | 12x12 | 6 | 0 | Alev; üstten alta 4 px dış kırpıldı (alt kütük kenarı); palete oturtuldu |
| ui_hud2_mese2_need_fun_v2.png | 12x12 | 6 | 2 | Topaç; yalnızca 8x8 yer kaplıyor (ikinci deneme) |
| ui_hud2_mese2_need_hygiene_v2.png | 12x12 | 6 | 0 | Büyük mavi su damlası, tek şekil; palete oturtuldu |

## Teslim EDİLMEYENLER (sorun)
| Parça | Neden |
|---|---|
| `badge_v2` | 1. deneme: ünlem net ama 14x14 (10x10'a sığmıyor, ölçek düşürülmez). 2. deneme: 10x10 ama ünlem kayboldu (kırmızı top). İki hak bitti. Ham: `900-red/badge_raw.png` (14x14, ünlemli), `badge2_raw.png` |
| `need_social_v2` | İki denemede de **tek kişi** çıktı (iki baş şartı yok). Ham: `social_raw.png`, `social2_raw.png` |
| `need_toilet_v2` | 1. deneme tanınmıyor (oturak gibi değil), 2. deneme testi/çömlek gibi; "kapaklı ahşap oturak" okunmuyor. Ham: `toilet_raw.png`, `toilet2_raw.png` |

Öneri (yeni iş gerekir): bu üçü için pixen yerine `create_image_pixflux` 32x32 + kılavuz (iki kişi/oturak elle çizilmiş), ya da `pixelart_workbench` ile elle çizim.

## Doğrulama
| Dosya | validate_iso | style.py check (arayuz) | Tarif ölçütü |
|---|---|---|---|
| money | geçti (10 renk) | GEÇTİ %100 | Tek satır, 2 yuva, opak ikon yerleri: GEÇTİ |
| tracker_v2 | geçti | GEÇTİ %100 | x6-41 sütunları aynı: GEÇTİ, ama **ikinci denemede de ham çıktıda orta sütunlar farklıydı** (hak bitti); uçlar ham çıktıdan, orta 24. sütunun tekrarı (kod ile sabitlendi). 16 px yüksek (tarif 18) |
| card_v2 | geçti | GEÇTİ %100 | Çerçeve ≥8 px dört kenarda; iç %99,98 aynı krem: GEÇTİ |
| btn_build_v2 | geçti | GEÇTİ %100 | Çekiç+mala, yeşil yok: GEÇTİ (mala ucu küreğe yakın okunuyor, kullanıcı gözüne bakılmalı) |
| btn_build_down_v2 / btn_book_down_v2 | geçti | GEÇTİ | Normalden 325 / 364 px farklı |
| need_hunger/energy/fun | geçti | GEÇTİ (%97-100) | 12x12 içinde, tanınır |
| need_hygiene, need_warmth | geçti | ilk KALDI (%39 / %65 palet), **palete oturtulduktan sonra GEÇTİ %100**. Ham hâller commit edilmedi (`art/tmp`) | Tanınır |

9-slice (48, 120, 200) dikişsiz; bkz. `900-9slice-test.png`. Tracker 48x16 (ince): 18'e göre iki satır az.

## Sorunlar
1. Üç parça teslim edilmedi (badge, social, toilet), yukarıda.
2. Pixen'e palet verilemiyor; iki ikon sonradan palete oturtuldu (palet uyumu için güvenli, ama sert renk basamakları olabilir).
3. `style.py fit` arayüz ailesinde referans olmadığı için çalışmadı.
4. `tools/art` değiştirilmedi.
