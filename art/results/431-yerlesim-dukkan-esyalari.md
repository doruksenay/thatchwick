unity-import: pending

# 431 Yerleşim: dükkân ve ev eşyaları (rapor)

Yan yana sayfası: [431-yanyana.png](431-yanyana.png) (yalnızca teslim edilen 12). Teslim edilmeyenler: `art/results/431-red/` (18 dosya). Masaüstü kopyası: yerel.

## Araç ve ayarlar
`create_map_object`, stil eşleme, `footprint_guide.py` kılavuzu + maske (1x1: 80x80 `--height 48`, pivot 40,24; 2x1: 112x112 `--n 2 --axis x --height 48`, pivot 56,32), `high top-down`, `low detail`, `basic shading`, `single color outline`. Açıklama: tarif + `no face, no eyes, no creature, no person, no text, no ground shadow, light from upper left`. Dekorlar (4) kılavuzsuz, temel mod 32x32. Kılavuz/maske aileye göre `esya` referansı eklenmeden gönderildi (301 yöntemi).
Her çağrı 1 gen, 30 çağrı = 30 generation (cost_max 47 içinde). Hız sınırı (rate limit) 2 çağrıda çıktı (scroll_rack, clay_trough; ücret yok), 15-30 sn sonra bir kez yeniden gönderildi. `get_balance` iş sonu: 2401,9 (paralel şeritler de harcıyor).

