unity-import: pending

# 306 Duvar eşyası pilotu (rapor): 5/5 teslim, review

Yan yana sayfası: [306-yanyana.png](306-yanyana.png). Önceki (reddedilen) meşale denemeleri: `art/results/306-red/`. Masaüstü kopyası: yerel.

## Araç ve ayarlar
- Meşale: önceki turun `torch_deneme2.png`'si (kullanıcı kararı 2026-11-18, kabul); yeniden üretilmedi, `style.py check --light` ve `validate_iso.py` ile doğrulanıp teslim edildi (0 gen bu turda).
- Kandil, raf, ot demeti, alet askısı: `create_map_object` stil eşleme, `side` görünüm, `low detail`, `basic shading`, `single color outline`, tarifteki açıklama + duvar eki + takım eki. 4 çağrı, paralel, çağrı başına 1 gen = 4 generation. Tek deneme.
- Kılavuz ve maske: `tools/art/wall_guide.py` depoda yok; eşdeğeri `art/tmp/mkguide.py` ile (commit edilmez) üretildi: duvar rengi (184,160,122) "/" eğimli paralelkenar, maske aynı şekil. Parça başına paralelkenar: kandil 32x36, raf 48x22, ot demeti 24x34, alet askısı 48x40.
- get_balance sonrası: 4929 / 5000 (iş başı 4945; paralel şeritler de harcıyor).

## Dosyalar (pivot = görselin merkezi, ALTTAN)
| Dosya (`_aday/`) | Boyut | Pivot x | Pivot y (alttan) |
|---|---|---|---|
| iso_wall_item_torch_wall_sw_ne.png | 48x64 | 24 | 32 |
| iso_wall_item_oil_lamp_wall_sw_ne.png | 48x64 | 24 | 32 |
| iso_wall_item_wall_shelf_sw_ne.png | 64x48 | 32 | 24 |
| iso_wall_item_herb_bundle_wall_sw_ne.png | 40x56 | 20 | 28 |
| iso_wall_item_tool_rack_wall_sw_ne.png | 64x64 | 32 | 32 |

## Doğrulama
| Eşya | Opak kutu | Renk | Yarı saydam | Maske dışı | Kılavuz rengi | Stil denetimi |
|---|---|---|---|---|---|---|
| meşale | x12-32 y13-44 | 40 | 0 | 0 | 0 | GEÇTİ (%98, x0.89, `--light`, ışık %25) |
| kandil | x10-39 y15-42 | 33 | 0 | 0 | 0 | UYARI (%99, x0.35 ailesinden soluk, `--light`, ışık %7) |
| raf | x8-55 y12-39 | 27 | 0 | 0 | 0 | GEÇTİ (%97, x0.93) |
| ot demeti | x8-31 y7-44 | 13 | 0 | 0 | 0 | UYARI (%83, ort. uzaklık 11.1, palete uzak) |
| alet askısı | x10-53 y8-59 | 15 | 0 | 0 | 0 | GEÇTİ (%100, x1.10) |

`validate_iso.py` hepsi geçti. KALDI yok. Döşeme testi duvar eşyası için geçerli değil.

## Sorunlar / gözlem
- Ot demeti yeşil (taze görünüyor, "kurutulmuş" ton değil) ve paleti aşıyor: UYARI, kullanıcı göz kararı versin.
- Kandil ailesinden soluk (x0.35): alev küçük, gövde koyu.
- Alet askısı: üç alet (balta, çapa, testere) ayırt edilmesi yan yana sayfasından kontrol edilmeli; 64x64'te çok küçük.
- Işık/ayna ve ahşap ton/doygunluk tablosu bu turda ölçülmedi (yan yana sayfasına bakın). Ayna: "\" yüzü aynayla, ışık sağ-üstten gelir.
- `tools/art/wall_guide.py` yok: kılavuz betiği kalıcı olmalıysa kod/PR oturumu eklemeli.
