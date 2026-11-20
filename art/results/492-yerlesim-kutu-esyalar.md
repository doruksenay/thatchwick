unity-import: pending

# 492 Yerleşim: kutu çizilen yer eşyaları (rapor)

Yan yana sayfası: [492-yanyana.png](492-yanyana.png) (teslim edilen 7 eşya + esya referansları). Teslim edilmeyenler: `art/results/492-red/` (7 dosya).
Masaüstü kopyası: yerel (bulutta yapılamaz).

## Araç ve ayarlar
`create_map_object`, stil eşleme, kılavuz + maske (`tools/art/footprint_guide.py`, `--axis x`; yükseklikler: su fıçısı 30, sini 20, bakır kazanlar 32, odun sepeti 20, turşu küpü 30, fıçı masa 40, şamdan 60, yalak 24, tezgâh 32, yatak 28, boyalı masa 28, kiler rafı 72, kurutma rafı 64, ip yumağı 10). `high top-down`, `low detail`, `basic shading`, `single color outline`, açıklama + "earthy muted medieval palette, desaturated" eki. 14 çağrı = 14 generation (cost_max 30 içinde), hepsi paralel. Fıçı masa ve kiler rafında ilk çağrı base64 aktarım hatasıyla doğrulamada reddedildi (üretim yapılmadı, ücret yok); kılavuz/maske yeniden kodlanıp bir kez gönderildi. Yeniden üretim yapılmadı. `get_balance` iş sonu: 2688,9 (paralel şeritler de harcıyor; bu işin payı 14).
Mum altlığı: 48 px tuvale 64 px'lik elmas kılavuz sığmadığı için 80x88 üretildi, ortadan 48x88'e kırpıldı (x 16-63; dışında opak piksel yok, içerik yeniden boyanmadı). Mum altlığı kılavuz yüksekliği 60.

## Sonuç özeti
| # | Eşya | Durum | Not |
|---|---|---|---|
| 1 | su fıçısı | teslim | GEÇTİ |
| 2 | sini masa | TESLİM EDİLMEDİ | 54 renk (>48), stil KALDI (x1,66 doygun) |
| 3 | bakır kazanlar | TESLİM EDİLMEDİ | 55 renk (>48); bordo-kırmızı çıktı |
| 4 | odun sepeti | teslim (UYARI) | doygun x1,34 |
| 5 | turşu küpü | teslim (UYARI) | palete uzak |
| 6 | fıçı masa | TESLİM EDİLMEDİ | 49 renk (>48); üstünde kupa yok, masa yerine kapaklı fıçı okunuyor |
| 7 | şamdan | teslim (UYARI) | soluk x0,52 |
| 8 | su yalağı | TESLİM EDİLMEDİ | stil KALDI (%79 palet, x1,59 doygun) |
| 9 | marangoz tezgâhı | teslim | GEÇTİ |
| 10 | yatak çerçevesi | TESLİM EDİLMEDİ | stil KALDI (x1,59 doygun) |
| 11 | boyalı masa | teslim (UYARI) | palete uzak; çiçek deseni belirsiz (üstte meyve/çiçek öbeği) |
| 12 | kiler rafı | teslim | GEÇTİ; solda raf dışında küçük fıçı var (ayak izi dışına taşıyor, bakılmalı) |
| 13 | kurutma rafı | TESLİM EDİLMEDİ | altında çim zemin ("no ground" ihlali), stil KALDI (%65 palet) |
| 14 | ip yumağı | TESLİM EDİLMEDİ | 49 renk (>48) |

Renk sınırı (48) aşan ve stil KALDI olanlarda `reduce_colors`/`fit` yapılmadı (esya ailesinin kilidi kurulu; doygunluk düzeltmesi yalnızca kilitsiz aile içindir; 301 kuralı). İkinci deneme koşulları (ayak izi, tanınma) tutmadığı için yapılmadı. Yatak ve yalak şekil olarak tanınıyor; kullanıcı gevşetmek isterse bu iki dosya `492-red/` içinde.

