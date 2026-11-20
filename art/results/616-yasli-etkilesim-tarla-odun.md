unity-import: pending

# 616 Yaşlılar: etkileşim kareleri 2 (hoe, sow, water, chop, work; 84 px)

- Araç: `animate_character`, `mode=pixminimax`, `frame_count=8`, `keep_first_frame=true`, başlangıç ve bitiş karesi = ilgili yönün depodaki durma karesi (`get_upload_url` ile Python'dan yüklendi; erkekte küreksiz sürüm: se `_v3`, sw/nw/ne `_v2`; kadında düz ad). Eylem metinleri tarifteki gibi, sonuna yaşlı duruşu eklendi. Çağrı başına 1 gen (aracın makbuzu `cost: 1 generation`, `cost_est` ile uyumlu).
- Toplam: 40 çağrı x 1 = **40 gen** (5 anahtar x 2 karakter x 4 yön). 10 eş zamanlı iş sınırı (diğer şeritlerle paylaşımlı) nedeniyle "10/10 used" ile reddedilen çağrılar ücretsiz, `wait_for_jobs` sonrası aynı çağrı yeniden gönderildi (retry istisnası). Hiçbir klip yeniden üretilmedi.
- Karakterler: elder_m_r84 `8dcaede5-cc4d-4068-b56f-ab5e327fea1c`, elder_f_r84 `b7f31e8d-29f2-4f6a-9092-a02759611159` (completed).
- `get_balance` sonrası: 918,6 kalan (diğer şeritler de harcıyor; bu işin payı 40).
- masaüstü kopyası: yerel
- İşleme: ham kare 84x108-152, tuval 84x84, ölçekleme yok. Her karenin en alt opak satırı y=81; klip başına tek yatay ofset (kare 0 kutu ortası = durma karesi kutu ortası); palet karakterin 4 durma karesinin renkleri, en yakın RGB, alfa ikili. Kare 0 ile son kare IoU > 0,95 ise son kare atıldı (8 kare), değilse 9 (erkek work SW, kadın sow SW: 9 kare). Pivot x=42, y=2 (ALTTAN). Betikler `art/tmp616/` (git yok sayar).
- Yan yana sayfası: `art/results/616-yanyana.png` (2 referans + 36 klibin 03. karesi). GIF üretilmedi.

## Teslim edilen dosyalar
`Assets/Art/Iso/_aday/anim/iso_char_<elder_m|elder_f>_r84_<hoe|sow|water|chop|work>_<se|sw|nw|ne>_<NN>.png`, **290 PNG**, hepsi 84x84, pivot x=42, pivot y=2 (alttan). 36 klip: 34 klip 8 kare, 2 klip 9 kare.

## Teslim EDİLMEYEN 4 klip (84x84'e sığmadı; içerik kırpılmadı)
| Klip | Taşan opak piksel | Ham boyut |
|---|---|---|
| elder_m water SE | 21 | 84x132 |
| elder_f chop SE | 791 | 120x152 |
| elder_f water SE | 101 | 120x132 |
| elder_f water SW | 60 | 84x132 |

Ham kareler PixelLab'de duruyor (animation_group): m water SE `c0e2de11-49d1-4fb4-8645-3c9412b429b7`, f chop SE `b91a0203-0006-45a6-873b-4cdb05c174ad`, f water SE `0ab2b0ad-1b9e-45d6-aa5d-5baad1497340`, f water SW `da4c0ed4-d9b9-4cef-9b2c-cb24a0f25d03`. Çözüm önerisi: bu 4 yön için tek yönlü yeniden üretim (4 gen; daha küçük hareket metni) ya da 96x96 tuval kararı (018c'deki chop sorunuyla aynı). Tek deneme kuralı gereği yeniden üretilmedi.

## Doğrulama
| Ölçüt | Değer |
|---|---|
| validate_iso (`--kind object --max-colors 32`, 290 dosya) | 290/290 geçti |
| style.py check karakter | GEÇTİ 212, UYARI 78, KALDI 0 (UYARI doygunluk/palet; aile kilidi kurulu değil). Ayrıntı yerelde `art/tmp616/stylecheck.txt` |
| Ayak satırı | her karede en alt opak satır y=81 |
| Kare 0 / son kare IoU | 0,921-1,0 (en düşük: erkek work SW 0,921, kadın sow SW 0,945; ikisi 9 kare) |

## Sorunlar ve notlar
- Kimlik (yüz, giysi), alet okunurluğu (çapa, balta, kova) ve yaşlı erkekte küreğin görünmemesi gözle yan yana sayfasından kontrol edilmeli; otomatik ölçülmedi.
- Kadın water SW'nin ham IoU'su 0,765 (kare 0 ile 8 farklı): taştığı için zaten teslim edilmedi.
- Bu çalışmada kare işlemede `numpy` kullanılmadı (saf Pillow); `tools/art` değişmedi.
- Arada PixelLab'de bir iş "blank image" ile hata verdi (ücretsiz); büyük olasılıkla başka şeride aitti, bu işin 40 klibi eksiksiz üretildi.
