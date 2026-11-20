unity-import: pending

# 510 Easter egg NPC'leri: Johan Wick ve atsız şövalye yürüme, sevme, dağıtma

- Araç: `animate_character`, `mode=pixminimax`, `frame_count=8`, `keep_first_frame=true`, `end_frame_url` = ilgili yönün PixelLab döndürme görseli (rotations/<yön>.png; yerelde `get_upload_url` gerekmedi). Tek deneme, yön başına tek çağrı, hepsi 1 generation (makbuz). Toplam **24 gen** (tahmin 32, tavan 45). `get_balance` sonrası: 2267,7 (diğer şeritler aynı anda harcıyor, sayı yalnızca bu işe ait değil). Defter: `art/ledger/510.md`.
- Karakterler: johan_wick `8d5fc676-...`, horseless_knight `f3a16e27-...`; ikisi `completed`, 8 rotasyon, 84x84.
- Hız/yuva sınırı: iş yuvası 10/10 dolduğu için birçok çağrı "need 1 job slots" ile reddedildi (ücret yok); `wait_for_jobs` sonrası her biri bir kez yeniden gönderildi. Çift üretim yok.
- Ham çıktı 84-120 x 112-120 px, 9 kare. İşleme (018e kuralı): yalnızca kaydırma. Her karenin en alt opak satırı durma karesinin satırına çekildi; yatay ofset kare 0 ayak merkezinden (pet se -18, wreck se -6, wreck nw -18, diğerleri 0). Ölçekleme yok, alfa ikili, palet = karakterin 8 durma karesinin renkleri (en yakın RGB). Kare 0 ile son kare IoU > 0,95 ise son kare atıldı: şövalye ne (0,924) ve nw (0,922) 9 kare, diğerleri 8.
- Kırpılan opak piksel (84x84 dışına taşan, toplam): johan walk2 se 12, ne 70, nw 26; johan wreck sw 7; şövalye ne 17; diğerleri 0.
- Yan yana sayfası: `art/results/510-yanyana.png`. GIF'ler: `art/results/510-johan-walk2.gif`, `510-johan-pet.gif`, `510-johan-wreck.gif`, `510-sovalye-walk2.gif` (yönler yan yana, 2x, 110 ms; pet GIF'inde teslim edilmeyen yönler de var).
- masaüstü kopyası: yerel

## Teslim edilen dosyalar
Yol: `Assets/Art/Iso/_aday/anim/iso_char_<johan_wick|horseless_knight>_r84_<klip>_<yön>_<NN>.png`, hepsi 84x84, pivot x=42, pivot y=2 (alttan). Toplam 162 PNG.

| Klip | Yönler (kare sayısı) |
|---|---|
| johan_wick walk2 | e 8, ne 8, n 8, nw 8, w 8, s 8, se 8 (**sw yok**) |
| horseless_knight walk2 | e 8, ne 9, n 8, nw 9, w 8, sw 8, s 8, se 8 |
| johan_wick pet | **yalnızca ne 8** |
| johan_wick wreck | se 8, sw 8, nw 8, ne 8 |

## Teslim EDİLMEYEN (art/results/510-red/, 32 PNG)
| Klip | Neden |
|---|---|
| johan walk2 sw | Kafa farkı 4 px (ölçüt ≤ 3). Diğer yürüme yönleri 1-3 |
| johan pet se | PixelLab kare içine köpek çizdi (iş dosyası: köpek çizilmez, oyunda ayrı görsel). Eğilme ve sevme okunuyor, yalnız köpek var |
| johan pet sw | Hareketsiz: 8 karenin hepsi aynı (IoU 1,00) |
| johan pet nw | Gövde belirgin eğilmiyor (kafa farkı 4 px); yalnızca küçük bir el görünüyor |

Kod `IsoArt.SetAction`'da yön yoksa `se`'ye düşüyor; pet için se yok, yalnızca ne var. Johan walk2 sw eksik. Kullanıcı kararı: red klasöründeki kareleri yine de kabul etmek (köpekli se ve sw yürüme borderline) ya da yeniden üretim işi açmak. Pet için neredeyse 3 yön yeniden üretilmeli (prompt'a "no dog drawn, hand held out low" eklemek önerilir).

## Doğrulama
| Ölçüt | Değer |
|---|---|
| validate_iso (`--kind object --max-colors 32`), 194 dosya | 194/194 geçti |
| style.py check karakter | KALDI 0, UYARI 0, GEÇTİ 194 (doygunluk x1,0-1,2, palet yakınlığı %100). Tablo: `art/results/510-yanyana.png` (referanslarla) |
| Palet dışı oran | %0 |
| Ayak satırı | durma karesinin en alt satırı (79-82, yöne göre) her karede sabit |
| Johan walk2 kafa farkı (en üst satır max-min) | e 2, ne 1, n 1, nw 2, w 2, s 2, se 3, sw 4 (kaldı) |
| Şövalye walk2 kafa farkı (sekme isteniyor, ölçüt yok) | e 4, ne 5, n 1, nw 4, w 3, s 2, se 4, sw 4 |
| Ardışık kare IoU en düşük | johan walk2 0,82-0,93; şövalye 0,80-0,86; wreck 0,71-0,83; pet ne 0,79 |
| Tepe kırpılması | yukarıda; ne yürüyüşte 70 px (toplam) |

## Sorunlar ve notlar
- **Kimlik:** yüz, kapüşonlu gri pelerin, sırt çantası ve şövalyenin miğferi/sürkotu yan yana sayfasında korunmuş görünüyor; otomatik ölçüm yok.
- **wreck:** ayak satırı sabit kuralı gereği tekme sırasında ayak zeminde kalıyor (yükselmiyor), hareket gövde, pelerin ve yürüyüş adımıyla okunuyor. sw ve nw karelerinde ayak çevresinde küçük dağınık pikseller (PixelLab'in çizdiği ot/toz parçası) var; kullanıcı GIF'ten teyit etsin. Ekin çizilmedi ama bu serpintiler tam temiz değil.
- **pet ne:** sırt dönük; gövde 19 px alçalıyor, sağda küçük bir el okunuyor.
- **Şövalye:** at yok, yumruklar önde, sekme kafa farkı 1-5 px; GIF'ten kullanıcı bakar.
- **Öneri:** pet ve yürüme sw için ikinci tur; `tools/art` altında klip işleme betiği olsaydı yeniden yazmak gerekmezdi (betikler `art/tmp` altında kaldı).
