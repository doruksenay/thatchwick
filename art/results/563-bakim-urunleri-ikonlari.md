unity-import: pending

# 563 Bakım ürünleri ve avokado ikonları: sonuç (durum: review, kısmi)

Yan yana sayfası (yalnızca teslim edilen 5): `art/results/563-yanyana.png` (arayuz ailesinde referans yok).

## Araç ve ayarlar
- `create_image_pixen`, 32x32, `no_background=true`, `outline: single color outline`, `detail: low detail`; 316 yöntemi (ikon başına bir çağrı). Her açıklamanın sonuna "no face, no eyes, no creature, no person" eklendi.
- 18 çağrı x 1 gen = **18 gen** (`cost_est` 17, `cost_max` 34). 1 gen boşa gitti: hız sınırı sonrası turuncu merhem (calendula) yanlışlıkla ikinci kez gönderildi (atıldı). 10/10 eşzamanlı iş hız sınırı hatası alındı, çağrılar boşalınca yeniden gönderildi (harcama yok).
- `get_balance` sonrası: 1674,7 (paralel şeritler dahil).
- Renk: ham 30-67 renk, Pillow median-cut ile 16'ya indirildi (dither yok, alfa 0/255). Ham hâller `art/tmp563/` (commit edilmedi).
- `map_object` yöntemi kullanılmadı (tek deneme kuralı); yüzlü çıkanlar yeniden denenmedi.
- Masaüstü kopyası: yerel.

## Teslim edilenler (`Assets/Art/Iso/_aday/`, 32x32 RGBA, pivot x 16, pivot y 0 alttan)
icon_item_rose_water, icon_item_lip_balm, icon_item_lavender_sachet, icon_item_avocado, icon_item_avocado_pit (5/17).

## Teslim edilmeyenler (`art/results/563-red/`, 12)
| Dosya | Neden |
|---|---|
| avocado_cream, bath_set, calendula_salve | `style.py check`: KALDI (palet dışı renkler, %68-73); `fit` referanssız ailede çalışmadı |
| almond_oil | yaratık/yüz çizilmiş, şişe değil |
| avocado_mash | göz/yüz var |
| chamomile_rinse | insan figürü çıktı |
| cold_cream | yüz var |
| hand_salve | figür/yüz, kova değil |
| henna_paste | böcek/yaratık gibi |
| hypericum_oil | kavanozda göz var |
| joint_salve | saksıda yüz |
| walnut_rinse | testi değil, belirsiz şekil |

## Doğrulama (teslim edilen 5)
| Ölçüt | Sonuç |
|---|---|
| Boyut 32x32, yarı saydam piksel 0, renk ≤16 | 5/5 |
| `style.py check --family arayuz` | 5 GEÇTİ |
| `validate_iso.py` | uygulanmadı (UI ikonu) |
| Yüz/yaratık yok (göz kontrolü) | 5/5 |

## Sorunlar / öneri
1. pixen, "no face" son ekine rağmen 12 ikonun çoğunda yüz/figür üretti (316'daki aynı eğilim). Eksik 12 ikon için `create_map_object` 32x32 + aynı son ek (401/417 yöntemi) ile ayrı bir iş (563b, ~12-24 gen) önerilir.
2. 563-red içindeki 3 KALDI dosyanın şekli ayrıca incelenmedi.
