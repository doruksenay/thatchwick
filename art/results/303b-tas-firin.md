unity-import: pending

# 303b Taş fırın yeniden (rapor)

Yan yana sayfası: [303b-yanyana.png](303b-yanyana.png) (yeni fırın + eski 303 fırını + stil referansları; 302 tezgâhı eklenmedi). Teslim edilmedi: `art/results/303b-red/iso_item_stone_oven_2x1_se_nw_v2.png`.
Masaüstü kopyası: yerel (bulutta yapılamaz).

## Araç ve ayarlar
`create_map_object`, stil eşleme, kılavuz + maske (`footprint_guide.py --w 112 --h 104 --n 2 --axis x --height 48`), `high top-down`, `low detail`, `basic shading`, `single color outline`, tarifteki açıklama + ortak ek. 1 çağrı = 1 generation (cost_max 3). `get_balance` iş başı 3737,5 (paralel şeritler de harcıyor; iş sonu ölçülmedi).

## Sonuç: TESLİM EDİLMEDİ (status: failed)
Görsel ekmek fırını olarak okunuyor (dikdörtgen taş gövde, kemerli ağız, kürek, kütükler; yuvarlak çömlekçi fırını değil). Ancak `validate_iso.py --max-colors 48`: renk sayısı 57 > 48, BAŞARISIZ. Renk fazlası ikinci deneme nedenleri listesinde yok ve `reduce_colors` yapılmaz (301 kuralı); 303'teki şömine/tezgâh örneği gibi teslim edilmedi.

| Dosya | Piksel | Opak kutu | Renk | Yarı saydam | Alta değme | Sonuç |
|---|---|---|---|---|---|---|
| iso_item_stone_oven_2x1_se_nw_v2 | 112x104 | x8-103, y7-96 | 57 | 0 | yok | BAŞARISIZ (renk) |

Stil denetimi (esya): palete yakın %99, ort. uzaklık 6.8, doygunluk x0.86, GEÇTİ.
Pivot (teslim olsaydı): x 56, y 32 (alttan).

## Öneri
Kullanıcı görseli yan yana sayfasında beğenirse renk sınırını bu eşya için gevşetmeye (ya da `reduce_colors` izni) karar verebilir; aksi halde ikinci deneme (`seed` 1) için cost_max içinde 2 gen payı var.
