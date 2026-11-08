unity-import: pending

# 300 Ev eşyaları grup 1: sonuç

- İş: `art/queue/300-esya-grup1.md` (approved_by: user, cost_est 8, cost_max 15)
- Tarih: 2026-11-08
- Harcanan: **14 generation** (`get_balance` önce 1867,8, sonra **1853,8**; bugün kullanılan 146,2 / 150)
- Teslim: 6 / 8 (`Assets/Art/Iso/_aday/`). Sandık ve bardak teslim edilmedi (iki deneme de tutmadı).
- Toplu önizleme: `art/results/300-esya-grup1-toplu-x4.png` (köylü, çimen, 8 eşya; kırmızı alt çizgi = teslim edilmedi; kırmızı noktalar = yüzey yuvaları)

## Araç ve ayarlar
- `create_map_object` stil eşleme (`background_image` + `inpainting` `{"type":"mask"}`), `view=high top-down`, `detail=low detail`, `shading=basic shading`, `outline=single color outline`. Açıklama metinlerine ortak ek eklendi (tarifteki gibi).
- Kılavuz ve maske: `tools/art/footprint_guide.py` (`--axis x`, tablodaki tuval, n ve yükseklik). Dekorlar (32x32) için aynı mantıkla geçici betik: 12x6 tam 2:1 elmas, merkez (16,25), alt kenardan 4 px yukarıda (y 22-27); maske elmasın yükseklik kadar yukarı uzatılmışı, 1 px genişletilmiş. Betik repoya eklenmedi.
- 1. deneme: `iso_item_stool_se.png` tuvalin solunda, alta hizalı; tuval 80 + eşya (en çok 192); maske tabure bölgesinde siyah. Çıktıdan eşya bölgesi kendi tuval boyutunda kesildi (boyama yok). Araç tabure bölgesini her seferinde boş döndürdü.
- 2. deneme (kurala göre): tabure olmadan, yalnızca kılavuz + maske. Açıklama metni aynı kaldı (ortak ekteki "matching the stool on the left" ifadesi dahil).
- Not: Base64 aktarımında iki çağrı bozuk veri yüzünden araç tarafından reddedildi (iş açılmadı, bakiye değişmedi). Sonraki çağrılarda PNG'ler kayıpsız paletli (P) kodlandı; içerik aynı.
- `reduce_colors` yapılmadı (tarif gereği); hiçbir dosya 48 rengi aşmıyor.

## Çağrılar
| # | Eşya | Deneme | Object ID | Gen | Sonuç |
|---|---|---|---|---|---|
| 1 | masa | 1 (tabureli) | `2de787ca-d083-4be4-91ec-6efb28147d29` | 1 | teslim |
| 2 | sandalye | 1 (tabureli) | `b3b46113-4f34-4859-ac0f-ab84d1dbfd05` | 1 | teslim |
| 3 | sandık | 1 (tabureli) | `206935f1-74b3-40ba-94bc-9934e81ab79d` | 1 | yarım nesne (sol kenarda 25 px dilim, kaplama %39) |
| 4 | sandık | 2 (kılavuz) | `e3cd7c88-4318-4225-ad3b-987e2910a294` | 1 | ayak izi tutmadı (aşağıda), teslim yok |
| 5 | raf | 1 (tabureli) | `3aeeb7a4-6f34-4395-8e09-2c9d8f1f223f` | 1 | tanınmıyor: arkalıklı basamaklı bank gibi, önde açık raf yok |
| 6 | kazan | 1 (tabureli) | `23b7b9a1-b904-4df9-9612-2508a6f0b8fb` | 1 | parça (sol kenarda dilim, kaplama %0 sağda) |
| 7 | kazan | 2 (kılavuz) | `81d0ecf2-6981-4728-b48f-95b37448a68c` | 1 | teslim (içi dolu, aşağıda) |
| 8 | mum | 1 (tabureli) | `04ee1d44-9330-4f86-904d-81589459ef38` | 1 | tamamen boş çıktı |
| 9 | mum | 2 (kılavuz) | `f5662ac2-287f-4550-a148-678a9399c219` | 1 | teslim |
| 10 | saksı | 1 (tabureli) | `b70f21b8-adfb-44b4-b7f9-6cca8a31d25a` | 1 | tamamen boş çıktı |
| 11 | saksı | 2 (kılavuz) | `efaf86fe-f132-412d-9e8d-4ebd970f2ea2` | 1 | teslim |
| 12 | bardak | 1 (tabureli) | `8bd1435a-5933-438e-8e68-a24e75f496cf` | 1 | tamamen boş çıktı |
| 13 | raf | 2 (kılavuz) | `ba7b655f-cd36-4f32-a85f-dc3137c04b6c` | 1 | teslim |
| 14 | bardak | 2 (kılavuz) | `04161af3-fa82-45ba-83fc-593481480a64` | 1 | tabana değmiyor, teslim yok |

