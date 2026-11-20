unity-import: pending

# 480 İstek, süreli görev ve azık ikonları (16)

- Araç: `create_map_object` 32x32, `view: side`, `single color outline`, `basic shading`, `medium detail` (455a yöntemi), tarifin ortak eki ve palet metniyle. Tek deneme. 16 çağrı = 16 generation.
- Hız sınırı: ilk gönderimde 9 çağrı 429 aldı (başarısızlar iş açmadı); işler bitince bekleyip yeniden gönderildi.
- `get_balance` öncesi 2880,9, sonrası 2842,9 (ortak bakiye; paralel şeritlerin harcamasını da içerir).
- Son işlem: yarı saydam 0 (alfa eşiği 128), >16 renkli olanlar 16 renge indirildi (medyan kesim, titreşimsiz). Ham çıktılar `art/tmp480/` (commit edilmez).
- Yan yana sayfası: [480-yanyana.png](480-yanyana.png) (arayuz ailesinin referansı yok).
- Masaüstü kopyası: yerel.

## Dosyalar (hepsi 32x32; pivot alttan ortalı x=16, y=2), `Assets/Art/Iso/_aday/`
ui_icon_request_{favor, order, seasonal, call, story, vow, bequest, marker}, ui_icon_duty_{mine, fish, forest, forage, traps, clay, errand}, icon_item_miner_ration (.png)

## Doğrulama
Renk ≤16, yarı saydam 0 (hepsi). `validate_iso.py icon_item_miner_ration.png`: geçti. KALDI yok.

| Sonuç | Dosyalar |
|---|---|
| GEÇTİ (8) | miner_ration, duty_errand, duty_forest, duty_mine, request_call, request_favor, request_marker, request_seasonal |
| UYARI (8) | duty_clay, duty_fish, duty_forage, duty_traps, request_bequest, request_order, request_story, request_vow (palete uzak; aile kilidi yok) |

## Göz kontrolü (zayıf olanlar)
- **request_story**: yalnız küçük tüy kalem okunuyor, parşömen yok; en zayıf, yeniden üretim önerilir.
- **duty_errand**: ince değnek, bez bohça yok; zayıf.
- **duty_traps**: tahta kazık işaret direğine benziyor, ilmek görünmüyor; zayıf.
- **request_favor**: sepet yerine bez örtülü sandığa benziyor; kabul edilebilir.
- **duty_mine / duty_forest**: ikisi de demir uçlu saplı alet; karışma riski (balta ve kazma ayrışmalı), kullanıcı gözle baksın.
- **request_marker**: ünlem okunuyor, 16x16'ya küçültmede iki kat bakılmalı.
- order (rulo) ve bequest (mektup) ayrışıyor.

## Sorunlar
- Tek deneme kuralı gereği zayıf üç ikon (story, errand, traps) yeniden üretilmedi; ayrı iş (pixflux ikinci deneme) önerilir.
- Kabul sonrası `ui_icon_*` dosyaları `Assets/Resources/UI/Icons/` altına kopyalanır (kullanıcı/kod oturumu).
