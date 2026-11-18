unity-import: pending

# 353 Su işleri eşyaları (rapor)

Yan yana sayfası: [353-yanyana.png](353-yanyana.png). Teslim edilmeyen ve ham denemeler: `art/results/353-red/`.
Masaüstü kopyası: yerel (bulutta yapılamaz).

## Araç ve ayarlar
`create_map_object`, stil eşleme, referanssız kılavuz + maske (`tools/art/footprint_guide.py`, `--axis x`), `high top-down`, `low detail`, `basic shading`, `single color outline`, 301 ortak eki (ahşap eşyalarda `dark brown wood`). Kılavuz tuvalleri: çeşme 112x96 (2x1, h40), yalak 80x64 (h24), hela 80x88 (h40), sarnıç 144x112 (2x2, h24), fıçı 80x72 (h36), terazi 80x136 (h90).

Çağrılar (her biri 1 gen): 6 ilk deneme + 2 ikinci deneme (yalak %45 ve sarnıç %57 ile ayak izini doldurmadı) = 8 generation (cost_max 16 içinde). Terazinin ilk gönderimi base64 aktarım hatasıyla üretime girmeden reddedildi (ücret yok), kılavuz yeniden kodlanıp (palet PNG) gönderildi. `get_balance` iş sonu: 4978 (paralel şeritler de harcıyor; bu işin payı 8).

## Sonuç özeti
| # | Eşya | Durum | Not |
|---|---|---|---|
| 1 | çeşme (2x1) | teslim | ayak izi %93; taş duvar + bronz lüle + yalak, su görünür |
| 2 | taş yalak ve lüle | teslim (2. deneme, UYARI) | ayak izi %85; doygun x1.31 |
| 3 | akarsulu hela | teslim (UYARI soluk) | ayak izi %100; kapaklı ahşap kutu, arkadaki oluk belirgin değil (⚠ tanınırlık) |
| 4 | sarnıç (2x2) | TESLİM EDİLMEDİ | iki denemede ayak izi %57 / %67 (<%80); tarif kuralı |
| 5 | yağmur fıçısı | teslim (doygunluk x0,71 `fit`) | ham hâl doygunluk KALDI (x1.63); ayak izi %88; tarifteki oluk/lüle çıkmadı, yalnızca fıçı (⚠) |
| 6 | su terazisi | teslim | ayak izi %98, 1x1 tabanı aşmıyor, yükseklik ~114 px (~2 kat); tabanda küçük çimen lekesi var ("no ground" ⚠) |

## Dosyalar (Assets/Art/Iso/_aday/)
| Yol | Piksel | Pivot x | Pivot y (alttan) |
|---|---|---|---|
| iso_item_fountain_2x1_se.png | 112x96 | 56 | 32 |
| iso_item_kitchen_spout_se.png | 80x64 | 40 | 24 |
| iso_item_flush_privy_se.png | 80x88 | 40 | 24 |
| iso_item_rain_barrel_se.png | 80x72 | 40 | 24 |
| iso_item_water_tower_se.png | 80x136 | 40 | 24 |

## Doğrulama (validate_iso, `--max-colors 48`)
| Dosya | Opak sınır kutusu | Renk | Yarı saydam | Alta değme | Sonuç |
|---|---|---|---|---|---|
| çeşme | x14-103, y3-88 | 39 | 0 | yok (7) | geçti |
| yalak | x18-60, y17-56 | 31 | 0 | yok (7) | geçti |
| hela | x8-71, y7-80 | 27 | 0 | yok (7) | geçti |
| fıçı | x16-63, y6-62 | 44 | 0 | yok (9) | geçti |
| terazi | x9-70, y11-124 | 33 | 0 | yok (11) | geçti |

Maske dışı opak piksel hepsinde 0. Döşeme testi eşya için gerekmez. Tam 2:1 olmayan çıktı yok, kırpma yapılmadı.

## Stil denetimi (`style.py check --family esya`)
| Dosya | Palete yakın | Ort. uzaklık | Doygunluk | Sonuç |
|---|---|---|---|---|
| çeşme | %97 | 8.8 | x0.67 | GEÇTİ |
| yalak | %100 | 7.6 | x1.31 | UYARI (doygun) |
| hela | %100 | 6.3 | x0.56 | UYARI (soluk) |
| fıçı (fit) | %100 | 7.4 | x1.10 | GEÇTİ |
| terazi | %93 | 9.1 | x0.71 | GEÇTİ |
| sarnıç (teslim edilmedi) | %100 | 5.3 | x0.92 | GEÇTİ |

## Sorunlar
- Sarnıç (2x2): iki denemede de ayak izi yetersiz (yaklaşık 1x1 boyutunda çizdi); ayrı iş ya da farklı yöntem gerekir (353b önerisi: daha büyük tuval, "low wide stone well-like cistern" ifadesi).
- Fıçıda gutter lülesi ve helada oluk net değil; kullanıcı yan yana sayfaya bakmalı.
- Terazi tabanında çimen lekesi var.
- Araçlarda `seed` yok.
