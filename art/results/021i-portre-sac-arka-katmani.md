unity-import: pending

# 021i Portre saç arka katmanı: sonuç

Durum: tamam (10/10 teslim, KALDI yok; hepsi "soluk" UYARI). Masaüstü kopyası: yerel (bulut işçisi, atlandı).

## Araç ve ayarlar
`inpaint_image_pro_flash`, `Modify current layer, only changes`, 64x64, seed 0, 5 gen/çağrı. Taban `portrait_base_oval_adult_64.png` (`upload:` ile). Açıklama: `<ad> hairstyle seen from behind the head, neutral grey, back layer only` + `neutral grey, will be recoloured` + ortak ek. Maske (saf siyah/beyaz): tüm tuval, yüz dikdörtgeni (21,16)-(42,42) hariç. Eşzamanlılık sınırı (8, diğer şeritler dolu): reddedilen çağrılar bekleyip yeniden gönderildi, faturalanmadı.

## Harcama
10 x 5 = **50 gen** (cost_max 50). `get_balance` sonrası: 1373,7 (paralel şeritler dahil). Defter: `art/ledger/021i.md`. Renk 23-29, `reduce_colors` gerekmedi.

## Temizlik (yalnızca silme, yeniden boyama yok)
Model maske içinde omuz/kulak/ten/giysiyi de yeniden çizdi. Katmanlardan tabanla birebir aynı pikseller, sonra gri olmayan (doygun) pikseller (gri saça bitişik koyu dış hat hariç) saydam yapıldı. Sonuç yalnızca gri saç; renk sayısı 6-10'a indi.

## Dosyalar (portre, pivot yok; 64x64, yarı saydam piksel 0, maske dışı 0)
Yol: `Assets/Art/Iso/_aday/portrait_layer_hair_<ad>_back_64.png`

| Ad | Sonuç | Not |
|---|---|---|
| bald_fringe | UYARI soluk | baş arkası kemeri, sağlam |
| braid_single | UYARI soluk | sırtta örgü |
| braids_two | UYARI soluk | iki örgü, küçük |
| bun_high | UYARI soluk, palete uzak | topuz üstte, y=1 |
| bun_low | UYARI soluk | zayıf: düşük topuz görünmüyor |
| ponytail | UYARI soluk, palete uzak | sağ omuzda kuyruk |
| topknot | UYARI soluk | iyi |
| receding | UYARI soluk, palete uzak | çok seyrek (yan tutamlar) |
| long_curly | UYARI soluk | en dolu |
| coif_hair | UYARI soluk | zayıf, yalnızca kemer |

Yan yana: `art/results/021i-yanyana.png`. Tabana birleşim (x3): `art/results/021i-onizleme-x3.png`.

## Doğrulama
`validate_iso.py`: 10/10 geçti. Stil: KALDI yok; "soluk" UYARI gri katman olduğundan beklenir. Birleşimde kayma/çift dış hat yok.

## Sorunlar
1. Model arka katmanı yalnızca saç olarak tutmadı (torso/kulak da çizdi); temizlik sonrası bun_low, coif_hair, receding, braids_two zayıf. Kullanıcı görsel kararı gerek; gerekirse baş silüetine sıkı maskeyle yeni iş.
2. Baş üstü kemeri tabanın altında kalır; görünür kısım yan/omuz saçı.
