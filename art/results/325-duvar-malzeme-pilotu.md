unity-import: pending

# 325 Duvar malzemesi pilotu: kalas ve taş (bulut işçisi, şerit 1, 2026-11-18)

Yan yana sayfası: [325-yanyana.png](325-yanyana.png) (4 yeni + 3 referans; taş dosyaları teslim edilmedi, yalnızca sayfada)

## Araç ve ayarlar
- `inpaint_image_pro_flash`, `Modify current layer`, `no_background=true`, `seed=0` (008b yöntemi), 4 çağrı x 5 = **20 generation**. Araç açıklamasındaki maliyet (96 px'e kadar 5) `cost_est` ile uyumlu.
- Kaynak `iso_wall_wall_*` 102x115 tuvalden 96x96 kırpım (ofset sw_ne (0,19), se_nw (6,19)). Maske: ön yüz iç bölgesi (sütun başına tabandan -58..-7 satır; sw_ne x38-61, se_nw x44-67; kenar direkleri ve üst kiriş dışarıda). `tools/art/wall_compose.py` ile aynı ofsetle geri konuldu; maske dışı bayt değişmedi, tuval ve pivot (sw_ne (32,19), se_nw (70,19)) aynı.
- Kalan `get_balance`: 1033.7 (diğer şeritlerin harcaması dahil, ortak bakiye).
- Masaüstü kopyası: yerel.

## Dosyalar (teslim)
| Dosya | Boyut | Pivot (x, y ALTTAN) | Renk | Yarı saydam |
|---|---|---|---|---|
| Assets/Art/Iso/iso_wall_wall_plank_sw_ne.png | 102x115 | 32, 19 | 30 | 0 |
| Assets/Art/Iso/iso_wall_wall_plank_se_nw.png | 102x115 | 70, 19 | 33 | 0 |

## Teslim EDİLMEYEN
| Dosya | Neden |
|---|---|
| iso_wall_wall_stone_sw_ne | Yalnızca 217 piksel değişti; taş küçük bir yamada, kalanı eski sıva/çapraz kiriş. |
| iso_wall_wall_stone_se_nw | Taş uygulanmadı, kaynakla neredeyse aynı. İki eksen aynı malzemeyi göstermiyor (ölçüt tutmadı). |

Tek deneme kuralı gereği yeniden denenmedi.

## Doğrulama
`validate_iso.py --kind object --max-colors 48`: dördü teknik olarak GEÇTİ. `style.py check --family yapi`:

| Dosya | Palete yakın | Ort. uzaklık | Doygunluk | Sonuç |
|---|---|---|---|---|
| plank_sw_ne | %100 | 3.7 | x0.91 | GEÇTİ |
| plank_se_nw | %100 | 3.1 | x1.01 | GEÇTİ |
| stone_sw_ne | %100 | 2.7 | x0.80 | GEÇTİ (teslim yok) |
| stone_se_nw | %100 | 3.2 | x0.95 | GEÇTİ (teslim yok) |

## Sorunlar / öneri
- Kalas: kalaslar yalnızca orta şeritte; kenar direkleri ve üst kiriş eski bej sıvada kaldı. Görsel onay kullanıcıda.
- Taş: model taşı uygulamadı. Öneri: maskeyi genişletip ya da yeni seed ile ayrı iş (325b) açılsın; 326-332 türevleri taş yöntemine bağlıysa bunu bekler.
- Döşeme testi geçerli değil (duvar nesnesi).
