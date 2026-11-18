unity-import: pending

# 385 Tavan ve kiriş eşyaları (rapor): 4/6 teslim, review

Yan yana sayfası: [385-yanyana.png](385-yanyana.png) (teslim edilen 4 eşya + 7 referans). Teslim edilmeyenler: `art/results/385-red/`. Masaüstü kopyası: yerel.

## Araç ve ayarlar
- `create_map_object`, stil eşleme, `side` görünüm, `low detail`, `basic shading`, `single color outline`; tarifteki açıklama + ortak ek + takım eki. 6 çağrı x 1 gen = 6 generation (cost_max 12). Tek deneme.
- Kılavuz ve maske (`art/tmp385/`, commit edilmez; Python ile): saydam tuval, üst kenarda 6x3 px asılma noktası (#786046), maske asılma noktasının altındaki eşya alanı. Tuval en az 32 px olduğu için kanca 32x32, fener 32x48 (tarif 16x24 / 24x40 idi).
- Hız sınırı: avize (3) ve püskül (6) ilk gönderimde "rate limit exceeded" aldı (harcama yok), ~40 sn sonra bir kez yeniden gönderildi.
- Işıklı eşyalar (fener, avize) `style.py check --light` ile denetlendi.
- Fener: ham hâli "ailesinden çok doygun" (x1.51) KALDI, şekil tanınıyor; `style.py fit` ile doygunluk x0.72 indirildi, GEÇTİ. Ham hâli `385-red/`de.
- Teslim edilen dosyalarda yalnızca üstteki boş satırlar kırpıldı (asılma noktası üst kenar olsun diye), içerik değişmedi.
- get_balance sonrası: 4770,9 / 5000 (paralel şeritler de harcıyor; bu işin payı 6).

## Sonuç özeti
| # | Eşya | Durum | Not |
|---|---|---|---|
| 1 | kiriş kancası | teslim | UYARI: ailesinden soluk (x0.39), ince |
| 2 | asma fener | teslim (fit x0.72) | GEÇTİ |
| 3 | demir avize | teslim | GEÇTİ (x0.65) |
| 4 | ot demetleri | teslim | UYARI: ailesinden soluk (x0.45) |
| 5 | çelenk | TESLİM EDİLMEDİ | KALDI: palet %67, ort. uzaklık 15.1 (renkli çiçekler palet dışı); `fit` doygunluk düzeltir, palet sorununu çözmez |
| 6 | püskül | TESLİM EDİLMEDİ | KALDI: palet %71, ort. uzaklık 16.5 (canlı boyalı yün) |

## Dosyalar (Assets/Art/Iso/_aday/; pivot üst-orta, ALTTAN y = yükseklik)
| Yol | Piksel | Pivot x | Pivot y (alttan) |
|---|---|---|---|
| iso_ceiling_item_beam_hook.png | 32x29 | 16 | 29 (üst kenar) |
| iso_ceiling_item_hanging_lantern.png | 32x45 | 16 | 45 (üst kenar) |
| iso_ceiling_item_iron_chandelier.png | 64x37 | 32 | 37 (üst kenar) |
| iso_ceiling_item_ceiling_herbs.png | 40x28 | 20 | 28 (üst kenar) |

## Doğrulama
| Dosya | Renk | Yarı saydam | Palete yakın | Ort. uzaklık | Doygunluk/aile | validate_iso | Stil |
|---|---|---|---|---|---|---|---|
| beam_hook | 9 | 0 | %100 | 8.6 | x0.39 | geçti | UYARI soluk |
| hanging_lantern (fit) | 9 | 0 | %100 | 3.3 | x1.05 | geçti | GEÇTİ (`--light`, ışık %13) |
| iron_chandelier | 41 | 0 | %99 | 6.7 | x0.65 | geçti | GEÇTİ (`--light`, ışık %12) |
| ceiling_herbs | 18 | 0 | %100 | 7.1 | x0.45 | geçti | UYARI soluk |
| ceiling_garland (ham) | 39 | 0 | %67 | 15.1 | x0.94 | geçti | KALDI palet |
| ceiling_tassels (ham) | 25 | 0 | %71 | 16.5 | x1.18 | geçti | KALDI palet |

Döşeme testi tavan eşyasına uygulanmaz. Ayak izi yok (asılı eşya, ayak izi kutusu verilmedi).

## Sorunlar / öneri
- Çelenk ve püskül paleti aşıyor: ikinci deneme kuralının maddelerine girmiyor, tek deneme olduğu için teslim edilmedi. Yeniden denemek için tarife "muted, desaturated earth tones" vurgusu eklenebilir (kullanıcı kararı).
- Fener ve avizenin ışık pikseli denetim dışı; alev rengi kullanıcı göz kararına bırakıldı.
- `tools/art`'ta tavan/asılı eşya kılavuz betiği yok; `art/tmp385/` altındaki betik eşdeğeriydi. Kalıcı olması için kod oturumu eklemeli.
- Eşya → dosya eşlemesi ve pivotlar kod oturumu/kullanıcının işi (pivot üst-orta).
