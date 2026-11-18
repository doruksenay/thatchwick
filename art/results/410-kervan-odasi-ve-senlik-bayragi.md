unity-import: pending

# 410 Kervan Odası (8 eşya) ve şenlik bayrağı: 2/9 teslim, review

Yan yana sayfası: [410-yanyana.png](410-yanyana.png). Teslim edilmeyenler: `art/results/410-red/`. Masaüstü kopyası: yerel.

## Araç ve ayarlar
- `create_map_object`, stil eşleme, özel maske; `low detail`, `basic shading`, `single color outline`. Yer eşyaları `high top-down`, asılı ve duvar eşyaları ile bayrak `side`.
- Kılavuz: sandık, ekran, divan, halı `footprint_guide.py` (sandık 80x72 h24, ekran 112x88 h40, divan 144x96 h28, halı 176x96 `--n 3 --m 2` h2). Küçük eşyalar (ibrik, çini kase) 32x32 tuvalde 12x6 elmas; kandil 32x40 boş kılavuz; çini pano 40x40 "/" paralelkenar (184,160,122); bayrak 192x64, iki direk (renk 120,96,70) ve dikdörtgen maske. Betikler `art/tmp410/` (commit edilmez).
- Açıklama: tarif metni + 301 ortak eki (kahverengi ahşap ibaresi çıkarıldı, "medieval merchant caravan object", boyalı ahşap ve pirinç).
- 9 çağrı x 1 gen = 9 (cost_max 18), hepsi paralel, yeniden deneme yok. `fit` ücretsiz.
- `get_balance` sonrası: 4100,9 (paralel şeritler de harcıyor).
- **Benim hatam:** ekran (2x1) ve divan (3x1) kılavuzunu `--axis y` ile verdim; tarif "uzun ekseni \" der (390'daki gibi `--axis x`). İkisi "/" çıktı. Ek olarak ekranın altında yer döşemesi (platform) çıktı ("no extra objects" çiğnendi). Tek deneme kuralı gereği yeniden denemedim.

## Sonuç özeti
| # | Eşya | Durum | Not |
|---|---|---|---|
| 1 | brass_ewer | teslim | stil GEÇTİ (%90, x1.08) |
| 2 | tile_bowl | TESLİM EDİLMEDİ | ham ve `fit` stil KALDI (palet dışı mavi/turkuaz; %34 → %62) |
| 3 | lattice_screen | TESLİM EDİLMEDİ | eksen "/" (yanlış), altında zemin platformu; stil GEÇTİ |
| 4 | brass_lantern | teslim (`fit` x0.81, UYARI) | ham KALDI (x1.42) |
| 5 | cushioned_divan | TESLİM EDİLMEDİ | eksen "/" (yanlış); stil GEÇTİ, görsel iyi: yatay ayna ya da 410b ile `--axis x` |
| 6 | caravan_rug | TESLİM EDİLMEDİ | kökboya kırmızısı palet dışı: ham %14, `fit` %23 KALDI. Görsel tanınıyor |
| 7 | tile_panel | TESLİM EDİLMEDİ | stil KALDI (%79, ort. uzaklık 16,6; palet dışı mavi) |
| 8 | spice_chest | TESLİM EDİLMEDİ | ham KALDI (%53, x1.61); `fit` sonrası UYARI ama 59 renk > 48 |
| 9 | festive_bunting | TESLİM EDİLMEDİ | çok renkli bayraklar palet dışı: ham %16, `fit` %50 KALDI |

## Dosyalar (`Assets/Art/Iso/_aday/`; pivot ALTTAN)
| Dosya | Boyut | Pivot x | Pivot y |
|---|---|---|---|
| iso_item_brass_ewer_se.png | 32x32 | 16 | 7 |
| iso_item_brass_lantern_se.png | 32x40 | 16 | 20 (asma eşya; kullanıcı ayarlar) |

## Doğrulama (`validate_iso.py --kind object --max-colors 48`)
| Dosya | Opak sınır | Renk | Yarı saydam | Sonuç |
|---|---|---|---|---|
| ewer | x9-21 y4-28 | 19 | 0 | geçti |
| lantern (fit) | x9-22 y2-36 | 17 | 0 | geçti |
| (red) bowl fit | x8-23 y11-21 | 29 | 0 | geçti |
| (red) panel | x6-32 y3-32 | 14 | 0 | geçti |
| (red) chest fit | x8-71 y7-64 | 59 | 0 | BAŞARISIZ (renk) |
| (red) screen | x11-100 y7-80 | 31 | 0 | geçti |
| (red) divan | x10-133 y8-88 | 46 | 0 | geçti |
| (red) rug fit | x8-167 y5-88 | 33 | 0 | geçti |
| (red) bunting fit | x12-179 y10-56 | 21 | 0 | geçti |

## Stil denetimi (`style.py check --family esya`)
| Dosya | Palete yakın | Ort. uzaklık | Doygunluk | Sonuç |
|---|---|---|---|---|
| ewer | %90 | 9.0 | x1.08 | GEÇTİ |
| lantern (fit) | %83 | 10.3 | x1.22 | UYARI (palete uzak) |
| lantern ham | %69 | 13.0 | x1.42 | KALDI |
| bowl ham / fit | %34 / %62 | 22.5 / 19.9 | x1.36 / x1.15 | KALDI |
| panel | %79 | 16.6 | x0.85 | KALDI |
| chest ham / fit | %53 / %90 | 20.8 / 13.1 | x1.61 / x1.15 | KALDI / UYARI (59 renk) |
| screen | %96 | 8.1 | x0.80 | GEÇTİ |
| divan | %94 | 9.8 | x0.84 | GEÇTİ |
| rug ham / fit | %14 / %23 | 31.5 / 22.8 | x1.53 / x1.14 | KALDI |
| bunting ham / fit | %16 / %50 | 42.6 / 28.8 | x1.60 / x1.15 | KALDI |

## Sorunlar ve öneriler
- Mavi/turkuaz çini, kökboya kırmızısı halı ve dört renkli bayrak `esya` ailesinin toprak paletine sığmıyor; `fit` doygunluğu indiriyor ama ton sorununu çözmüyor. Karar kullanıcıda: bu üç öğe için palet istisnası (stil kapısı gevşetme) ya da aileye "kervan" renk eklemesi. Ham ve `fit` hâlleri `410-red/` altında; halı ve bayrak şekil olarak doğru.
- Sandık `fit` ile 59 renk (>48): `reduce_colors` (~0,1 gen) onayıyla kurtarılabilir (`410-red/chest_fit.png`).
- Divan "/" çıktı; kullanıcı yatay aynayı kabul ederse (ışık sağ-üst olur) kullanılabilir, yoksa `--axis x` ile yeniden (1 gen). Ekran da `--axis x` ile yeniden gerekir (zemin platformu çıkmaması için "no base, no platform" eklenmeli).
- Kandil: "asma" eşya, 385'teki tavan katmanı yöntemi yerine 32x40 side kılavuzsuz; zincir ucu üstte. Işıklı olduğu için `--light` denetimi yapılmadı.
- Döşeme testi, ışık ve ayna ölçümü yapılmadı; yan yana sayfasına bakılmalı.
