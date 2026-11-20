unity-import: pending

# 724 Domates ekini (rapor)

Sonuç: **1/3 teslim** (evre 3, olgun). Evre 1 ve 2 iki denemede de tutmadı, teslim edilmedi (`art/results/724-red/`). Yan yana sayfası: [724-yanyana.png](724-yanyana.png). Masaüstü kopyası: yerel.

## Araç ve ayarlar
`create_map_object`, stil eşleme, referanssız 1x1 kılavuz + maske (`footprint_guide.py --w 80 --h 72`, maske yüksekliği 10/22/32; tuval 80x72, pivot 40,24), `high top-down`, `low detail`, `basic shading`, `single color outline`, 374'ün açıklama eki. Evre 3 için de maske yüksekliği 32 kullanıldı (tarifteki 40 değil). `style.py fit --family bitki` uygulandı (x0,39).
Çağrılar: 3 ilk deneme + 2 ikinci deneme (evre 1 ve 2, "floating alone, no soil, no dirt, no grass" eklendi) = **5 çağrı** (cost_max 6). Hız sınırı hatası yok. `get_balance` sonrası: 2010,7 (paralel şeritler de harcıyor, bu işin payı ayrıştırılamadı).

## Teslim edilen
| Dosya | Boyut | Pivot x / y (alttan) | Opak sınır | Renk | Doygunluk | Stil denetimi | validate_iso |
|---|---|---|---|---|---|---|---|
| iso_crop_tomato_3_se.png | 80x72 | 40 / 24 | x28-54, y8-50 (43 px yüksek) | 41 | x1,14 | GEÇTİ (palete yakın %100) | geçti |

Yarı saydam piksel 0, zemin/tepsi yok, gölge pişirilmemiş. Kırmızı meyveli, yaprak dallı bitki; çubuk belirgin değil.

## Teslim edilmeyenler (`724-red/`)
- Evre 1 (filiz): 1. deneme toprak karosu üzerinde filiz (biçim doğru, tepsi var); 2. deneme toprak yok ama 4 kırmızı domates kümesi çıktı, filiz değil.
- Evre 2 (gelişmiş bitki): 1. deneme toprak karosu üzerinde çiçekli bitki (bitki iyi, tepsi var); 2. deneme çim karosu üzerinde turuncu meyveli ağaç.
Karoyu maskeyle silmek bitkinin alt yapraklarını da keseceği için yapılmadı.

## Öneri
Evre 1 ve 2 için ayrı iş: başka ek metin ("single stalk on bare tile") ya da `pixellab: false` ile ilk denemelerdeki bitkinin elle temizlenmesi. O zamana kadar kod evre 1 ve 2 için renkli şekli kullanır.
