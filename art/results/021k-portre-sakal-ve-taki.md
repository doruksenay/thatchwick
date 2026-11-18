unity-import: pending

# 021k Portre sakal ve takı: sonuç

Durum: kısmi. 11/12 dosya stil denetiminden geçti; 10 yeni dosya teslim edildi, 1 dosya zaten vardı (atlandı), 1 dosya KALDI (teslim edilmedi).
Masaüstü kopyası: yerel (bulut işçisi, atlandı).

## Araç ve ayarlar
`inpaint_image_pro_flash`, `Modify current layer, only changes`, 64x64, 5 gen/çağrı, seed 0, tek deneme. Taban `portrait_base_oval_adult_64.png` (`get_upload_url`, `image_url=upload:...`). Maskeler betikle, saf siyah/beyaz:
- sakal: (21,33)-(43,47)
- küpe: iki kulak (15,24)-(22,34) ve (42,24)-(49,34)
- burun halkası: (27,29)-(38,35)
- ip kolye: (22,43)-(43,52)
Açıklama: `<ad> beard, neutral grey, will be recoloured` + ortak ek; takılar gri değil kendi renginde (metal/ip).
Hız sınırı: 8 eşzamanlı iş sınırı (Tier 1, diğer şeritler slotları doldurdu). 6 çağrı reddedildi (faturalanmadı), slot açılınca bir kez yeniden gönderildi.

## Harcama
12 x 5 = **60 gen** (cost_max 60). `reduce_colors` gerekmedi (en çok 24 renk). `get_balance` sonrası: 1373,7 (paralel şeritler dahil). Defter: `art/ledger/021k.md`.

## Dosyalar (portre, pivot yok; 64x64, yarı saydam piksel 0)
| Yol (Assets/Art/Iso/_aday/) | Renk | Stil | Durum |
|---|---|---|---|
| portrait_layer_beard_stubble_64.png | 24 | UYARI (palete uzak) | teslim |
| portrait_layer_beard_short_64.png | 19 | UYARI (palete uzak) | **teslim edilmedi: 020'den aynı adlı dosya var (`cp -n`)**; yeni sürüm `art/results/021k-red/portrait_layer_beard_short_64_yeni.png` |
| portrait_layer_beard_full_64.png | 20 | UYARI (palete uzak) | teslim |
| portrait_layer_beard_long_64.png | 21 | UYARI (soluk x0.43) | teslim |
| portrait_layer_beard_forked_64.png | 24 | UYARI (palete uzak) | teslim |
| portrait_layer_beard_goatee_64.png | 23 | UYARI (palete uzak) | teslim |
| portrait_layer_earring_1_64.png | 13 | UYARI (doygun x1.35) | teslim |
| portrait_layer_earring_2_64.png | 13 | UYARI (palete uzak, doygun) | teslim |
| portrait_layer_earring_3_64.png | 15 | UYARI (palete uzak) | teslim |
| portrait_layer_earring_4_64.png | 11 | UYARI (palete uzak, doygun) | teslim |
| portrait_layer_necklace_cord_64.png | 15 | UYARI (palete uzak) | teslim |
| portrait_layer_nosering_1_64.png | 17 | **KALDI** (palete uzak, çok doygun x1.61) | teslim edilmedi: `art/results/021k-red/` |

Yan yana: `art/results/021k-yanyana.png`. Taban birleşim önizlemesi (x3; sıra: forked, full, goatee, long, short, stubble, küpe 1-4, kolye, burun halkası): `art/results/021k-birlesim-x3.png`.

## Doğrulama
- Hepsi 64x64, yarı saydam piksel 0, renk ≤ 24 (≤ 32). `validate_iso.py` geçti (yalnızca boyut/alfa anlamlı).
- Maske dışı piksel: 0 (12/12).
- Birleşim: sakallar tabana temiz oturuyor, kayma ya da çift dış hat yok; long ve forked belirgin, short/stubble/goatee kısa ve okunur.
- Döşeme testi: portre katmanı, uygulanmaz.

## Sorunlar
1. **Takılar zayıf okunuyor:** küpe 1 ve burun halkası x3 önizlemede neredeyse görünmez; küpe 2-3 küçük ama seçilir; ip kolye yakanın üstünde güçlükle ayırt ediliyor (yaka ile aynı bej/kahve). Kullanıcı gözüyle bakmalı; gerekirse daha dar maske ve daha kontrastlı renk adı (ör. `bright gold`) ile yeniden iş.
2. **Burun halkası KALDI** (doygunluk x1.61), teslim edilmedi.
3. **beard_short** adı 020'de teslim edilmiş dosyayla çakıştı; üzerine yazılmadı. Yeni sürüm `021k-red/` altında; kullanıcı hangisinin kalacağına karar verir.
4. Sakal maskesi tek parça; tabandaki ağız/çene çizgisi maske içinde yeniden çizildi (sakal çıktıları cildi de kapsıyor). Birleşimde sorun görünmedi.
5. Sakal renkleri nötr gri (yeniden renklendirilecek), stil UYARI'ları bu yüzden "palete uzak"/"soluk".
