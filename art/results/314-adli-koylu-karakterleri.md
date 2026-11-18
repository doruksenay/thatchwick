unity-import: pending
# 314 Adlı köylüler (Orhan, Hatice, Zeynep, Murat, Kerem, Ayşe): sonuç

- Durum: `review`. Harcanan 12,6 gen (6 × 2 + 6 × 0,1; cost_est 10,2, cost_max 14 içinde). `get_balance` sonrası 865,1 (aynı anda çalışan başka şeritlerin harcaması da bu rakamda).
- Yan yana: `art/results/314-yanyana.png` (stil denetimi). 14 kişilik sıra (012'nin 8'i + 6 yeni, x3): `art/results/314-14li-sira.png`.
- Masaüstü kopyası: yerel (bulut işçisi).

## Araç ve ayarlar
`create_character`, `mode=v3`, `humanoid`, `view=high top-down`, `outline=single color black outline`, `detail=high detail`, 012'nin ortak açıklama eki, 8 yön. 6 çağrı paralel gönderildi (hız sınırı yok). Sonra karakter başına bir `reduce_colors` (`num_colors=32`, `dithering=none`, 8 yön birlikte, `upload:` ile).
- **Sapma (orhan):** tarif `size=58` dedi, v3 yalnızca 4'ün katını kabul ediyor (çağrı reddedildi, ücret yazılmadı). `size=56` kullanıldı, `char_pad.py` ile 60x60'a getirildi (farmer_a2_56 ile aynı yön ayak hizası). Gerçek boy 54.
- Diğer beşi `size=60`. Referans verilmedi (012 tarifinde de yok).

| Çağrı | Id | Gen |
|---|---|---|
| create_character orhan (56) | 3c778523-9f6b-46e1-adb8-b6302df43c0d | 2 |
| create_character hatice (60) | 94e0b28e-f765-4862-93b8-4afe18d0780c | 2 |
| create_character zeynep (60) | 3746a869-832d-4b9c-a612-3c6f820ce2df | 2 |
| create_character murat (60) | 5c02872c-55fa-4c89-8c3f-89d6518f4638 | 2 |
| create_character kerem (60) | c47130f1-4a74-452c-9500-76bd69f1d551 | 2 |
| create_character ayse (60) | 2ecddf1c-485e-43c3-8618-dcf68f981b37 | 2 |
| reduce_colors x6 | 6e983289, 3d093160, 24fdda3b, 2377726f, c9672b92, b92b8002 | 0,6 |

## Dosyalar (hepsi 60x60, `.meta` yok)
- `Assets/Art/Iso/_aday/iso_char_<ad>_56_cand_{s,se,e,ne,n,nw,w,sw}.png`, 6 × 8 = 48 dosya (renk azaltılmış).
- Ham: `Assets/Art/Iso/_aday/_ham/iso_char_<ad>_56_cand_<yön>.png`, 48 dosya.
- Betikler: `tools/art/{fetch314,fetch314b,upload314,verify314,check314,row314}.py` (yalnızca bu iş için indirme/ölçüm).

## Doğrulama (SE, renk azaltılmış)
| Karakter | Boy | Genişlik | Renk (SE / 8 yön) | Yarı saydam | Alfa = ham | Pivot x | Pivot y (alttan) | validate_iso | Stil |
|---|---|---|---|---|---|---|---|---|---|
| orhan | 54 | 33 | 32 / 32 | 0 | evet | 29,5 | 1 | geçti | UYARI ailesinden soluk (x0,56) |
| hatice | 59 | 30 | 32 / 32 | 0 | evet | 30,0 | 0 | geçti (alt boşluk 0) | geçti (x0,88) |
| zeynep | 58 | 33 | 32 / 32 | 0 | evet | 27,5 | 1 | geçti | UYARI ailesinden soluk (x0,46) |
| murat | 55 | 37 | 32 / 32 | 0 | evet | 32,5 | 2 | geçti | geçti (x0,72) |
| kerem | 55 | 37 | 32 / 32 | 0 | evet | 29,5 | 2 | geçti | geçti (x1,08) |
| ayse | 58 | 42 | 32 / 32 | 0 | evet | 34,0 | 0 | geçti (alt boşluk 0) | geçti (x0,63) |

Stil denetiminde "palete yakın" %95-100, KALDI yok.

Pivot y, yön başına (s, se, e, ne, n, nw, w, sw): orhan 1,1,1,2,2,2,1,1; hatice 1,0,1,1,1,1,1,0; zeynep 1,1,1,1,2,1,1,1; murat 3,2,2,3,5,4,2,2; kerem 3,2,2,3,5,3,2,2; ayse 0 (8 yönde). Murat ve Kerem ayakları 2-5 px yüksekte; yön başına pivot gerekir (010/011 yöntemi). Hatice ve Ayşe tuvalin dibine değiyor (alt boşluk 0).

## Baskın gövde rengi (SE)
orhan #51352b koyu kurum-kahve; hatice #d5c7a7 un-bej; zeynep #52655a soluk yeşil; murat #e3d5ae krem; kerem #40211a koyu kırmızı-kahve; ayse #925631 pas-kahve. 012'nin sekizi: woman_a #cbbb9d, elder_m #7b502a, elder_f #402127, boy_a #8f5729, girl_a #7a8a95, woodcutter_a #535520, farmer_f #c8b798, craftsman_a #774b28.

## Sorunlar / notlar
- **Benzer çiftler (baskın renk):** hatice (#d5c7a7), murat (#e3d5ae) ve farmer_f (#c8b798) açık bej; ayrım silüet (başörtü/önlük, geniş şapka, çomak). kerem (#40211a) ile elder_f (#402127) neredeyse aynı koyu kırmızı-kahve; ayrım kürk yaka, bohça, sakal ve yaş/boy. Tarifin "hiçbir ikisi aynı baskın renkte olmasın" ölçütü bu iki çiftte gevşek tutuyor; kullanıcı sıraya bakıp karar versin.
- Zeynep ve Orhan ailesinden soluk (UYARI), teslim edildi.
- 8 yönde aksesuar tutarlılığı gözle denetlenmedi; kullanıcı `314-14li-sira.png` ve `314-yanyana.png` ile bakmalı.
- Aday klasörü dışına taşıma yok; onay sonrası taşınır.
