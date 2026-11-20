unity-import: pending

# 583 Yeni malzeme ikonları 1/2: sonuç (durum: review)

Yan yana sayfası: `art/results/583-yanyana.png`; x4 önizleme (parşömen + koyu): `art/results/583-ikonlar-x4.png`. Masaüstü kopyası: yerel.

## Araç ve ayarlar
- `create_image_pixen` 32x32, `no_background=true`, `single color outline`, `low detail`, ikon başına bir çağrı (316 yöntemi), 19 gen. Her açıklamanın sonuna "no face, no eyes, no creature, no person" eklendi.
- pixen'in yüz/yaratık çizdiği 6 ikon (boar_meat, duck_egg, goat_cheese, goose_egg, meat, rennet) tarifteki yedek yöntemle `create_map_object` 32x32 (high top-down, low detail) ile bir kez yeniden üretildi: 6 gen. Hız sınırı (10/10) yüzünden 3 çağrı bekleyip bir kez yeniden gönderildi (harcama yok).
- Toplam **25 gen** (cost_est 19, cost_max 38 içinde). get_balance sonrası: **1615,7** (paylaşılan hesap, paralel şeritlerin harcaması dahil).
- Renk: ham 20-59 renk; Pillow median-cut 16 renk, dither yok, alfa ikili (yeniden boyama yok).

## Dosyalar (`Assets/Art/Iso/_aday/`, 32x32 RGBA, pivot x 16, pivot y 0 alttan; ikon, pivot önemsiz)
Teslim edilen 14 (yeni dosya). `meat`, `poultry`, `game_meat` zaten `_aday/` altında vardı, üzerine yazılmadı; yeni hâlleri `art/tmp583/out/` içinde commit edilmedi, istenirse yeniden kopyalanır. Liste: meat, mutton, goat_meat, pork, rabbit_meat, poultry, duck_meat, goose_meat, pigeon_meat, game_meat, duck_egg HARİÇ goose_egg, goat_milk, goat_cheese, offal, gut, horn, rennet (icon_item_ öneki).

## Teslim edilmeyen
- `icon_item_boar_meat`: pixen yaban domuzu yüzü çizdi, map_object bütün bir domuz çizdi (tek deneme). Ham hâller `art/583-red/`. Öneri: tarif "wild boar" kelimesini atıp "dark red raw ham-like meat cut with coarse fat" yazılsın.
- `icon_item_duck_egg`: stil denetiminde KALDI (palet dışı renkler, turkuaz); şekil tanınıyor. `art/583-red/final_duck_egg_KALDI.png`. Öneri: açıklamayı "pale grey-green egg" yap.

## Doğrulama
| Ölçüt | Sonuç |
|---|---|
| Boyut / yarı saydam | 32x32; yarı saydam 0 |
| Renk | ≤16 |
| style.py check arayuz | 12 GEÇTİ, 5 UYARI (gut, meat, mutton, offal, pigeon_meat: palete uzak), 1 KALDI (duck_egg, teslim edilmedi) |
| validate_iso | uygulanmadı (UI ikonu) |

## Sorunlar (kullanıcı gözüyle bakılmalı)
- `duck_meat` ekmek somunu gibi, `pigeon_meat` somon dilimine benziyor, `gut` kutu üstünde sarmal gibi, `horn` mavi-mor ve belirsiz: zayıf, yeniden üretim önerilir. Teslim edildi, reddi kullanıcıya bırakıldı.
- Tarifte ışık sol-üstten ve kontur ölçütleri göz kontrolüyle makul.
