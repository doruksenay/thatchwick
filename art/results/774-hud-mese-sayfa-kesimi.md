unity-import: pending

# 774 HUD meşe: sayfa kesimi

- Araç: PixelLab yok (`pixellab: false`); Python/Pillow ile bağlı bileşen kesimi (sayfa 1'de 5 bileşen, sayfa 2'de 1; 8-komşuluk, 1 px saydam pay). Hiçbir piksel boyanmadı ya da yeniden renklendirilmedi.
- Harcanan: 0 generation. Kalan (get_balance): 448,60.
- Yan yana (3×): `art/results/774-yanyana.png`; stil sayfası: `art/results/774-stil-yanyana.png`; 9-slice testi: `art/results/774-9slice-test.png`.
- Masaüstü kopyası: yerel.

## Dosyalar (`Assets/Art/Iso/_aday/ui_hud2_mese_*.png`)
UI parçaları; pivot ortada (boyutun yarısı).
| Dosya | Boyut | 9-slice sol | sağ | üst | alt | Not |
|---|---|---|---|---|---|---|
| clock | 77x73 | - | - | - | - | zincirler dahil |
| money | 65x45 | - | - | - | - | iki yuva |
| tracker | 69x25 | 21 | 22 | 0 | 0 | yalnız yatay gerilir |
| frame | 60x62 | 8 | 8 | 8 | 8 | köşe = perçin + çıta |
| bar | 154x19 | 8 | 8 | 0 | 0 | iç boşluk 7 px (y 6-12) |
| bar_fill_green/amber/red | 1x7 | - | - | - | - | üst 1 px açık ton |
| window | 225x172 | 32 | 32 | 32 | 32 | sarmaşık dahil; değer gözle ölçüldü |

Dolgu renkleri (taban / üst, palette_arayuz'dan en yakın): yeşil #6b983a / #72bf33; hardal #d1823d / #ebb15e; kırmızı #b81f2a / #b04f40.

## Doğrulama
`style.py check --family arayuz`: 9 dosyanın hepsi GEÇTİ (palete %100, uzaklık 0.0). `validate_iso.py` UI parçalarına uygulanmaz.

## Sorunlar
- 9-slice testinde bar ve tracker'da dikiş yok. `frame` ve `window` iç krem kenarındaki yırtık kâğıt çıkıntıları gerilince çoğalıp kalınlaşıyor; düzeltme denenmedi.
- Pencere köşe değeri (32) yaklaşık; sağda ~33 px.
- `tracker` yaprak uçları dikeyde taşar, üst/alt 0.
