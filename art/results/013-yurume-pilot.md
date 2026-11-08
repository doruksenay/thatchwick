unity-import: pending

# 013 Yürüme animasyonu pilotu (farmer_a2_56, SE, 4 yöntem)

- Tarih: 2026-11-08
- Karakter: `farmer_a2_56`, PixelLab `37cebe59-b761-4dd8-b23c-b11f60f58294` (60x60, 8 yön)
- Yön: yalnızca south-east. Aynalama yok.
- Durum: `review`. Yöntem seçimi kullanıcıda.

## Ön kontrol (ücretsiz)
`get_character` sonucu:
- Durum `completed`, 8 rotasyon var, başlangıçta animasyon yoktu.
- Şablon listesinde `walk` var. Dört yöntemin dördü de çalıştırıldı.
- PixelLab'deki SE rotasyonu `_ham/iso_char_farmer_a2_56_cand_se.png` ile bayt bayt aynı (cmp).

## Çağrılar ve maliyet
Her çağrıda yanıttaki maliyet satırı tarifteki tahminle aynıydı. Hiçbir çağrı reddedilmedi, yeniden deneme yapılmadı.

| # | Yöntem | Ayarlar | Grup | Maliyet satırı | Çağrı öncesi harcanan / bakiye |
|---|---|---|---|---|---|
| 1 | template | `template_animation_id=walk`, `ai_freedom=0`, `walk_pilot_template` | 6856eb7a-6b93-4fc3-a8ca-f8dbf9078841 | 1 | 0 / 1873,8 |
| 2 | v3 | `frame_count=6`, `keep_first_frame=true`, end_frame = SE ham kare (`upload:69329442-...`), `walk_pilot_v3` | 815f98c0-26cd-45a1-9b4b-29864abb94b0 | 1 (tuval 60x84) | 1 / ~1872,8 |
| 3 | skeleton-v3 | `template_animation_id=walk`, `walk_pilot_skel` | b5d485cb-7bce-4792-b242-4d0a6df74d6a | 3 | 2 / ~1871,8 |
| 4 | pixminimax | `frame_count=8`, `keep_first_frame=true`, aynı end_frame, `walk_pilot_pmm` | 05bc8ebd-6828-44d7-ad82-60df91395daa | 1 (tuval 60x84) | 5 / ~1868,8 |

- Toplam: **6 gen** (cost_max 6).
- `get_balance` sonrası: **1867,8**.
- end_frame dosyası `get_upload_url` ile yüklendi (`upload:` ref verildi, base64 kullanılmadı).
- Kareleri alma yolu: `get_character` yanıtındaki backblaze kare bağlantıları (`.../animations/<id>/south-east/{i}.png`), curl ile, ücretsiz. ZIP ya da workbench kullanılmadı.

## Kare 00
- **v3 ve pmm:** 00 karesi referans karedir. Opak kırpığı durma karesiyle piksel piksel aynı.
- **tmpl ve skel:** 00 karesi aracın ürettiği ilk karedir, durma karesi değildir.
  - tmpl 00: 24 px geniş. Durma karesi 32 px geniş; kese kaybolmuş.
  - skel 00: 28 px geniş.

## İşleme
Betik: `tools/art/anim_pilot.py` (yeni), `uv run --with pillow`. Aşağıdaki adımlar her karede ölçeklemeden yapıldı.
1. **60x60 tuval.** Yatayda yöntem başına tek ofset kullanıldı: birleşik opak kutunun ortası, durma karesinin opak kutu ortasına geldi. Ofsetler: tmpl -1, v3 0, skel -22, pmm 0.
2. **Ayak satırı.** Her karenin en alt opak satırı, durma karesinin ayak satırına (y=58) kaydırıldı. Yalnızca tamsayı dikey kaydırma.
3. **Palet.** 32 renk, `iso_char_farmer_a2_56_cand_*.png` dosyalarının birleşimi. Her piksel en yakın RGB renge eşlendi. Alfa ikili: a≥128 ise 255, aksi 0. `reduce_colors` kullanılmadı.

## Ölçümler
- Durma karesi: boy 58, ayak satırı 58, 32 px geniş.
- IoU, ortak tuvalde opak maskeyle hesaplandı.
- Bacak bölgesi: opak kutunun alt %35'i. Palet dışı oran, 32 renkli paletin dışında kalan piksel oranı (ham karelerde).

