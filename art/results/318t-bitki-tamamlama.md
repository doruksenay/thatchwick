unity-import: pending

# 318t Bitki tamamlama (rapor, 2026-11-20)

Sonuç: **5/9 teslim**. Teslim edilmeyen: carrot_3, forage_hazel, forage_berry_bush_v2, edge_trees_c (`art/results/318t-red/`). Yan yana sayfası: [318t-yanyana.png](318t-yanyana.png) (4 bitki referansı + 5 yeni). Masaüstü kopyası: yerel.

## Araç ve ayarlar
`create_map_object`, `high top-down`, `low detail`, `basic shading`, `single color outline`. 9 çağrı, her biri tek denemeydi.
- Ekinler (carrot_2/3, cabbage_2, lettuce_3, spinach_2): stil eşleme, `footprint_guide.py --w 80 --h 72 --n 1` kılavuzu + maskesi, yükseklik 22 (evre 2) ve 32 (evre 3). Tuval 80x72, pivot x=40, y(alttan)=24 (310/317/318 ile aynı).
- forage_hazel, forage_berry_bush_v2: stil eşleme, saydam 64x64 tuval, maske tuvalin tamamı (378 yöntemi).
- edge_trees_c (80x112) ve edge_undergrowth_2x1_a (128x64): **basic mod** (referanssız, saydam arka plan). 370'te stil eşleme zemin/blok çıkardığı için bu önerilmişti; tarifin "kaynak iş yöntemi" ifadesinden sapma budur.
- 3 çağrı (berry, trees_c, undergrowth) ilk gönderimde "rate limit exceeded" aldı (üretim başlamadı); ~30 sn sonra bir kez yeniden gönderildi.
- Doygunluk: ham çıktılar x0,36-0,81 çarpanıyla `style.py fit --family bitki` ile aileye indirildi (undergrowth x1,00). Bitki ailesinin kilidi kurulmadı, palet uyumu uyarıdır.
- **Maliyet:** 9 çağrı, ~13,5 generation (`cost_est` 12, `cost_max` 24). `get_balance` 2721,9 → 2691,9, ancak paralel şeritler de harcıyor; kesin ayrıştırılamadı. Sonrası bakiye: 2691,9.

## Dosyalar (Assets/Art/Iso/_aday/)
| Yol | Piksel | Pivot x | Pivot y (alttan) | Opak kutu (x, y alttan) | Renk | Not |
|---|---|---|---|---|---|---|
| iso_crop_carrot_2_se.png | 80x72 | 40 | 24 | x12-65, y15-61 | 35 | yoğun tüylü yeşil yapraklar, iyi |
| iso_crop_cabbage_2_se_v3.png | 80x72 | 40 | 24 | x13-65, y13-57 | 55 | açık yeşil gevşek rozet, yandan-üstten, yıldız yok. **Ad `_v3`**: `_v2` zaten vardı (310), üzerine yazılmadı. Renk >48 (yalnız rapor) |
| iso_crop_lettuce_3_se_v2.png | 80x72 | 40 | 24 | x8-71, y13-63 | 53 | kabarık kıvrık yapraklı baş, iyi. Renk >48 (yalnız rapor) |
| iso_crop_spinach_2_se_v2.png | 80x72 | 40 | 24 | x14-66, y10-61 | 50 | birkaç sap üstünde koyu yeşil yapraklar; hâlâ biraz yıldız/rozet okunur ⚠. Renk >48 (yalnız rapor) |
| iso_edge_undergrowth_2x1_a.png | 128x64 | 64 | 16 (2x1 için varsayım) | x6-123, y16-42 | 22 | zemin elması yok, ot/çalı şeridi; eğrelti ağırlıklı, böğürtlen/fidan belirgin değil ⚠ |

## Doğrulama
| Dosya | validate_iso (--max-colors 48) | style.py check (fit sonrası) |
|---|---|---|
| carrot_2 | geçti | UYARI (%36 palet, x1,15) |
| cabbage_2_v3 | renk 55 (yalnız rapor); saydam 0, alta değme yok | UYARI (%60, x1,16) |
| lettuce_3_v2 | renk 53 (yalnız rapor) | UYARI (%63, x1,15) |
| spinach_2_v2 | renk 50 (yalnız rapor) | UYARI (%53, x1,15) |
| undergrowth_2x1_a | geçti (22 renk) | UYARI (%38, x1,15) |

Hiçbiri KALDI almadı. Yarı saydam 0; alt boşluk 10-15 satır. Zemin değil, döşeme testi yok. Gölge pişirilmemiş, ışık sol-üstten (gözle). Ayrıntılı taban-hizası ölçümü (aynı bitkinin öteki evreleriyle) yapılmadı; pivot sabit.

## Teslim edilmeyenler (`318t-red/`)
- **carrot_3** (ham): kılavuz 32 px yükseklik verdiği hâlde çıktı tek renk yeşil bir ot tutamı; mor havuç omzu görünmüyor, carrot_2'den ayırt edilemiyor.
- **forage_hazel** (ham): ince, tek eğik gövdeli küçük bir ağaç + altında yeşil zemin parçası. Çok gövdeli çalı değil, fındık görünmüyor.
- **forage_berry_bush_v2** (ham): kırmızı-mor dal yığını; yaprak yok, böğürtlen okunmuyor, dikenli kemerli dal yok.
- **edge_trees_c** (ham): yalnız iğne yapraklı (çam) öbeği ve altında zemin elması; geniş yapraklı + iğne yapraklı karışık değil, zemin parçası var.

## Sorunlar
- Tek deneme kuralı (`max_attempts: 2`'ye rağmen çalışma başına tek deneme uygulandı); 4 parça için ikinci deneme önerilir: carrot_3 (açıklamada "purple" yerine "dark purple root top clearly visible" ve yükseklik maskesi), hazel/berry (64x64 yerine daha geniş tuval, zemin yasağı), trees_c (basic mod + "no ground tile, oak and pine mixed").
- Araçta seed yok. Basic modda saydam arka plan geldiği için undergrowth zemin bloğu sorunu çıkmadı; trees_c çıktı yine de zemin elması içeriyor.
- Evre sırası (cabbage 1<2<3, lettuce, spinach, carrot) boy olarak bakılmadı; yan yana sayfası kullanıcı kararı için hazır.