Gözlem: 32x32 dekorlarda tabure referanslı (112x64) mod üçte üç boş döndü; büyük eşyalarda üçte ikisi tutmadı. Kılavuzlu yöntem 5 denemenin 4'ünde tuttu.

## Dosyalar (pivot = taban elması merkezi, y ALTTAN, px)
| Dosya | Tuval | Ayak izi (tuvalde) | Pivot x, y (alttan) |
|---|---|---|---|
| `Assets/Art/Iso/_aday/iso_item_table_wood_2x1_se_nw.png` | 112x88 | 2x1 +x, x 8-103, y 32-79 | 56, 32 |
| `Assets/Art/Iso/_aday/iso_item_chair_wood_se.png` | 80x84 | 1x1, x 8-71, y 44-75 | 40, 24 |
| `Assets/Art/Iso/_aday/iso_item_shelf_wood_2x1_se_nw.png` | 112x96 | 2x1 +x, x 8-103, y 40-87 | 56, 32 |
| `Assets/Art/Iso/_aday/iso_item_cooking_pot_se.png` | 80x68 | 1x1, x 8-71, y 28-59 | 40, 24 |
| `Assets/Art/Iso/_aday/iso_item_candle_se.png` | 32x32 | dekor 12x6, x 10-21, y 22-27 | 16, 7 |
| `Assets/Art/Iso/_aday/iso_item_flower_pot_se.png` | 32x32 | dekor 12x6, x 10-21, y 22-27 | 16, 7 |

## Doğrulama
`uv run --with pillow python tools/art/validate_iso.py <dosya> --kind object --max-colors 48 [--footprint X,Y,W,H]`: altı dosyanın hepsi **geçti**.

| Dosya | Opak sınır kutusu | Boy | Renk | Yarı saydam | Alt satıra değme | Maske dışı opak | Ayak izi kaplaması (sol / sağ yarı) | Kılavuz rengi |
|---|---|---|---|---|---|---|---|---|
| masa | x 9-102, y 5-80 | 76 | 15 | 0 | hayır (7 satır boşluk) | 0 | %100 / %100 | yok |
| sandalye | x 13-68, y 4-76 | 73 | 9 | 0 | hayır (7) | 0 | %84 / %91 | yok |
| raf | x 8-103, y 7-88 | 82 | 20 | 0 | hayır (7) | 0 | %100 / %100 | yok |
| kazan | x 10-69, y 6-60 | 55 | 20 | 0 | hayır (7) | 0 | %94 / %91 | yok |
| mum | x 10-21, y 7-24 | 18 | 8 | 0 | hayır (7) | 0 | dekor: tabanın üst yarısında oturuyor (alt y=24, pivot y=25) | yok |
| saksı | x 10-21, y 5-27 | 23 | 13 | 0 | hayır (4) | 0 | dekor: tabana tam oturuyor (alt y=27) | yok |

