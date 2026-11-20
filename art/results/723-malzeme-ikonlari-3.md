unity-import: pending

# 723 Yeni malzeme ikonları 3: sonuç (durum: review)

Yan yana sayfası: `art/results/723-yanyana.png` (arayuz ailesinde referans yok). Parşömen önizlemesi x4: `art/results/723-ikonlar-x4.png`.

## Araç ve ayarlar
- `create_image_pixen`, 32x32, `no_background=true`, `outline: single color outline`, `detail: low detail`, her açıklamanın sonunda "no face, no eyes..." eki. İkon başına bir çağrı (316 yöntemi). 14 çağrı x 1 = **14 gen** (`cost_est` 14). Hız sınırı (10/10, paylaşılan hesap) hatası alan çağrılar boşalınca bir kez yeniden gönderildi; hatalı çağrı harcama yapmadı.
- `get_balance` sonrası: 1535,7.
- Ham çıktılar 40-61 renkti; Pillow median-cut ile 16 renge indirildi (dither yok, alfa 0/255). Yeniden boyama yok.
- Masaüstü kopyası: yerel.

## Teslim edilenler (`Assets/Art/Iso/_aday/`, 32x32 RGBA, pivot x 16, pivot y 0 alttan)
cinnamon, dill_dried, gut_string, lavender_dried, mint_dried, picnic_cloth, sage_dried, sucuk, thyme_dried, treatise_table, treatise_waters, vine_leaf (12).

## Teslim edilmeyenler (`art/results/723-red/`)
- `icon_item_horn_comb`: kek/mantar benzeri, üstünde göz gibi noktalar; boynuz tarak değil, "yüz yok" ölçütü tutmadı. Öneri: `create_map_object` 32x32 ile yeniden üret.
- `icon_item_treatise_sweets`: stil denetimi KALDI (palete %75, palet dışı renkler); ayrıca kitap değil rulo/kağıt çıktı.
- **DİKKAT:** `treatise_sweets` yanlışlıkla `Assets/Art/Iso/_aday/` altına kopyalandı ve kabuk silmeye izin vermediği için orada kaldı. Kullanıcı bu dosyayı silmeli (kopyası `723-red/` içinde).

## Doğrulama
| Ölçüt | Sonuç |
|---|---|
| Boyut 32x32 | 14/14 |
| Yarı saydam piksel | 0 |
| Renk ≤16 | 14/14 |
| style.py check | 10 GEÇTİ (picnic_cloth %95, thyme %95 dahil), 1 UYARI (vine_leaf, palete uzak 12,3), 1 KALDI (treatise_sweets) |
| validate_iso | uygulanmadı (UI ikonu) |

## Sorunlar
- `treatise_table` kitap, `treatise_waters` rulo çıktı; üç risale ailesi biçim olarak tutarsız.
- `gut_string` yumak topu gibi okunuyor; kabul edilebilir.
- Eksik 2 ikon (horn_comb, treatise_sweets) için yeni iş gerekir (~2 gen).
