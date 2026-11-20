unity-import: pending

# 477b Ürün ikonları: beceri tamamlama 2/4: sonuç (durum: review)

Yan yana sayfası: `art/results/477b-yanyana.png` (arayuz ailesinde kabul referansı yok, yalnızca yeni 17 görsel).

## Araç ve ayarlar
- `create_map_object` basic mod, 32x32, `low detail`, `basic shading`, `single color outline`, `high top-down` (477a/420a yöntemi). Her açıklamanın sonuna "simple still-life object, no face, no eyes, no creature, no person". 17 çağrı x 1 gen = **17 gen** (cost_est 17, cost_max 36).
- Hız sınırı: ilk turda 3 çağrı (quilt, dowry_linen, tulum) "rate limit exceeded" verdi; harcama yok, boşalınca bir kez yeniden gönderildi (tarifin tek istisnası). quilt ikinci gönderimde alındı; dowry_linen ve tulum ikinci gönderimde yine hız sınırına takıldı, üçüncü gönderimde alındı (hatalı çağrılar faturalanmadı).
- Renk indirgeme: 7 ikon (cart_wheel 41, dowry_linen 23, fish_oil 25, fishing_net 18, quilt 25, silk 17, tin_brooch 45 renk) median-cut ile 15 renge indirildi (dither yok, alfa korundu); ham hâller `art/results/477b-red/raw_*.png`.
- `get_balance` sonrası: **2527,9** (paylaşılan hesap, paralel şeritlerin harcaması dahil).
- Masaüstü kopyası: yerel.

## Dosyalar (`Assets/Art/Iso/_aday/`, 32x32 RGBA, pivot x 16, pivot y 0 alttan; ikon, pivot önemsiz)
silk_thread, silk, bait, fishing_net, fish_oil, cart_wheel, hinge, tin_brooch, tunic, linen_shirt, quilt, dowry_linen, tulum, tulum_cheese, boots, leather_vest, harness (hepsi `icon_item_<ad>.png`).

## Doğrulama
| Ölçüt | Sonuç |
|---|---|
| Boyut | 32x32 (17/17) |
| Yarı saydam piksel | 0 |
| Renk sayısı | ≤16 (17/17) |
| `style.py check --family arayuz` | KALDI yok. GEÇTİ: boots, fishing_net, harness, leather_vest, silk, silk_thread, tulum, tunic. UYARI (kilit kurulmadı / palete uzak): bait, cart_wheel, dowry_linen, fish_oil, hinge, linen_shirt, quilt (palet dışı), tin_brooch (palet dışı), tulum_cheese (palet dışı) |
| `validate_iso.py` | uygulanmadı (UI ikonu) |

## Sorunlar
1. Tek deneme kuralı gereği yeniden üretim yapılmadı; şekil okunabilirliği kullanıcı gözüyle denetlenmeli (özellikle hinge, tulum_cheese, tin_brooch, quilt).
2. Aynı hesaptaki paralel şeritlerin işleri `wait_for_jobs` listesinde görüldü; bunlar 477b'ye ait değil.