Kaplama = ön köşeden (pivot x) sınır kutusunun sol ve sağ ucuna uzaklık / ayak izinin yarı genişliği (nominal 32 px; 2x1'de ön köşeden ölçüldü).

### Işık (ortalama parlaklık; üst / sol yüz / sağ yüz, yaklaşık)
Üst yüz = ayak izinin ölçülen yükseklik kadar kaldırılmış hali; geri kalanı ön köşenin solu ve sağı. Yaklaşık ölçüm.
| Eşya | Üst | Sol | Sağ | Sıra doğru mu |
|---|---|---|---|---|
| masa | 72 | 33 | 25 | evet |
| sandalye | 41 | 52 | 52 | hayır, ama ölçüm güvenilmez: "üst" bölgeye koyu arkalık direkleri giriyor; oturak gözle sol yüzden açık. Gözle kontrol edin |
| raf | 140 | 82 | 73 | evet |
| kazan | 42 | 18 | 17 | evet (sağ ve sol neredeyse eşit) |
| mum | 158 | 73 | 71 | evet |
| saksı | 74 | 60 | 65 | kısmen (sağ biraz açık, yapraklar yüzünden) |

### Masa ve raf üst yüzeyi
- **Masa:** üst yüzey tuvalde y 5-52, **ALTTAN 36-83** (yüzey 27 px kaldırılmış; ön köşe ALTTAN 36, arka köşe ALTTAN 83). Yüzey boş. Yuvalar (v = 0,5): u 0,25 → (40, 21) ALTTAN 67; u 0,5 → (56, 29) ALTTAN 59; u 0,75 → (72, 37) ALTTAN 51. Üçü de opak üst yüzeyde.
- **Raf:** üst yüzey tuvalde y 7-54, **ALTTAN 42-89** (33 px kaldırılmış). Yüzey boş. Yuvalar: u 0,3 → (43, 25) ALTTAN 71; u 0,7 → (69, 37) ALTTAN 59. İkisi de üst yüzeyde.
- Yuvalar toplu önizlemede kırmızı noktayla işaretli. Dekor önizlemelerinde mum ve saksı masanın u 0,5 yuvasına (pivot yuva noktasında) yerleştirildi.
- Yuva noktası formülü (2x1, +x ekseni): x = 24 + 64u, y = H - 48 + 32u - kaldırma (masa 27, raf 33).

### İstenmeyen ayrıntı
- Kılavuz rengi (#786046): hiçbirinde yok. Zemin lekesi, pişmiş gölge, alev, buhar, ek nesne: yok.
- **Kazan:** "empty" istendi ama içinde sarımsı yemek ve iki yeşil çubuk (kepçe ya da ot) var. Ateş ve buhar yok.
- **Sandalye:** "seat facing toward the lower right" istendi, ama oturak **SW'ye (sol alta)** bakıyor; arkalık NE kenarında. Dosya adı `_se`. Yatay ayna ile SE'ye bakan sürüm çıkar (ışık sağ-üstten olur).
- **Mum:** sönük, ışık efekti yok. Krem rengi sivri mum ucu küçük ölçekte aleve benzeyebilir.
- **Raf ve kazan (2. deneme):** tabure referansı olmadığı için palet tabureden daha açık ve kızıl (raf açık kızıl ahşap, sandık da öyleydi). Masa ve sandalye tabureyle aynı koyu ahşap ailesinde.

### Ayna uygunluğu (yatay ayna, ışık aynada sağ-üstten gelir; yönerge sol-üst ister)
| Eşya | Sol/sağ yarı parlaklık farkı | Şekil simetrisi (ayna IoU) | Asimetrik ayrıntı | Üretilebilen yönler |
|---|---|---|---|---|
| masa | 4 | 0,44 (2x1 eksen aynada döner) | yok, 180° simetrik | se_nw = SE ve NW; ayna = sw_ne (SW ve NE). 4 yön |
| sandalye | 0 | 0,48 | arkalık (önü SW) | SW (bu dosya), ayna ile SE. Arkadan görünüm (NW, NE) yeni üretim ister |
| raf | 3 | 0,62 | açık raflar SW yüzünde, arka kapalı | se_nw önü SW; ayna ile sw_ne önü SE. Arkası görünen yönler yeni üretim ister |
| kazan | 2 | 1,00 | kulplar simetrik | 4 yön (tek sprite yeter; ayna gerekmez) |
| mum | 2 | 1,00 | yok | 4 yön (tek sprite) |
| saksı | -1 | 0,72 | çiçekler biraz asimetrik | 4 yön (tek sprite ya da ayna) |

## Teslim edilmeyenler
- **Sandık** (`iso_item_chest_wood_se.png`): 1. deneme yarım nesne (tuvalin sol kenarında 25 px'lik dilim). 2. deneme tam bir sandık ama ayak izinden küçük: sınır kutusu x 15-64, kaplama nominal 64 px'lik tabana göre %78 / %78 (çizili 62 px'lik elmasa göre %81), ön alt köşe tabanın ön köşesinden 5 px yukarıda (y 58, taban y 63). Kural "%80'den küçükse tutmaz" (nominal ölçü) olduğu için teslim edilmedi. Sınırda; görsel olarak kabul ederseniz `_red` kopyası masaüstünde. Ayrıca palet tabureden açık turuncu. Sol/sağ farkı -18 (sağ yüz soldan açık, ışık sırası ters).
- **Bardak** (`iso_item_clay_cup_se.png`): 1. deneme tamamen boş. 2. deneme bardak (10x10 px, 6 renk) ama taban elmasına hiç değmiyor: alt y=20, taban y 22-27; 2-7 px havada. Teslim edilmedi.

## Masaüstü kopyası
- `C:\Users\User\OneDrive\Desktop\art\esya\2026-11-08_<ad>\` (table_wood, chair_wood, chest_wood, shelf_wood, cooking_pot, candle, flower_pot, clay_cup): teslim edilen PNG, `_onizleme_x4.png` (köylü, çimen karosu, ayak izi konturlu eşya; dekorlarda masa üstündeki hali), reddedilen denemeler `_red\` altında.
- `C:\Users\User\OneDrive\Desktop\art\esya\2026-11-08_300_toplu_x4.png`
- `C:\Users\User\OneDrive\Desktop\art\_indeks.md`: 8 satır eklendi.

## Sorunlar ve öneriler
- Tabure referanslı stil eşleme bu turda güvenilir değildi (8 denemenin 2'si tuttu; dekorlarda hiç). Gelecek gruplarda doğrudan kılavuz yöntemi ya da daha küçük bir referans denenebilir; karar kullanıcıda.
- Kılavuz modunda palet açık ve kızıl kayıyor; toplu renk azaltma ya da palet eşleme aşamasında ele alınabilir.
- Pivotlar `Assets/Editor/IsoPivots.txt`'e kullanıcı tarafından eklenecek; işçi dokunmadı.