| Ölçüm | tmpl | v3 | skel | pmm |
|---|---|---|---|---|
| Ham tuval | 60x60 | 60x84 | 96x96 | 60x88 |
| Kare sayısı | 6 | 7 (00 = referans) | 6 | 9 (00 = referans) |
| Boy, ham (kare başına) | 57,57,56,57,57,57 | 58,57,56,57,58,58,58 | 58,57,57,58,61,60 | 58,56,57,57,57,59,60,60,58 |
| Boy aralığı ham → işlenmiş | 56-57 → 56-57 | 56-58 → 56-58 | 57-61 → 57-59 (tepe kırpıldı) | 56-60 → 56-59 (tepe kırpıldı) |
| Ayak satırı aralığı, hizalamadan önce | 58-59 | 68-71 | 75-79 | 70-74 |
| Ardışık IoU min / ort (ham) | 0,68 / 0,76 | 0,76 / 0,86 | 0,66 / 0,75 | 0,72 / 0,78 |
| Ardışık IoU min / ort (işlenmiş) | 0,66 / 0,76 | 0,76 / 0,85 | 0,65 / 0,72 | 0,70 / 0,78 |
| Döngü kapanış IoU, son↔ilk (ham / işl) | 0,74 / 0,74 | 1,00 / 1,00 | 0,75 / 0,68 | 1,00 / 1,00 |
| Bacak bölgesi renk sayısı, ham | 38-43 | 35-40 | 23-39 | 27-39 |
| Bacak palet dışı oran, ham (ort; aralık) | 0,38 (0,35-0,43) | 0,29 (0,25-0,41) | 0,41 (0,36-0,49) | 0,29 (0,23-0,34) |
| Renk ham → işlenmiş (toplam) | 55 → 32 | 61 → 32 | 54 → 32 | 57 → 32 |
| Yarı saydam ham / işl | 0 / 0 | 0 / 0 | 0 / 0 | 0 / 0 |
| validate_iso (işlenmiş) | 6/6 geçti | 7/7 geçti | 6/6 geçti | 9/9 geçti |

Ardışık IoU dizileri (işlenmiş):
- tmpl: 0,72 0,83 0,66 0,71 0,87
- v3: 0,86 0,81 0,86 0,91 0,88 0,76
- skel: 0,71 0,84 0,65 0,65 0,76
- pmm: 0,71 0,82 0,77 0,78 0,75 0,82 0,85 0,70

Kapanış IoU'su 1,00 olan v3 ve pmm'de son kare durma pozuyla aynı silüete dönüyor. Döngüde 00 ve son kare art arda gelince aynı poz iki kez gösterilir ve dikişte takılma olur. Döngüde birinin atılması gerekir (aşağıda öneri).

## Dosyalar
Hepsi 60x60. Pivot y = 1 (ALTTAN), ayak satırı 58.

Pivot x, `char_preview.py` yöntemiyle kare başına ölçüldü:
- Aralık 27,0-35,5. Durma karesinde 31,5.
- Animasyonda tek pivot kullanılacaksa durma karesinin (31,5; 1) değeri önerilir. Kareler bu değere göre hizalı yazıldı.

| Yol | Adet |
|---|---|
| `Assets/Art/Iso/_aday/anim/_ham/iso_char_farmer_a2_56_walk_tmpl_se_00..05.png` | 6 (60x60, ham) |
| `Assets/Art/Iso/_aday/anim/_ham/iso_char_farmer_a2_56_walk_v3_se_00..06.png` | 7 (60x84, ham) |
| `Assets/Art/Iso/_aday/anim/_ham/iso_char_farmer_a2_56_walk_skel_se_00..05.png` | 6 (96x96, ham) |
| `Assets/Art/Iso/_aday/anim/_ham/iso_char_farmer_a2_56_walk_pmm_se_00..08.png` | 9 (60x88, ham) |
| `Assets/Art/Iso/_aday/anim/iso_char_farmer_a2_56_walk_<yöntem>_se_NN.png` | 28 (60x60, işlenmiş) |
| `art/results/013-yurume-pilot-karsilastirma.png` | Satır sırası tmpl, v3, skel, pmm. Her satır köylü, durma karesi, işlenmiş kareler; 4x |
| `art/results/013-yurume-{tmpl,v3,skel,pmm}-x4.gif` | 4 (köylü, durma karesi, animasyon; sonsuz döngü) |
| `tools/art/anim_pilot.py` | İşleme, ölçüm ve önizleme betiği |

