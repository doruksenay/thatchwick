---
id: "421"
title: Tüccar malları (3 ikon) ve risaleler (4 kitap eşyası)
status: running
claimed_at: 2026-11-19T07:23Z
approved_by: user        # kullanıcı 19.11.2026: "hem eşyaları ekle"; otomatik akış (art/budget.md)
priority: 25
family: esya
style_gate: false
cost_est: 7
cost_max: 14
depends_on: ["399"]
max_attempts: 1
pixellab: true
outputs:
  - Assets/Art/Iso/_aday/icon_item_indigo.png
  - Assets/Art/Iso/_aday/icon_item_saffron.png
  - Assets/Art/Iso/_aday/icon_item_black_pepper.png
  - Assets/Art/Iso/_aday/iso_item_book_almanac_se.png
  - Assets/Art/Iso/_aday/iso_item_book_herbal_se.png
  - Assets/Art/Iso/_aday/iso_item_book_husbandry_se.png
  - Assets/Art/Iso/_aday/iso_item_book_smithing_se.png
created: 2026-11-19
---
# 421 Tüccar malları ve risaleler

## Amaç
Belge 34-D5 (gezgin tüccar): yalnızca tüccarda satılan çivit, safran, karabiber (pazar panelinde ikonla görünür) ve Kervan Odası kataloğunda pazar günü satılan dört risale (masa/raf üstü, "Oku" ile beceri XP'si).

## Yöntem
- İkonlar 401/417 ile aynı: `create_map_object` 32x32, `high top-down`, `low detail`, `basic shading`, `single color outline`; her açıklamanın sonuna `, simple still-life object, no face, no eyes, no creature, no person, plain background`.
- Risaleler 399 oyuncaklarıyla aynı yuva yöntemi (yuva M, masa/raf üstü, tuval 32x32, pivot taban elması). Dördü aynı biçimde: kapalı, deri kaplı kalın kitap; yalnızca kapak rengi ve kapaktaki küçük kabartma işaret farklı.

## Tarif
| # | Dosya (`_aday/`) | Açıklama |
|---|---|---|
| 1 | `icon_item_indigo.png` | `a small dark blue cake of indigo dye, rough block, with a little blue powder` |
| 2 | `icon_item_saffron.png` | `a small pinch of deep red-orange saffron threads in a tiny plain clay dish` |
| 3 | `icon_item_black_pepper.png` | `small heap of black peppercorns in a tiny cloth pouch` |
| 4 | `iso_item_book_almanac_se.png` | `closed thick medieval book, dark blue leather cover with a small embossed sun and moon, brass corners` |
| 5 | `iso_item_book_herbal_se.png` | `closed thick medieval book, green leather cover with a small embossed leaf, brass corners` |
| 6 | `iso_item_book_husbandry_se.png` | `closed thick medieval book, brown leather cover with a small embossed sheep, brass corners` |
| 7 | `iso_item_book_smithing_se.png` | `closed thick medieval book, dark red leather cover with a small embossed anvil, brass corners` |

## Doğrulama
`python tools/art/validate_iso.py <kitaplar>`, `python tools/art/style.py check --family esya <hepsi>`. Yüz/göz/figür içeren görsel teslim edilmez. Dönem: deri cilt, el yazması; basılı kitap görünümü yok.

## Notlar
- Eşya → dosya eşlemesi kod oturumunun işi (`IsoArt.Items`; ikonlar `icon_item_<id>` adıyla kendiliğinden bulunur).

<!-- İşçi bundan sonrasına dokunmaz; kullanıcı kararı:
status: review → accepted ya da rejected yap, rejected ise nedenini buraya yaz. -->
