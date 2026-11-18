unity-import: pending

# 021e Portre ağız ve kulak: sonuç

Durum: kısmi. 5/9 dosya teslim edildi; 4 ağız stil denetiminde KALDI (teslim edilmedi). Masaüstü kopyası: yerel (bulut işçisi, atlandı).

## Araç ve ayarlar
`inpaint_image_pro_flash`, `Modify current layer, only changes`, 64x64, 5 gen/çağrı. Taban `portrait_base_oval_adult_64.png` (`get_upload_url` ile yüklendi; `source_image_id` yerine `image_url=upload:...`). Maskeler betikle, saf siyah/beyaz: ağız (24,31)-(40,38) 16x7; kulaklar (15,21)-(23,34) ve (41,21)-(49,34). Açıklama tarifteki gibi (+ ortak ek).
Dokuzuncu çağrı (ear_large) 8 eşzamanlı iş sınırına takıldı (faturalanmadı); bir iş bitince bir kez yeniden gönderildi.

## Harcama
9 çağrı x 5 = 45 gen (cost_max 45). `get_balance` sonrası: 1634,2 (paralel şeritler dahil). Defter: `art/ledger/021e.md`.

## Dosyalar (portre, pivot yok; 64x64)
| Yol (Assets/Art/Iso/_aday/) | Renk | Stil | Not |
|---|---|---|---|
| portrait_layer_ear_small_64.png | 13 | UYARI (palete uzak, x1.46) | |
| portrait_layer_ear_medium_64.png | 14 | UYARI (palete uzak, x0.92) | gri tonlu, kulak rengi tenden ayrık |
| portrait_layer_ear_large_64.png | 17 | GEÇTİ x1.07 | sivri (elf benzeri) çıktı |
| portrait_layer_mouth_neutral_full_64.png | 13 | UYARI x1.37 | |
| portrait_layer_mouth_wide_64.png | 16 | UYARI x1.30 | açık ağıza yakın |

Teslim edilmeyen (`art/results/021e-red/`): mouth_neutral_thin (x1.58), mouth_small (x1.57), mouth_downturned (x1.53), mouth_slight_smile (x1.55): hepsi KALDI (palete uzak, çok doygun). Yeniden üretim gerekir.

## Stil denetimi
Tablo yukarıdaki sütunlarda; KALDI x4 yukarıda. Yan yana: `art/results/021e-yanyana.png`. Taban birleşimi x3 önizleme (9 parça): `art/results/021e-onizleme-x4.png`.

## Doğrulama
- Hepsi 64x64, yarı saydam piksel 0, renk ≤ 17 (≤ 32).
- Maske dışı piksel: 0 (9/9).
- Birleşim: kayma ve çift dış hat yok. Ağız katmanları maske dikdörtgenini (112 px) tamamen dolduruyor (ten dahil), yani şeffaf boşluk bırakmıyor.

## Sorunlar
1. Ağız katmanlarında "lips grey" isteği doygunluğu düşürmedi; 4/6 KALDI. Öneri: ağız için kontrastlı tek ton ya da daha dar maske ile yeniden üretim.
2. ear_large sivri kulak (elf) üretti; dönem/ırk tutarlılığı için "round human ears" ile yeniden denenebilir. ear_medium belirgin gri.
3. Ağız maskesi 16x7 bütün dikdörtgeni dolduruyor; birleşimde tenle uyumsuz kenar olabilir (gözle görülür bir şerit yok).