Masaüstü kopyası:
- `C:\Users\User\OneDrive\Desktop\art\karakter\2026-11-08_yurume_pilot\` içinde `ham\` (28 dosya), `islenmis\` (28 dosya), karşılaştırma görseli ve 4 GIF var.
- `_indeks.md` dosyasına 4 satır eklendi.

## Gözle notlar
**tmpl**
- Bacak lekesi: adımlar okunur; bacaklarda hafif renk karışması var.
- Kol: 04-05'te belirgin sallanma, diğer karelerde gövdeye yapışık.
- Adım okunurluğu: iyi.
- Kimlik: **zayıf.** 01-03'te gövde daralmış (24-26 px; durma karesi 32 px), kese ve kayış kaybolup geri geliyor. Tunik tonu ve yüz yakın.

**v3**
- Bacak lekesi: en temiz bacaklar.
- Kol: çok az hareket ediyor.
- Adım okunurluğu: **zayıf.** Adım çok kısa, karakter yerinde ağırlık aktarıyor gibi duruyor.
- Kimlik: **en iyi.** Kese, kayış, yüz ve tunik bütün karelerde durma karesiyle aynı. Silüet en sakin (ort IoU 0,85).

**skel**
- Bacak lekesi: adımlar en geniş ve açık. 05 karesinde ayak çevresinde başıboş birkaç piksel var. Bacak palet dışı oranı en yüksek (0,41).
- Kol: 04-05'te geniş sallanma.
- Adım okunurluğu: iyi.
- Kimlik: iyi ama tam değil. 00 karesi bile durma karesinden farklı (28 px), kese 04-05'te küçülüyor.
- Silüet IoU'su en düşük (min 0,65). Boy 57-61; 04-05'te tepe 2 satır kırpıldı (60x60 tuvale sığmadı).

**pmm**
- Bacak lekesi: az; bacak palet dışı oranı v3 ile eşit en düşük (0,29).
- Kol: hafif ve doğal sallanma.
- Adım okunurluğu: iyi. 8 kare akıcı, tam adım çevrimi.
- Kimlik: iyi. Kese, kayış ve yüz korunuyor.
- Boy 56-60; 05-07'de tepe 1 satır kırpıldı.

## Öneri (karar kullanıcıda)
**pixminimax** öne çıkıyor:
- 1 gen/yön.
- Kimlik v3'e yakın, adım okunurluğu tmpl/skel düzeyinde.
- Bacak lekesi az, kapanış uygun.
- Kullanırken son kare (08, durma pozuna dönüş) döngüden atılmalı; 00-07 ile 8 karelik döngü olur.

İkinci aday skeleton-v3: adım en belirgin, ama 3 gen/yön, silüet sıçraması en yüksek ve boy 60x60'a sığmıyor. Template'te kimlik kayması (kese kaybı) var. v3 temiz ama yürüme okunmuyor.

## Sorunlar
- **60x60 tuvalde tepe kırpılması.** Ayak satırı 58'e hizalanınca 59 satırdan uzun kareler tepeden kırpılıyor:
  - skel 04-05: 2 satır, toplam 18 piksel saç.
  - pmm 05-07: 1 satır, toplam 10 piksel.
  - Tarif gereği tuval büyütülmedi. Tam set için tuvalin 64 olması ya da ayak satırının değişmesi önerilir; karar kullanıcıda.
- **GIF süresi.** GIF zamanlaması 10 ms çözünürlüklü; Pillow 125 ms'yi 120 ms olarak yazdı (~8,3 fps).
- **Önizlemede yatay oynama.** Önizleme ve GIF'te `char_preview.py` mantığıyla her kare kendi ayak bandı pivotuna göre ortalanıyor. Unity'de tek pivotla gösterimde yatay oynama bundan farklı görünebilir.
- **Bacak ölçümü.** Bacak bölgesi ölçümü opak kutunun alt %35'i olduğundan sallanan kolun alt kısmını da içerebilir.
