unity-import: pending

# 573 Zincir pozları: yat, uyan, giyin, işine bak (84 px, erkek ve kadın)

**Özet:** 32 klip üretildi (32 gen). 12 klip teslim edildi (dress 7 yön-klibi, wake_stretch 4); 20 klip teslim edilmedi (lie_sleep 8, admire 8, wake_stretch 4, dress 1). Tek deneme kuralı gereği yeniden üretim yapılmadı. Kod pozu bulamayan anahtarlar eski simgeye düşer; `lie_sleep` ve `admire` için yeni iş gerekir (aşağıda öneri).

- Araç: `animate_character`, `mode=pixminimax`, `frame_count=8`, `keep_first_frame=true` (9 kare). Karakterler `man_r84` (c63fe39d-b168-4732-bf11-b476172c98e8) ve `woman_r84` (135cb0b1-bd46-4514-8c87-0834b96ef80d), ikisi `completed`, 8 rotasyon.
- `end_frame_url` = karakterin aynı yöndeki rotasyon görüntüsü (durma karesi; tek yön/çağrı gerektiği için 1 çağrı = 1 yön). Tek sapma: `lie_sleep`'te `end_frame` kullanılmadı, çünkü yatan figürün sonunu ayakta duruşa çekmek anlamsız; 4 yön tek çağrıda gönderildi.
- Maliyet: her çağrı 1 gen/yön (tahmin ile aynı). Toplam 32 gen (lie_sleep 2 çağrı x 4 yön = 8, diğerleri 24 çağrı x 1 yön). Yuva sınırı (10 eşzamanlı iş, hesap genelinde) yüzünden "need 1 job slots" hatası alan çağrılar bekleyip yeniden gönderildi, bu çağrılar faturalanmadı.
- `get_balance` sonrası: 1289,7 / 5000 (Tier 2). Diğer şeritler de harcadığı için bu sayı yalnızca bu işe ait değil.
- Masaüstü kopyası: yerel
- Yan yana sayfası: `art/results/573-yanyana.png` (2 referans + teslim edilen her klipten kare 00 ve 03). GIF: `art/results/573-<man|woman>-<dress|wake_stretch>.gif` (yönler yan yana, 2x). Reddedilen ham klip önizlemeleri: `art/results/573-red/573-onizleme-*-ham.png`.

## İşleme (018c ile aynı)
Ham kare 84x112 (bazı kliplerde 84x120-128); yalnızca kaydırma: en alt opak satır y=81'e çekildi, x kaydırma yok (ham genişlik 84), ölçekleme yok. Palet: `iso_char_<cinsiyet>_r84_cand_se.png` renkleri, en yakın RGB, alfa ikili (eşik 128). Kare 0 ile 8 IoU > 0,95 ise 8. kare atıldı (dress ve wake_stretch erkek NE, kadın SW: 8 kare; kadın wake_stretch NW ve NE IoU 0,82 / 0,87: 9 kare). Pivot x=42, y=2 (ALTTAN).

## Teslim edilen dosyalar
`Assets/Art/Iso/_aday/anim/iso_char_<man|woman>_r84_<anahtar>_<yön>_<NN>.png`, hepsi 84x84, pivot x=42, pivot y=2 (alttan). Toplam 90 PNG.

| Anahtar | Erkek | Kadın |
|---|---|---|
| dress | se 8, sw 8, nw 8, ne 8 | sw 8, nw 8, ne 8 (se YOK) |
| wake_stretch | ne 8 | sw 8, nw 9, ne 9 |
| lie_sleep | YOK | YOK |
| admire | YOK | YOK |

