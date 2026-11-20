unity-import: pending

# 432 Yerleşim: dış mekân, bahçe ve bitki örtüsü (rapor)

Sonuç: **11/13 teslim** (2 eşya teslim edilmedi). Yan yana: [432-yanyana.png](432-yanyana.png) (esya referansları solda, yeni 11 dosya sağda). Teslim edilmeyenler ve ham hâller: `art/results/432-red/`. Masaüstü kopyası: yerel.

## Araç ve ayarlar
`create_map_object`, stil eşleme, referanssız kılavuz + maske (`footprint_guide.py`; 1x1: 80x72 yükseklik 40, pivot 40/24; 2x1: 112x96 `--axis x` yükseklik 44, pivot 56/32). Dekor (4 bitki): 32x32, taban 12x6 elmas alt-ortada, elle üretilmiş kılavuz (`footprint_guide.py` 32 px'te bozuk çıktı verdi, Pillow ile `art/tmp432/`'de yazıldı). `high top-down`, `low detail`, `basic shading`, `single color outline`, tarif açıklaması + `, no face, no eyes, no creature, no person, no text` + 301 ortak eki.
13 çağrı x 1 gen = **13 generation** (cost_est 14, cost_max 23). İki çağrı (at bağlama direği, çömlek sergisi) hız sınırına takıldı (üretim başlamadı, ücret yok), ~40 sn sonra bir kez yeniden gönderildi. `get_balance` sonrası: **2416,90** (paralel şeritler de harcıyor).
Doygunluk düzeltmesi (`style.py fit`): esya ailesi için tabela ve çömlek sergisi (x0,79 ve x0,73); bitki ailesi için 4 bitki (x0,52-0,86, `bitki` kilidi kurulmadı, 424k/430 ile aynı yol). Ham hâller `432-red/raw_*`.

## Teslim edilenler (Assets/Art/Iso/_aday/)
| Dosya | Boyut | Pivot x / y (alttan) | Renk | Stil | validate_iso |
|---|---|---|---|---|---|
| iso_item_shop_sign_se.png | 80x72 | 40 / 24 | 13 | GEÇTİ (fit sonrası, ham UYARI x1,45) | geçti |
| iso_item_signpost_se.png | 80x72 | 40 / 24 | 27 | GEÇTİ | geçti |
| iso_item_notice_post_se.png | 80x72 | 40 / 24 | 27 | GEÇTİ | geçti |
| iso_item_clothes_line_2x1_se.png | 112x96 | 56 / 32 | 31 | GEÇTİ | geçti |
| iso_item_hitching_post_se.png | 80x72 | 40 / 24 | 41 | GEÇTİ | geçti |
| iso_item_pottery_display_se.png | 80x72 | 40 / 24 | 29 | GEÇTİ (fit sonrası, ham KALDI x1,59) | geçti |
| iso_item_smith_display_se.png | 80x72 | 40 / 24 | 26 | GEÇTİ | geçti |
| iso_item_flower_clump_se.png | 32x32 | 16 / 7 | 36 | UYARI (bitki, fit, palet %46) | geçti |
| iso_item_shrub_se.png | 32x32 | 16 / 7 | 15 | UYARI (bitki, fit, palet %61) | geçti |
| iso_item_reeds_se.png | 32x32 | 16 / 7 | 15 | UYARI (bitki, fit x1,26) | geçti |
| iso_item_grass_tuft_se.png | 32x32 | 16 / 7 | 15 | UYARI (bitki, fit, palet %70) | geçti |

Yarı saydam piksel 0, gölge pişirilmemiş. Zemin parçası olmadığından döşeme testi yok.

## Teslim EDİLMEYEN (`432-red/`)
| Dosya | Neden |
|---|---|
| iso_item_cloth_drying_line_2x1_se | esya stil denetimi KALDI (palete %77, uzaklık 11,3, palet dışı renkler; boyalı kırmızı/mavi/sarı kumaş doğal olarak aile paleti dışında). `fit` palet sorununu çözmez; doygunluk x1,00 zaten uygun. Tek deneme kuralı gereği tekrar yok |
| iso_item_net_drying_rack_2x1_se | `validate_iso` BAŞARISIZ: 57 renk > 48 (fit sonrası da 57; ham stil KALDI x1,50, fit sonrası GEÇTİ). `reduce_colors` tarifte yok, çağrılmadı |
Öneri: ikisi için ayrı iş (ağ rafı: renk indirme; boyalı kumaş: palet dışı renk kararı kullanıcıda).

## Sorunlar
- Çalı (shrub) çok küçük (12x12 px opak); saz ve çim öbeği tanınıyor. Ölçek kod tarafında ayarlanabilir; kullanıcı yan yana sayfasından karar versin.
- Çim öbeği yere yassı, ışık yönü ve gölge ölçülmedi (dekor, 32 px).
- Kılavuz taban elmasları bazı eşyalarda çıktıda görünür olabilir; kullanıcı kontrol etsin.
