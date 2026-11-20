unity-import: pending

# 771 Meydan yer tutucuları (3. deneme) — rapor

Yan yana sayfası: [771-yanyana.png](771-yanyana.png) (5 teslim + esya referansları). Teslim edilmeyenler: `art/results/771-red/` (4 dosya).
Masaüstü kopyası: yerel (bulutta yapılamaz).

## Araç ve ayarlar
`create_map_object`, stil eşleme (kılavuz + maske `tools/art/footprint_guide.py`), `high top-down`, `low detail`, `basic shading`, `single color outline`, 736'daki ortak ek + "simple shapes, few colors, flat muted earthy tones". Kılavuz yükseklikleri: çuval 30, sepet 22, yün sepeti 20, askı 70, raf 76, kurutma ipi 66, kilim 2, şişe 28. Duvar halısı kılavuzsuz temel mod (48x64, `side`). 9 çağrı = 9 generation (cost_max 24), 3'lü paralel. Hata yok, yeniden deneme yok.
`style.py fit --family esya` (yerel doygunluk düzeltmesi, PixelLab harcamaz): çuval x1,52→x1,15, raf x1,35→x1,14, yün sepeti x2,03→x1,14, sepet x1,29→x1,15, kilim x1,01→x1,15. Kilim 64x32'ye elmas maskesiyle kırpıldı (maske dışı 184 px silindi, içerik yeniden boyanmadı).
`reduce_colors` kilim için denendi: tarif tek parça base64 aktarımında kısaldı (araç "truncated" hatası verdi, ücret yok); yeniden denenmedi (tek deneme kuralı), kilim 50 renkle teslim edilmedi.
`get_balance` iş sonu: 9881 (paralel şeritler de harcıyor; bu işin payı 9).

## Sonuç özeti
| # | Eşya | Durum | Not |
|---|---|---|---|
| 1 | çuval | teslim (UYARI) | fit sonrası palete uzak (%90), doygunluk x1,15 |
| 2 | sepet | teslim | GEÇTİ (fit sonrası) |
| 3 | yün sepeti | TESLİM EDİLMEDİ | fit sonrası stil KALDI (%71 palet) |
| 4 | giysi askısı | teslim | GEÇTİ (ham) |
| 5 | kumaş rafı | teslim (UYARI) | fit sonrası palete uzak; %98 palet, x1,14 |
| 6 | kurutma ipi | TESLİM EDİLMEDİ | altında kahverengi zemin elması çizildi ("no ground" ihlali, 1664 opak px); iplerde kumaş çok küçük |
| 7 | kilim | TESLİM EDİLMEDİ | 50 renk (>48); fit sonrası da 50 |
| 8 | cam şişe | TESLİM EDİLMEDİ | 20x48 çıktı (hedef 32x32 tuval, şişe 32 yüksek) ve stil KALDI (x1,39 doygun, %56 palet); kırpılamaz |
| 9 | duvar halısı | teslim | GEÇTİ; düz ön görünüm, iso duvar düzlemine eğimli değil, `sw_ne` adı ayna için, kullanıcı bakmalı |

## Dosyalar (Assets/Art/Iso/_aday/)
| Yol | Piksel | Pivot x | Pivot y (alttan) |
|---|---|---|---|
| iso_item_grain_sack_se.png | 80x72 | 40 | 24 |
| iso_item_wicker_basket_se.png | 80x64 | 40 | 24 |
| iso_item_garment_stand_se.png | 80x96 | 40 | 24 |
| iso_item_cloth_bolt_shelf_2x1_se.png | 112x104 | 56 | 32 |
| iso_wall_item_tapestry_sw_ne.png | 48x64 | 24 | 32 |

## Doğrulama (validate_iso, `--max-colors 48`)
| Dosya | Opak sınır kutusu | Renk | Yarı saydam | Sonuç |
|---|---|---|---|---|
| çuval | x12-68, y10-63 | 36 | 0 | geçti |
| sepet | x11-68, y7-54 | 35 | 0 | geçti |
| askı | x10-69, y6-88 | 33 | 0 | geçti |
| raf | x33-89, y14-88 | 22 | 0 | geçti |
| halı (duvar) | x3-45, y3-60 | 20 | 0 | geçti |
| (red) yün sepeti | x8-71, y7-55 | 43 | 0 | geçti (stil KALDI) |
| (red) kurutma ipi | x9-88, y6-64 | 29 | 0 | geçti (zemin elması var) |
| (red) kilim | x1-62, y0-31 | 50 | 0 | BAŞARISIZ (renk) |
| (red) şişe | x30-49, y8-55 | 39 | 0 | geçti (stil KALDI, ölçek) |

Stil denetimi (esya): şu tabloya bakın, yan yana sayfasında da var.
| Dosya | Palete yakın | Doygunluk / aile | Sonuç |
|---|---|---|---|
| kumaş rafı | %98 | x1,14 | UYARI |
| askı | %99 | x0,94 | GEÇTİ |
| çuval | %90 | x1,15 | UYARI |
| sepet | %94 | x1,15 | GEÇTİ |
| duvar halısı | %100 | x1,09 | GEÇTİ |

Döşeme testi: yalnızca kilim zemin sınıfı, teslim edilmediği için yapılmadı. Gölge pişirilmemiş, ışık sol-üstten (görsel kontrol).

## Sorunlar / öneri
- Kurutma ipi ve şişe için kılavuzsuz temel mod ya da elmassız kılavuz denenebilir (yeni iş). Kılavuzun taban elması ince nesnelerde zemin olarak çiziliyor.
- Kilim: PixelLab `reduce_colors` büyük base64'ü iletemedi; `art/tmp/` altında yükleme (`upload:<id>`) gerekir, araç yoksa kod oturumu 36 renge indirsin.
- `art_link.py --apply` kod oturumunda çalıştırılacak (5 dosya için).
