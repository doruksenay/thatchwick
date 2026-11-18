unity-import: pending

# 020 Portre pilotu: sonuç

Durum: kısmi. 5/8 dosya teslim edildi, 3/8 stil denetiminde KALDI (teslim edilmedi).
Masaüstü kopyası: yerel (bulut işçisi, atlandı).

## Araç ve ayarlar
- `create_image_pixflux`, 64x64, `no_background`, `direction=south`, medium detail, basic shading, palet = `iso_char_farmer_a2_56_cand_se.png` (forced palette). 1 gen/çağrı.
- Katman: `inpaint_image_pro_flash`, `Modify current layer, only changes`, taban `source_image_id` ile (base64 aktarımı bozuldu), maskeler betikle (saf siyah/beyaz). 5 gen/çağrı.

## Harcama
| Çağrı | Gen |
|---|---|
| Portre adayı 1 (aldric, maud, agnes, elder) | 4 |
| Maud ve Agnes aday 2 (seed 7) | 2 |
| Taban (2 deneme) | 2 |
| Katman x3 | 15 |
| **Toplam** | **23** (cost_max 30) |

`get_balance` sonrası: 1788,8 (diğer şeritlerin harcaması dahil).

## Dosyalar (portre, pivot yok; 64x64)
| Yol | Boyut | Not |
|---|---|---|
| Assets/Art/Iso/_aday/portrait_aldric_64.png | 64x64 | stil UYARI |
| Assets/Art/Iso/_aday/portrait_elder_m_64.png | 64x64 | GEÇTİ |
| Assets/Art/Iso/_aday/portrait_base_bald_64.png | 64x64 | GEÇTİ |
| Assets/Art/Iso/_aday/portrait_layer_hair_short_brown_64.png | 64x64 | GEÇTİ |
| Assets/Art/Iso/_aday/portrait_layer_beard_short_64.png | 64x64 | GEÇTİ |

Teslim edilmeyenler (`art/results/020-red/`, `_red` eki): Maud (2 aday), Agnes (2 aday), uzun kızıl saç katmanı.

## Stil denetimi (`style.py check --family karakter`)
| Dosya | Palete yakın | Ort. uzaklık | Doygunluk | Sonuç |
|---|---|---|---|---|
| aldric | %100 | 9.2 | x1.40 | UYARI (doygun) |
| maud aday 1 | %100 | 9.1 | x1.51 | KALDI |
| maud aday 2 | %98 | 10.5 | x1.67 | KALDI |
| agnes aday 1 | %100 | 10.3 | x1.69 | KALDI (palete uzak) |
| agnes aday 2 | %100 | 10.2 | x1.76 | KALDI |
| elder | %94 | 5.9 | x1.09 | GEÇTİ |
| taban (2. deneme) | %100 | 7.0 | x1.18 | GEÇTİ |
| saç kısa | %100 | 7.2 | x1.22 | GEÇTİ |
| saç uzun kızıl | %81 | 12.6 | x1.81 | KALDI |
| sakal | %100 | 7.5 | x1.10 | GEÇTİ |

Yan yana: `art/results/020-yanyana.png` (yalnızca teslim edilen iki tam portre; Maud/Agnes KALDI). Dört portre önizlemesi (ilk adaylar, KALDI olanlar dahil): `art/results/020-onizleme-x4.png`. Katman birleşimi: `art/results/020-katman-birlesim-x4.png`.

## Doğrulama
- Tüm dosyalar 64x64, yarı saydam piksel 0. Renk sayısı: portreler 16-24; kısa saç 30, sakal 27 (≤32). Uzun saç 34 (>32, ayrıca KALDI, `reduce_colors` çağrılmadı).
- Katman maske dışı piksel: 0 (üçünde de).
- Birleşim (taban + kısa saç; + uzun saç; + kısa saç + sakal): boşluk, kayma ya da çift dış hat görünmüyor. Katmanlar taban üzerine temiz oturuyor.
- `validate_iso.py` (aldric): geçti (yalnızca boyut/alfa anlamlı).

## Sorunlar
1. **Taban kel ama sakallı:** iki denemede de taban sakalsız çıkmadı (ilk: tam sakal, ikinci: kısa sakal). Teslim edilen taban hâlâ sakal taşıyor; sakal katmanı üstüne biniyor. Tek aday sınırı nedeniyle üçüncü deneme yapılmadı. Öneri: sakalı inpaint ile sildirmek ya da yüz parçaları setini ayrı üretmek.
2. **Doygunluk:** kadın/çocuk portrelerinde (ten, kızıl saç) PixelLab paleti aşıp aile ortalamasından çok doygun çıkıyor; 4 aday da KALDI. Öneri: palet referansına portre tonları eklemek ya da portre için ayrı doygunluk eşiği.
3. Aldric'te kaş yarası belirgin değil; Agnes gözleri yeşil değil, kahve (reddedilen aday). Elder'ın saçı kukuletanın altında kayboluyor.
4. Katman akışı çalıştı: "only changes" + maske, maske dışı 0 piksel; kısa saç katmanı yüz bölgesini boş bırakıyor (doğru).
5. İlk `image_base64` çağrıları aktarımda bozuldu (PNG decode hatası); `source_image_id` ile çözüldü. Hata çağrıları faturalanmadı.
6. Taban ikinci deneme (+1 gen) tarifte açıkça yoktu; portre başına 2 aday kuralına benzetilerek yapıldı.
