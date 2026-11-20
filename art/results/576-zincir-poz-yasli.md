unity-import: pending

# 576 Zincir pozları, yaşlılar: kısmi teslim, yalnızca `wash` (8/32 klip)

**Özet:** 32 klip üretildi (32 gen). `wash` 8 klip (2 karakter x 4 yön, 65 PNG) teslim edildi. `sit`, `sit_eat` ve `lie_sleep` (24 klip) **teslim edilmedi**: pixminimax, başlangıç ve bitiş karesi olarak durma karesi verilince karakteri ayakta bıraktı. Yeniden denenmedi (tek deneme kuralı).

- Araç: `animate_character`, `mode=pixminimax`, `frame_count=8`, `keep_first_frame=true`, yön başına tek çağrı. `custom_start_frame_url` ve `end_frame_url` = yönün durma karesi (015a ile aynı: elder_m `_v3` se ve `_v2` sw/nw/ne yani küreksiz, elder_f düz `idle`). Özel kare tek yön kabul ettiği için 32 ayrı çağrı. Eylem metinleri iş dosyasındakiyle aynen.
- Karakterler: elder_m `8dcaede5-cc4d-4068-b56f-ab5e327fea1c`, elder_f `b7f31e8d-29f2-4f6a-9092-a02759611159`.
- Maliyet: çağrı başına 1 gen (tahmin 32, tavan 44), toplam **32 gen**. `get_balance` sonrası: **1289,7** (hesap geneli, paralel şeritler dahil; iş öncesi 1377,7 okundu, farkın 88'i başka şeritlerin).
- Eşzamanlılık: 10/10 iş yuvası dolunca 17 çağrı reddedildi (ücret yok), yuva açılınca aynı çağrılar yeniden gönderildi (hız sınırı istisnası).
- Masaüstü kopyası: yerel
- Ham kareler PixelLab'de (indirme: `art/tmp576`, commit edilmedi). Gruplar `get_character` çıktısında; `sit`/`sit_eat`/`lie_sleep`/`wash` adlı animasyonlar.

## Neden 24 klip teslim edilmedi
Ölçüm (her klip, 9 ham kare): `sit`, `sit_eat`, `lie_sleep` kliplerinde opak kutunun tepesi ve tabanı kareler boyunca sabit (tepe 16-18, alt 92-96; oturan ya da yatan karakterde tepe aşağı inmeli, yatanda kutu yataya yayılmalı). Gözle bakıldığında:
- `sit`: karakter ayakta, küçük omuz oynaması dışında hareket yok.
- `lie_sleep`: karakter ayakta, yatış yok; nefes alma bile seçilmiyor.
- `sit_eat`: karakter ayakta kaseyi tutup kaşıkla yiyor (yeme hareketi okunuyor, ama oturma yok ve kase küçük değil, parlak turuncu). `sit_eat` adıyla teslim edilirse kod oturma varsayacağı için yanlış poz çıkar; teslim edilmedi. İsterseniz ayakta yeme klibi olarak ayrıca değerlendirilebilir (işlenmemiş ham, kareler `art/results/576-red/` altında).
- Kök neden büyük olasılıkla: başlangıç ve bitiş karesi ayakta duruş olduğu için model hedef pozu bırakmıyor (`wash` bundan az etkilendi çünkü eğilme ayakta gövdeyle yapılabiliyor).
- Önizleme sayfaları (kare 0, 2, 4, 6, 8; 4 yön): `art/results/576-red/{elder_m,elder_f}_{sit,sit_eat,lie_sleep}_kareler-0-2-4-6-8.png`.

## Teslim edilen: `wash`
İşleme (015a ile aynı yöntem, ücretsiz): PixelLab 84x108/112 tuval verdi; kare 0 durma karesiyle aynı kutuya oturacak ofsetle (yalnızca kaydırma, ölçekleme yok) tüm kareler 84x84'e maskeyle kırpıldı (kırpılan opak piksel: 0). Palet: yönün durma karesinin renkleri (31-32), en yakın RGB; alfa ikili. Karedeki mavi su sıçraması durma paletinde karşılığı olmadığı için gri tonuna düştü (su damlası okunmuyor, eğilme ve el yüze hareketi okunuyor). Kare 08, 00'a IoU > 0,93 ise atıldı (7/8 klipte; elder_m NE'de IoU 0,825 olduğu için 9 kare 00-08 teslim edildi, döngü o yönde sert kapanır).

Dosyalar (`Assets/Art/Iso/_aday/anim/`, 84x84, pivot x=42, pivot y ALTTAN = yönün durma karesinin ayak boşluğu):
| Yol | Kare | Pivot y (alttan) |
|---|---|---|
| iso_char_elder_m_r84_wash_se_00..07 | 8 | 4 |
| iso_char_elder_m_r84_wash_sw_00..07 | 8 | 3 |
| iso_char_elder_m_r84_wash_nw_00..07 | 8 | 4 |
| iso_char_elder_m_r84_wash_ne_00..08 | 9 | 4 |
| iso_char_elder_f_r84_wash_se_00..07 | 8 | 2 |
| iso_char_elder_f_r84_wash_sw_00..07 | 8 | 2 |
| iso_char_elder_f_r84_wash_nw_00..07 | 8 | 4 |
| iso_char_elder_f_r84_wash_ne_00..07 | 8 | 4 |

Toplam 65 PNG. Pivot y iş dosyasındaki (42, 2) yerine 015a gibi durma karesinin ayak boşluğu esas alındı.

## Doğrulama
| Ölçüt | Sonuç |
|---|---|
| validate_iso (nesne, 32 renk) | 65/65 geçti |
| style.py check karakter | 0 KALDI, 11 UYARI (hepsi "ailesinden soluk": elder_f NW x0,38-0,56 (kare 00-07; yaşlı kadının NW durma karesi soluk), elder_f NE 00 x0,58, elder_m NE 08 x0,60, elder_m SW 04 x0,59), 54 GEÇTİ. Aile kilidi kurulmadığından uyarı düzeyinde; `fit` uygulanmadı. Soluk görünüm yaşlı paletinden (durma karesiyle aynı renkler) |
| Ayak satırı | her klipte sabit (tüm karelerde aynı alt satır: elder_m SE 80, SW 81, NW 80, NE 80; elder_f SE 82, SW 82, NW 80, NE 80) |
| Baş tepesi (eğilme) | elder_m se 4-9, sw 4-7, nw 3-8, ne 3-13; elder_f se 4-14, sw 4-9, nw 2-9, ne 2-7; kırpılma yok |
| Giysi ve saç kimliği | durma karesiyle aynı palet; yüz, giysi gözle aynı (kare sayfasında bakıldı) |
| Kürek / alet | elder_m'de kürek yok (küreksiz `_v2`/`_v3` başlangıç karesi), elde nesne yok |
| El konumu (`hand_anchors.txt`) | `wash` nesne taşımaz: el çapası gerekmiyor. `sit_eat` teslim edilmediği için kaşık/kase el konumu da yok |

## Görseller
- Yan yana (referanslar + kare 04, 8 klip): art/results/576-yanyana.png
- GIF (110 ms, 4 yön yan yana): art/results/576-wash-elder_m.gif, art/results/576-wash-elder_f.gif

## Sorunlar / öneri
- Teslim edilmeyen: elder_m ve elder_f x `sit`, `sit_eat`, `lie_sleep` x 4 yön = 24 klip. Kodda bu pozlar için en yakın poz ya da eski simge sürer (iş notu).
- Yeni deneme için öneriler (kullanıcı kararı; bu işçi aynı yöntemle ikinci deneme yapmadı):
  1. Başlangıç karesi olarak durma karesi yerine oturma/yatma pozunun bir kılavuz karesi (örn. `create_object_state` ya da elle çizilmiş taslak) verip `end_frame` vermemek; yalnızca `custom_start_frame` kullanmak.
  2. `skeleton-v3` ile eylem pozlarını elle yazmak (oturma ve yatma için eklem pozları), ya da `create_character_state` ile "oturan" ve "yatan" durum karakterleri üretip onları 8 kareli nefes/yeme klibine çevirmek.
  3. Sofra ve yatak nesnesiyle birlikte çizilmiş poz istenirse (taburede oturan yaşlı), bu bir karakter + eşya birleşik sprite işidir; tek karakter animasyonu aracı bunun için yetersiz kalıyor.
