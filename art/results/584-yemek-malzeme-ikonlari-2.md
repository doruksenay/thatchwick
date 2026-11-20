unity-import: pending

# 584 Yeni malzeme ikonları 2/2: sonuç (durum: review)

Yan yana sayfası: `art/results/584-yanyana.png` (arayuz ailesinde kabul referansı yok, yalnızca 16 yeni görsel). Önizleme x4: `art/results/584-ikonlar-x4.png` (parşömen), `art/results/584-ikonlar-x4-koyu.png` (koyu zemin).

## Araç ve ayarlar
- `create_image_pixen`, 32x32, `no_background=true`, `outline: single color outline`, `detail: low detail`; her açıklamaya `, simple still-life object, no face, no eyes, no creature, no person` eklendi. 316 yöntemi: ikon başına bir çağrı.
- 18 çağrı x 1 gen = **18 gen** (`cost_est` 18, `cost_max` 36). Hız sınırı (10/10 eşzamanlı iş, paylaşılan hesap) hatası çok kez alındı; reddedilen çağrılar harcama yapmadan yeniden gönderildi (tarifin tek istisnası). Her ikon tam bir kez üretildi.
- `get_balance` sonrası: 1615,7 (paralel şeritlerin harcaması dahil).
- **Renk indirgeme:** ham çıktılar 31-62 renk (ölçüt ≤16); Pillow ile 16 renge median-cut (dither yok), alfa 0/255 eşiklendi (ham yarı saydam piksel 0). Yeniden boyama yok.
- Masaüstü kopyası: yerel.

## Dosyalar (`Assets/Art/Iso/_aday/`, 32x32 RGBA, pivot x 16, pivot y 0 alttan; ikon, pivot önemsiz): 16 teslim
icon_item_{soup_bone, tomato, seed_tomato, pomegranate_molasses, verjuice, sumac, plum_sour, mint_ground, thyme_ground, dill_ground, lavender_ground, sage_ground, fenugreek_ground, fenugreek_roasted, family_blend, picnic_basket}.png

## Teslim edilmeyenler (`art/results/584-red/`)
- `icon_item_tomato_puree.png`: yüz çizilmiş (göz ve ağız), ayrıca stil denetiminde KALDI (palet dışı renkler, %73).
- `icon_item_tomato_paste.png`: çömlek altında göz benzeri iki nokta (yüz izlenimi); tarif "no face" şartına aykırı.
Tek deneme kuralı (`max_attempts: 1`) gereği yeniden üretilmedi; yeniden üretim için 2 gen yeterli (öneri: `create_map_object` 32x32, 401/417 yöntemi).

## Doğrulama
| Ölçüt | Sonuç |
|---|---|
| Boyut | 32x32 (16/16) |
| Yarı saydam piksel | 0 |
| Renk sayısı | ≤16 (indirgeme sonrası 14-16) |
| `style.py check --family arayuz` | 12 GEÇTİ, 4 UYARI (family_blend, fenugreek_ground, tomato, verjuice: palete uzak), 1 KALDI (tomato_puree, teslim edilmedi) |
| `validate_iso.py` | uygulanmadı (izometrik değil, UI ikonu) |

## Sorunlar
1. `verjuice`: çömlek yerine yalnızca yeşil üzüm salkımı çizilmiş (üzüm tanınıyor, kap yok). Teslim edildi, kullanıcı değerlendirsin.
2. `soup_bone`: ayaklı kazan biçimi (kil çömlek yerine); `plum_sour`: rulo yerine kese/çanta gibi okunuyor, belirsiz. `sage_ground`: kese yerine çömlek biçimli.
3. `tomato` ve `seed_tomato` sağlıklı; domates yalnızca salça için bilinçli istisna (60a-E1).
4. 2 ikon (tomato_paste, tomato_puree) eksik: itemlar için yeniden üretim gerekir.
