unity-import: pending

# 491 Midye, inci, kırmız, çuha, kürk, gübre, çekirge, sülük ikonları (9)

- Araç: `create_image_pixflux` 32x32, `no_background=true`, tarifteki açıklamalar (483 ile aynı ayarlar, tohum verilmedi). 9 çağrı x 1 gen = 9 gen, hız sınırı hatası olmadı.
- `get_balance` sonrası: 2762,9 (paylaşılan havuz, diğer şeritler de harcıyor).
- Yan yana: `art/results/491-yanyana.png`. Masaüstü kopyası: yerel.
- Doygunluk düzeltmesi (`style.py fit --family hayvan`): kırmız boyası (x0.66), kırmız (x0.63), iskarlat çuha (x0.73). Ham hâller `art/results/491-red/`.

## Teslim (Assets/Art/Iso/_aday/), 32x32, pivot x ortada (16), pivot y alttan = alt boşluk
| Dosya | Pivot y (alttan) | validate | Stil denetimi |
|---|---|---|---|
| icon_mussel.png | 4 | geçti | UYARI (hayvan: palet dışı renkler; esya KALDI, soluk) |
| icon_pearl.png | 5 | geçti | GEÇTİ (esya, hayvan) |
| icon_kermes.png | 3 | geçti | UYARI (hayvan, fit sonrası: palete uzak; esya KALDI) |
| icon_dye_crimson.png | 5 | geçti | UYARI (hayvan, fit sonrası: palet dışı; esya KALDI) |
| icon_scarlet_cloth.png | 4 | geçti | UYARI (hayvan, fit sonrası: palet dışı; esya KALDI) |
| icon_rabbit_fur.png | 4 | geçti | UYARI (esya ve hayvan: soluk) |
| icon_pigeon_dung.png | 4 | geçti | GEÇTİ (esya, hayvan) |
| icon_locust.png | 3 | geçti | UYARI (esya: soluk); hayvan GEÇTİ |
| icon_leech.png | 1 | geçti | GEÇTİ (esya, hayvan) |

## Sorunlar
- Kırmızı tonlu üç görsel esya ailesiyle KALDI (palet dışı); tarifteki kural gereği hayvan ailesiyle yeniden denetlendi. Hayvan kilidi kurulmadı: palet uyumu yalnızca uyarı.
- Görsel kalite (nesnenin tanınması, örn. kırmız taneleri, sülük) elle incelenmedi; kullanıcı yan yana sayfasına baksın.