## Sonuç özeti (12 teslim, 18 teslim edilmedi)
| Eşya | Durum | Not |
|---|---|---|
| shop_counter_2x1 | teslim | GEÇTİ |
| anvil | teslim | GEÇTİ |
| bellows | teslim (UYARI) | ailesinden doygun (x1.30) |
| quench_tub | teslim | GEÇTİ |
| iron_pile | teslim (UYARI) | palete uzak |
| scroll_rack | teslim | GEÇTİ |
| scroll_chest | teslim (UYARI) | palete uzak |
| writing_lectern | teslim | GEÇTİ |
| pot_drying_rack_2x1 | teslim | GEÇTİ |
| clay_trough | teslim | GEÇTİ |
| fish_basket | teslim (UYARI) | palete uzak, soluk (x0.53) |
| shears_chalk (dekor) | teslim (UYARI) | palete uzak, soluk |
| cutting_table_2x1 | TESLİM EDİLMEDİ | stil UYARI ama validate_iso BAŞARISIZ: 56 renk > 48 |
| wash_tub | TESLİM EDİLMEDİ | stil KALDI (yalnız çok doygun); `fit` ile stil GEÇTİ (x1.23) ama 52 renk > 48 ve alt boşluk 22 satır (taban tutmadı) |
| bedside_stand, bread_rack, cloth_bolt_shelf_2x1, flour_sack, garment_stand, grain_sack, hide_rack, ink_quill, kneading_trough_2x1, letter_box, lumber_pile_2x1, stone_blocks, unfinished_furniture, wax_seal, wicker_basket, yarn_basket | TESLİM EDİLMEDİ | stil KALDI (palet dışı renkler / ağır doygunluk); palet sorunu `fit` ile çözülmediği için düzeltme denenmedi (yalnız salt doygunluk KALDI'sı olan wash_tub denendi) |

## Dosyalar (Assets/Art/Iso/_aday/)
| Yol | Piksel | Pivot x | Pivot y (alttan) |
|---|---|---|---|
| iso_item_shop_counter_2x1_se.png | 112x112 | 56 | 32 |
| iso_item_pot_drying_rack_2x1_se.png | 112x112 | 56 | 32 |
| iso_item_anvil_se.png | 80x80 | 40 | 24 |
| iso_item_bellows_se.png | 80x80 | 40 | 24 |
| iso_item_quench_tub_se.png | 80x80 | 40 | 24 |
| iso_item_iron_pile_se.png | 80x80 | 40 | 24 |
| iso_item_scroll_rack_se.png | 80x80 | 40 | 24 |
| iso_item_scroll_chest_se.png | 80x80 | 40 | 24 |
| iso_item_writing_lectern_se.png | 80x80 | 40 | 24 |
| iso_item_clay_trough_se.png | 80x80 | 40 | 24 |
| iso_item_fish_basket_se.png | 80x80 | 40 | 24 |
| iso_item_shears_chalk_se.png | 32x32 | 16 | 7 |

## Doğrulama (validate_iso --kind object --max-colors 48)
| Dosya | Opak sınır kutusu | Renk | En alt opak satır | Sonuç |
|---|---|---|---|---|
| anvil | x15-64, y13-65 | 41 | 65 | geçti |
| bellows | x9-70, y13-72 | 40 | 72 | geçti |
| clay_trough | x8-71, y26-72 | 45 | 72 | geçti |
| fish_basket | x10-69, y16-68 | 38 | 68 | geçti |
| iron_pile | x11-68, y16-69 | 37 | 69 | geçti |
| pot_drying_rack_2x1 | x9-102, y7-104 | 42 | 104 | geçti |
| quench_tub | x14-65, y9-70 | 13 | 70 | geçti |
| scroll_chest | x8-71, y8-72 | 29 | 72 | geçti |
| scroll_rack | x17-62, y10-72 | 40 | 72 | geçti |
| shears_chalk | x4-27, y6-26 | 21 | 26 | geçti |
| shop_counter_2x1 | x8-103, y7-104 | 19 | 104 | geçti |
| writing_lectern | x19-57, y13-72 | 45 | 72 | geçti |
| cutting_table_2x1 (red) | x8-103, y7-103 | 56 | 103 | BAŞARISIZ |
| wash_tub fit (red) | x21-58, y24-57 | 52 | 57 | BAŞARISIZ |

Çıktılar tam 80x80 / 112x112 / 32x32 tuvalde; kırpma yapılmadı. Ayak izi kaplama oranı, döşeme/dikiş testi (zemin işi değil), ışık yönü ve ayna ölçümü yapılmadı; görsel kontrol yan yana sayfasında kullanıcıda.

## Stil denetimi (`style.py check --family esya`)
| Dosya | Palete yakın | Ort. uzaklık | Doygunluk | Sonuç |
|---|---|---|---|---|
| anvil | %100 | 5.8 | x0.72 | GEÇTİ |
| bellows | %97 | 8.7 | x1.30 | UYARI |
| clay_trough | %100 | 8.6 | x0.62 | GEÇTİ |
| fish_basket | %89 | 10.1 | x0.53 | UYARI |
| iron_pile | %82 | 12.6 | x0.80 | UYARI |
| pot_drying_rack_2x1 | %86 | 7.3 | x1.18 | GEÇTİ |
| quench_tub | %100 | 9.9 | x1.14 | GEÇTİ |
| scroll_chest | %80 | 11.7 | x1.00 | UYARI |
| scroll_rack | %92 | 8.3 | x1.24 | GEÇTİ |
| shears_chalk | %84 | 12.0 | x0.55 | UYARI |
| shop_counter_2x1 | %99 | 4.3 | x0.71 | GEÇTİ |
| writing_lectern | %97 | 7.3 | x1.12 | GEÇTİ |
| cutting_table_2x1 (red) | %85 | 12.8 | x1.39 | UYARI |
| wash_tub (red, ham) | %92 | 9.8 | x1.54 | KALDI |
| bedside_stand (red) | %53 | 17.6 | x1.75 | KALDI |
| bread_rack (red) | %71 | 15.7 | x1.40 | KALDI |
| cloth_bolt_shelf_2x1 (red) | %77 | 16.7 | x1.26 | KALDI |
| flour_sack (red) | %76 | 12.2 | x0.95 | KALDI |
| garment_stand (red) | %75 | 13.6 | x1.75 | KALDI |
| grain_sack (red) | %79 | 13.7 | x1.45 | KALDI |
| hide_rack (red) | %62 | 15.5 | x1.44 | KALDI |
| ink_quill (red) | %40 | 20.9 | x1.56 | KALDI |
| kneading_trough_2x1 (red) | %97 | 10.5 | x1.55 | KALDI |
| letter_box (red) | %78 | 12.8 | x0.77 | KALDI |
| lumber_pile_2x1 (red) | %76 | 13.6 | x1.73 | KALDI |
| stone_blocks (red) | %72 | 15.9 | x0.60 | KALDI |
| unfinished_furniture (red) | %68 | 18.4 | x0.87 | KALDI |
| wax_seal (red) | %25 | 38.6 | x1.76 | KALDI |
| wicker_basket (red) | %49 | 22.3 | x1.56 | KALDI |
| yarn_basket (red) | %49 | 27.2 | x1.57 | KALDI |

## Sorunlar
- 18 eşya teslim edilmedi (yukarıda). Max 1 deneme kuralı gereği yeniden üretilmedi; yeni iş (431b) olarak, açıklamaları sadeleştirilmiş (renk/malzeme adı, "natural brown wood") ve gerekirse referans görselle yeniden gönderilebilir. Özellikle dekorlar (mürekkep, mühür, mektup kutusu) 32x32 temel modda palete çok uzak; stil eşlemeli bir yüzey karosu denemesi önerilir.
- Cutting table 56 renkli: `reduce_colors` ya da `fit` + `correct` ile kurtarılabilir (PixelLab `reduce_colors` kullanılmadı, tarif dışı).
- Ham dosyalar `art/tmp431/` altında (commit dışı).
