unity-import: pending

# 564 Tarım eşyaları (rapor): 11/12 teslim, review

Yan yana sayfası: [564-yanyana.png](564-yanyana.png) (teslim edilen 11). Teslim edilmeyen / elenen denemeler: `art/results/564-red/`. Masaüstü kopyası: yerel.

## Araç ve ayarlar
`create_map_object`, stil eşleme, referanssız kılavuz + maske (`footprint_guide.py`; kılavuz/maske 1 bit PNG'ye çevrilip gönderildi, base64 kısaltmak için), `high top-down`, `low detail`, `basic shading`, `single color outline`, 301 ortak eki + takım eki (1 Köylü: açık çam/ip bağlama; 2 Zanaatkâr: koyu meşe/demir; 3 Tüccar: oymalı koyu meşe/pirinç). 12 ilk çağrı paralel + 2 ikinci deneme (duvar bebeği: KALDI; ambar: çimen zemin lekesi) = 14 generation (cost_max 24 içinde). get_balance iş sonu: 1930,7 (paralel şeritler de harcıyor; bu işin payı 14). Duvar eşyası (bebek) 306 yöntemiyle: 40x56 tuval, "/" paralelkenar 24x34, duvar rengi (184,160,122), `side` görünüm.
Tuval ve kılavuz yüksekliği: tohum sandığı 1 80x64 h24; 2 80x80 h36; 3 (2x1) 112x96 h40; imbik 80x72 h30; fidelik 80x64 h18; sıcak yastık 80x64 h20; saban (2x1) 112x80 h28; baykuş kutusu 80x112 h60; sırık 80x88 h44; korkuluk 80x112 h64; ambar (2x2) 144x160 h80.

## Sonuç özeti
| # | Eşya | Durum | Not |
|---|---|---|---|
| 1 | tohum sandığı 1 | teslim | GEÇTİ |
| 2 | tohum sandığı 2 | teslim (UYARI) | ailesinden soluk x0,56 |
| 3 | tohum sandığı 3 | teslim | GEÇTİ; sol yarıda ayak izi %74 |
| 4 | imbik | teslim | 49 renk (>48, ⚠, yalnızca rapor) |
| 5 | fidelik | teslim | 61 renk (>48, ⚠) |
| 6 | sıcak yastık | teslim | tepede küçük buhar var (tarifte "steaming") |
| 7 | saban (2x1) | teslim | ayak izi dar (%48), yerde yatan alet |
| 8 | baykuş kutusu | teslim | direk: ayak izi %47 (dar) |
| 9 | ekin sırığı | teslim | 49 renk (⚠); üç sırık, ayak izi %34 (dar) |
| 10 | duvar bebeği | TESLİM EDİLMEDİ | iki denemede de stil KALDI (d1 %60 palete yakın, x1,91; d2 x1,60 doygun); kırmızı kurdele |
| 11 | giydirilmiş korkuluk | teslim | tabanda küçük tahta kaide var |
| 12 | köy tohum ambarı (2x2) | teslim (2. deneme) | d1'de çimen/taş zemin lekesi vardı, elendi; d2 temiz, GEÇTİ |

## Dosyalar (Assets/Art/Iso/_aday/, pivot ALTTAN)
| Yol | Piksel | Pivot x | Pivot y |
|---|---|---|---|
| iso_item_seed_chest_1_se.png | 80x64 | 40 | 24 |
| iso_item_seed_chest_2_se.png | 80x80 | 40 | 24 |
| iso_item_seed_chest_3_se.png | 112x96 | 56 | 32 |
| iso_item_alembic_se.png | 80x72 | 40 | 24 |
| iso_item_seedling_tray_se.png | 80x64 | 40 | 24 |
| iso_item_hotbed_se.png | 80x64 | 40 | 24 |
| iso_item_plough_se.png | 112x80 | 56 | 32 |
| iso_item_owl_box_se.png | 80x112 | 40 | 24 |
| iso_item_crop_stake_se.png | 80x88 | 40 | 24 |
| iso_item_scarecrow_dressed_se.png | 80x112 | 40 | 24 |
| iso_item_village_seed_store_se.png | 144x160 | 72 | 40 |

## Doğrulama (validate_iso `--max-colors 48`) ve stil
| Dosya | Renk | Yarı saydam | Maske dışı | Kılavuz | Ayak izi sol/sağ | validate | Stil (esya) |
|---|---|---|---|---|---|---|---|
| sandık 1 | 21 | 0 | 0 | 0 | 90/79% | geçti | GEÇTİ %100 x1.03 |
| sandık 2 | 32 | 0 | 0 | 0 | 100/100% | geçti | UYARI x0.56 soluk |
| sandık 3 | 34 | 0 | 0 | 0 | 74/98% | geçti | GEÇTİ %98 x0.85 |
| imbik | 49 | 0 | 0 | 0 | 100/100% | renk ⚠ | GEÇTİ %90 x0.90 |
| fidelik | 61 | 0 | 0 | 0 | 100/100% | renk ⚠ | GEÇTİ %89 x1.04 |
| sıcak yastık | 43 | 0 | 0 | 0 | 98/97% | geçti | GEÇTİ %100 x0.79 |
| saban | 41 | 0 | 0 | 0 | 48/47% | geçti | GEÇTİ %100 x0.83 |
| baykuş kutusu | 13 | 0 | 0 | 0 | 47/47% | geçti | GEÇTİ %98 x1.02 |
| sırık | 49 | 0 | 0 | 0 | 33/35% | renk ⚠ | GEÇTİ %99 x0.65 |
| korkuluk | 41 | 0 | 0 | 0 | 99/99% | geçti | GEÇTİ %100 x0.88 |
| ambar (d2) | 44 | 0 | 0 | 0 | ölçülmedi (d1: 99%) | geçti | GEÇTİ %90 x0.81 |

Hepsi tabana değmiyor (en alt opak satırın altında 7+ boş satır). Tam 2:1 olmayan çıktı yok; kırpma yok. Işık/ayna ve döşeme testi ölçülmedi (yan yana sayfasına bakın).

## Sorunlar
- Duvar bebeği iki denemede de stil denetimini geçemedi (kırmızı kurdele doygun); teslim edilmedi. Kurdelesiz ya da kahve kurdeleli yeni iş gerekebilir.
- 3 dosyada renk 48 üstü (301 kuralı: yalnızca raporlandı, `reduce_colors` yapılmadı).
- Araçlarda `seed` yok; ikinci deneme aynı kılavuz ve maskeyle.
- Kılavuz/maske PNG'sinin base64'ü tekrar eden dizeler yüzünden aktarımda bozuluyor (ilk iki çağrı reddedildi, ücret yok); 1 bit palet PNG ile çözüldü. Öneri: `footprint_guide.py` 1 bit palet PNG çıktısı versin.