## Teslim edilmeyenler ve nedenleri (gözle, ham önizleme sayfalarından)
| Klip | Neden |
|---|---|
| lie_sleep, erkek SE, SW, NE; kadın SE, SW | karakter hiç yatmadı, ayakta nefes alıyor (poz tutmadı) |
| lie_sleep, erkek NW; kadın NW, NE | model yatağı karenin içine çizdi (yatak ayrı eşya, yatakla birlikte pişirilemez); kadın NW'de ayakta bir figür de var, kadın NE'de yatan figürle yatak karışık |
| admire, 8 klibin hepsi | belirgin hareket yok ve elde nesne yok: kare 0 ile ortadaki kareler neredeyse aynı (poz tutmadı) |
| wake_stretch, erkek SE | yatak karenin içine çizildi |
| wake_stretch, erkek SW, NW; kadın SE | poz tutmadı, ayakta kalıyor (kollar kalkmıyor) |
| dress, kadın SE | kollar baş üstüne kalkınca tepe tuvalden 3 satır taşıyor (84x84'e sığmadı); kırpmak içerik kaybı olurdu. Kaydırılmış hâli `art/results/573-red/iso_char_woman_r84_dress_se_*.png` |

Notlar: wake_stretch'te teslim edilen dört klipte karakter yatakta oturmuyor; "yatağın kenarında oturup gerinme" yerine ayakta kolları kaldırıp esneme/esneyen poz var (yatak yok, sade tutuldu). Kod oturumu yatağa göre konumlamak isterse bunu bilmeli. Kadın wake_stretch NW kare 04 civarında beden bükülüyor (kısa bir eğilme), kontrol edilmeli.

## Doğrulama
| Ölçüt | Değer |
|---|---|
| validate_iso (90 dosya, `--kind object`) | 90/90 geçti (84x84, <=32 renk, alt boşluk 2, yarı saydam 0) |
| style.py check karakter | KALDI 0, UYARI 11, GEÇTİ 79 |
| UYARI ayrıntısı | hepsi "ailesinden doygun" (x1,25-1,35): erkek dress SE kare 01, 06, 07; erkek dress SW kare 01-07; erkek wake_stretch NE kare 05. Palet yakınlığı %100. Aile kilidi kurulmadı (referans < 3), palet uyumu uyarı düzeyi. `fit` uygulanmadı (şekil sorunu yok, tüm UYARI'lar hafif). |
| Ayak satırı | her karede en alt opak satır y=81 (kaydırma ile) |
| Kare 0 - son kare IoU | dress 0,998-1,0; wake_stretch erkek NE 0,999, kadın SW 1,0, kadın NW 0,82, kadın NE 0,87 |
| Tepe kırpılması | teslim edilenlerde yok (en küçük tepe y: 0 erkek dress SE, kadın dress SW; diğerleri 1-5) |
| Gölge ve ışık | gölge pişirilmemiş, ışık sol-üstten (gözle yan yana sayfasından) |

## el_anchors (hand_anchors.txt için)
Ölçülmedi: teslim edilen kliplerde eller baş/göğüs hizasında hareket ediyor (dress, wake_stretch) ve elde taşınan nesne yok; admire (elde nesne klibi) teslim edilmediği için el çapası çıkarılmadı. Sonraki admire yeniden üretiminde ilk kare el konumu ölçülmeli.

## Sorunlar ve öneriler
- `lie_sleep` ve `admire` modelin pixminimax metin kipinde tutmadı; ikisi için yeniden üretim gerekiyor. Öneri (karar kullanıcı/kod oturumunda): (a) `skeleton-v3` özel poz ile `lie_down` ve `hold` hareketleri (yatağı karenin içine çizmez, ~3 gen/yön), ya da (b) `custom_start_frame` ile yatan poza (kendi çizdiğimiz tek kare, `pixelart_workbench` ile) başlatıp pixminimax; (c) uykuyu yatağın üstüne ayrı bir "yatan figür" katmanı olarak ayrı iş (karakter + yatak bütünü, `create_object_state`). Tarif metnindeki "lie_sleep yatay figürdür 96x84 dene" kısmına gerek kalmadı: model hiçbir yönde tek başına yatan figür üretmedi.
- Hesap genelindeki 10 iş yuvası sınırı, bu işin 32 çağrısını 6 partiye böldü (toplam ~15 dakika). Paralel şeritler aynı yuvaları paylaşıyor.
- Birden çok `admire` ve `dress` çağrısı ayrı grup kimliğiyle kaydedildi (grup kimliği vermeyi unuttuğum çağrılarda); klipler PixelLab hesabında duruyor, silinmedi.
- Aşağıdaki PixelLab klip kimlikleri (animasyon kimliği) `art/tmp573/manifest.py` içinde; commit edilmiyor, gerekirse tool-results'tan yeniden okunur.
