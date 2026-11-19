unity-import: pending

# 440 Gömme ve ev eşyaları (rapor): 5/5 teslim, review

Yan yana sayfası: [440-yanyana.png](440-yanyana.png). Masaüstü kopyası: yerel.

## Araç ve ayarlar
- `create_map_object`, stil eşleme (özel maske), `low detail`, `basic shading`, `single color outline`; tarifteki açıklama metni aynen. 5 çağrı paralel, çağrı başına 1 gen = 5 generation (cost_max 16). Tek deneme, hız sınırı yok.
- Görünüm: duvar ve tavan eşyaları `side`, nakış kasnağı `high top-down` (302 ile aynı).
- Kılavuz ve maske (`art/tmp440/`, commit edilmez; Python ile): duvar eşyaları 306'daki gibi duvar renginde (184,160,122) "/" eğimli paralelkenar (niş 48x64, lamba nişi 48x64, pencere oturağı 64x48); tavan kirişi 64x40 saydam tuval + üst kenarda 6x3 asılma noktası; kasnak `footprint_guide.py` ile 80x72, 1x1 ayak izi. Referans verilmedi.
- Teslimde piksel içeriği değiştirilmedi.
- get_balance sonrası: 3450,9 / 5000 (paralel şeritler de harcıyor; bu işin payı 5).

## Dosyalar (Assets/Art/Iso/_aday/; pivot = görsel merkezi, ALTTAN; tavan eşyasında üst-orta)
| Yol | Piksel | Pivot x | Pivot y (alttan) |
|---|---|---|---|
| iso_wall_item_wall_niche_sw_ne.png | 48x64 | 24 | 32 |
| iso_wall_item_wall_niche_lamp_sw_ne.png | 48x64 | 24 | 32 |
| iso_wall_item_window_seat_sw_ne.png | 64x48 | 32 | 24 |
| iso_ceiling_item_painted_beam_se.png | 64x40 | 32 | 40 (üst kenar) |
| iso_item_embroidery_frame_se.png | 80x72 | 40 | 24 |

## Doğrulama (validate_iso, `--max-colors 48`; hepsi geçti)
| Dosya | Opak kutu | Renk | Yarı saydam | Stil denetimi |
|---|---|---|---|---|
| wall_niche | x12-39 y20-57 | 11 | 0 | UYARI (palete %100 ama ort. uzaklık 10.7, ailesinden soluk x0.46) |
| wall_niche_lamp | x12-39 y17-60 | 20 | 0 | UYARI (ort. uzaklık 10.5, soluk x0.25) |
| window_seat | x9-53 y10-38 | 14 | 0 | GEÇTİ (%100, x1.02) |
| painted_beam | x4-59 y3-36 | 23 | 0 | UYARI (palete %85, ort. uzaklık 11.8, x0.65) |
| embroidery_frame | x20-59 y9-56 | 25 | 0 | GEÇTİ (%98, x0.91) |

KALDI yok. Döşeme testi bu iş için geçerli değil. Gölge pişirilmemiş.

## Sorunlar / gözlem
- Kiriş: ince, eğimli bir çubuk; "kırmızı-mavi-koyu sarı bantlar" yalnızca kısmen okunuyor (mavi ve ocre ucu belirgin, kırmızı zayıf). Göz kararı gerekli.
- Nişler ailesinden soluk (gri taş tonu); lamba nişinde alev yok denecek kadar küçük (sıcak ışık noktası var). Pencere oturağı düz ahşap, tarife uygun.
- Kasnak üstünde pembe bir dikiş şeridi var (yarım nakış), makul.
- Pivotlar kullanıcı işi (`Assets/Editor/IsoPivots.txt`).