## Dosyalar (Assets/Art/Iso/_aday/)
| Yol | Piksel | Pivot x | Pivot y (alttan) |
|---|---|---|---|
| iso_item_water_barrel_se.png | 80x72 | 40 | 24 |
| iso_item_firewood_basket_se.png | 80x64 | 40 | 24 |
| iso_item_brine_crock_se.png | 80x72 | 40 | 24 |
| iso_item_candlestick_se.png | 48x88 | 24 | 24 |
| iso_item_workbench_2x1_se_nw.png | 112x88 | 56 | 32 |
| iso_item_table_painted_2x1_se_nw.png | 112x80 | 56 | 32 |
| iso_item_pantry_shelf_2x1_se_nw.png | 112x104 | 56 | 32 |

## Doğrulama (validate_iso, `--max-colors 48`)
| Dosya | Opak sınır kutusu | Renk | Yarı saydam | Sonuç |
|---|---|---|---|---|
| su fıçısı | x16-63, y7-62 | 35 | 0 | geçti |
| odun sepeti | x8-71, y7-55 | 41 | 0 | geçti |
| turşu küpü | x17-62, y11-62 | 34 | 0 | geçti |
| şamdan (kırpık) | x6-41, y5-80 | 25 | 0 | geçti |
| tezgâh | x17-97, y9-79 | 45 | 0 | geçti |
| boyalı masa | x15-100, y4-72 | 25 | 0 | geçti |
| kiler rafı | x13-99, y8-96 | 37 | 0 | geçti |
| (red) sini masa | x15-65, y8-56 | 54 | 0 | BAŞARISIZ (renk) |
| (red) bakır kazanlar | x20-55, y13-58 | 55 | 0 | BAŞARISIZ (renk) |
| (red) fıçı masa | x17-62, y9-70 | 49 | 0 | BAŞARISIZ (renk) |
| (red) ip yumağı | x10-69, y10-46 | 49 | 0 | BAŞARISIZ (renk) |
| (red) yatak | x9-101, y5-79 | 34 | 0 | geçti (stil KALDI) |
| (red) yalak | x11-96, y10-72 | 27 | 0 | geçti (stil KALDI) |
| (red) kurutma rafı | x12-98, y12-88 | 39 | 0 | geçti (stil KALDI) |

## Stil denetimi (`style.py check --family esya`)
| Dosya | Palete yakın | Ort. uzaklık | Doygunluk | Sonuç |
|---|---|---|---|---|
| su fıçısı | %99 | 6.4 | x0.96 | GEÇTİ |
| odun sepeti | %100 | 6.3 | x1.34 | UYARI |
| turşu küpü | %96 | 13.5 | x0.90 | UYARI |
| şamdan | %99 | 5.1 | x0.52 | UYARI |
| tezgâh | %99 | 4.6 | x1.10 | GEÇTİ |
| boyalı masa | %98 | 10.6 | x0.97 | UYARI |
| kiler rafı | %99 | 5.6 | x0.97 | GEÇTİ |
| (red) yatak | %100 | 12.2 | x1.59 | KALDI |
| (red) kurutma rafı | %65 | 17.5 | x1.10 | KALDI |
| (red) yalak | %79 | 15.7 | x1.59 | KALDI |
| (red) sini masa | %87 | 11.8 | x1.66 | KALDI |
| (red) fıçı masa | %84 | 10.8 | x0.97 | UYARI |
| (red) bakır kazanlar | %85 | 11.2 | x1.38 | UYARI |
| (red) ip yumağı | %100 | 10.3 | x1.21 | UYARI |

## Yapılmayanlar / sorunlar
- Ölçülmedi: ışık sırası, ayna uygunluğu, ayak izi kaplaması sayısal değeri; zemin döşeme testi yok (zemin değil). Gölge pişirilmemiş görünüyor (gözle).
- Teslim edilmeyen 7 eşya için öneri: ayrı bir iş (renk sınırı gevşetmesi ya da `reduce_colors` izni; yatak ve yalak yeniden üretimi). `tools/art` içinde base64 aktarımı için yükleme aracı olsaydı büyük kılavuzlar hatasız geçerdi (öneri).
- `items.json` `pending` alanını kod oturumu kaldırır.
