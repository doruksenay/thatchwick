unity-import: pending

# 355 Boru, kurşun ve yeni yiyecek ikonları; su yolu araç ikonları: sonuç (durum: review)

Yan yana sayfası: `art/results/355-yanyana.png` (arayuz ailesinde kabul referansı yok, yalnızca yeni görseller).

## Araç ve ayarlar
- `create_image_pixen`, `no_background=true`, `outline: single color outline`, `detail: low detail`; 7 ikon 32x32, 7 ikon 16x16 (pixen küçük kanvasta kare ve 16 destekler). Gerekçe: 316 ile aynı araç, ikon başına bir çağrı.
- 14 çağrı x 1 gen = **14 gen** (`cost_est` 8, `cost_max` 14: sınır içinde, 2 katı aşılmadı). Hız sınırı (10/10 eşzamanlı iş) 5 çağrıda alındı, işler bitince bir kez yeniden gönderildi (tarifin tek istisnası); reddedilen çağrılar harcama yapmadı.
- `get_balance` sonrası: 4694,9 (paralel şeritlerin harcaması dahil).
- **Renk indirgeme:** ham çıktılar 29-69 renk (ölçüt ≤16). Pillow ile 16 renge median-cut (dither yok, alfa 0/255) indirildi; yeniden boyama yok. Ham hâller `art/tmp355/` altında (commit edilmedi).
- Masaüstü kopyası: yerel.

## Dosyalar (`Assets/Art/Iso/_aday/`, RGBA; ikon, pivot önemsiz: 32x32'de pivot x 16, y 0; 16x16'da pivot x 8, y 0 alttan)
| Dosya | Boyut | Renk |
|---|---|---|
| icon_item_lead.png | 32x32 | 16 |
| icon_item_pipe_wood.png | 32x32 | 15 |
| icon_item_pipe_clay.png | 32x32 | 16 |
| icon_item_pipe_lead.png | 32x32 | 16 |
| icon_item_kasar.png | 32x32 | 16 |
| icon_item_kimiz.png | 32x32 | 15 |
| icon_item_salgam.png | 32x32 | 16 |
| ui_icon_tool_conduit_ditch.png | 16x16 | 14 |
| ui_icon_tool_conduit_stone.png | 16x16 | 15 |
| ui_icon_tool_conduit_flume.png | 16x16 | 15 |
| ui_icon_tool_conduit_pipe_wood.png | 16x16 | 15 |
| ui_icon_tool_conduit_pipe_clay.png | 16x16 | 15 |
| ui_icon_tool_conduit_pipe_lead.png | 16x16 | 15 |
| ui_icon_tool_conduit_erase.png | 16x16 | 15 |

## Doğrulama
| Ölçüt | Sonuç |
|---|---|
| Boyut | 32x32 (7/7), 16x16 (7/7) |
| Yarı saydam piksel | 0 (14/14) |
| Renk sayısı | ≤16 (indirgeme sonrası) |
| `style.py check --family arayuz` | KALDI yok; 3 GEÇTİ (kimiz, pipe_lead x2), 11 UYARI (kilit kurulmadı / palete uzak) |
| `validate_iso.py` | uygulanmadı (izometrik değil, UI ikonu) |
| Üç boru ayırt ediliyor mu | Evet, renkle (kahve / kiremit kırmızısı / gri) |

## Sorunlar (göz kontrolü, kullanıcı kararı için)
1. **Boru şekli üç malzemede aynı değil** (tarif "aynı boru şekli" diyordu): `icon_item_pipe_clay` dirsekli/testi benzeri bir parça, `pipe_wood` küçük ve ince silindir, `pipe_lead` kalın boru. Renk ayrımı net, şekil tutarsız.
2. **`icon_item_lead`** külçe yerine gri silindir/boru gibi okunuyor; `pipe_lead` ile karışabilir. Yeniden üretim önerilir ("rectangular ingot, trapezoid bar").
3. **16x16 araç ikonları:** `pipe_clay`/`pipe_lead` halka kesit olarak okunuyor; `pipe_wood` dolu kahverengi top gibi, halka değil. `ditch` ve `stone` mavi şeritli küçük kare/küre gibi, ark olarak zor okunuyor; `flume` sehpa belirsiz; `erase` çarpı seçilemiyor. 16x16 pixen sınırı; elle düzeltme ya da 32x32 üretip küçültme önerilir.
4. `kasar` (tekerlek peynir, dilim kesik), `kimiz` (deri tulum) ve `salgam` (mor sıvılı testi) tanınıyor; yüz/göz yok.
5. Koyu/açık zemin 2x testi yalnızca koyu zeminde yapıldı.
