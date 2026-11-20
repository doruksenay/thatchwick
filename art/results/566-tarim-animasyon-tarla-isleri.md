unity-import: pending

# 566 Tarım animasyonları 2/2: kalbur, sırıkla silkeleme, saban, gübre serpme (84 px)

- Karakterler: `man_r84` (c63fe39d-b168-4732-bf11-b476172c98e8), `woman_r84` (135cb0b1-bd46-4514-8c87-0834b96ef80d), 84x84, 8 yön, `completed`.
- Yöntem: `animate_character`, `mode=pixminimax`, `frame_count=8`, `keep_first_frame=true`, `end_frame_url` = ilgili yönün rotasyon karesi (kendi yönü; `end_frame` yalnızca tek yönde verilebildiği için 32 tek yönlü çağrı). Maliyet çağrıdan önce okundu: 1 gen/yön (cost_est 32 ile aynı). Toplam **32 gen**, tek deneme.
- Eşzamanlılık: 10 iş yuvası başka şeritlerle paylaşıldığından "need 1 job slots but only 0 available" hatası veren çağrılar (üretim başlamadan reddedildi, harcama yok) yuva boşalınca bir kez daha gönderildi; hiçbir klip iki kez üretilmedi.
- `get_balance` sonrası kalan: 1384,7 (diğer şeritler de harcıyor, sayı bu işe ait değil).
- İşleme (018c ile aynı): ham kare 84x112 (bazı kliplerde 88-120 geniş); yalnızca kaydırma: her karenin en alt opak satırı y=81, yatay ortalama; ölçekleme yok. Palet: `iso_char_<cinsiyet>_r84_cand_se.png` renkleri, en yakın RGB, alfa ikili. Kare 0 ile 8 IoU > 0,95 ise son kare atıldı (8 kare), değilse 9 kare. Pivot x=42, y=2 (ALTTAN).
- Yan yana sayfası: `art/results/566-yanyana.png`. GIF'ler: `art/results/566-<man|woman>-<anahtar>.gif` (yönler yan yana, 2x).
- masaüstü kopyası: yerel

## Teslim edilen dosyalar
`Assets/Art/Iso/_aday/anim/iso_char_<man|woman>_r84_<anahtar>_<se|sw|nw|ne>_<NN>.png`, hepsi 84x84, pivot x=42, pivot y=2 (alttan). Toplam 229 PNG (28 klip).

## Teslim edilmeyenler (84x84'e sığmadı)
4 klip tuvalin üstünden taşıyor (uzun sırık/yukarı kalkan kol); kırpmak içerik kesmek olurdu (kural: içerik kırpma yok). Kaydırılmış hâlleri `art/results/566-red/` altında:
- man shake_pole se (76 px taşma), man shake_pole sw (73 px), woman shake_pole sw (53 px), woman spread se (32 px).
- Öneri: bu 4 yön için tek yönlü yeniden üretim (daha kısa sırık/salınım) ya da 96x96 tuval kararı. Tek deneme kuralı gereği yeniden üretilmedi. Kod bu yönlerde çapa pozuna düşer.

## Klip tablosu
| Cinsiyet | Anahtar | Yön | Kare | Ham genişlik | Kare0-son IoU | En küçük tepe y | Durum |
|---|---|---|---|---|---|---|---|
| man | plough | ne | 8 | 84 | 0.999 | 5 | teslim |
| man | plough | nw | 8 | 84 | 1.0 | 5 | teslim |
| man | plough | se | 8 | 84 | 0.998 | 4 | teslim |
| man | plough | sw | 8 | 84 | 0.998 | 4 | teslim |
| man | shake_pole | ne | 8 | 104 | 0.999 | 5 | teslim |
| man | shake_pole | nw | 8 | 84 | 0.975 | 2 | teslim |
| man | shake_pole | se | 9 | 88 | 0.843 | 0 | TESLİM EDİLMEDİ (76 px taşma) |
| man | shake_pole | sw | 8 | 84 | 0.998 | 0 | TESLİM EDİLMEDİ (73 px taşma) |
| man | sieve | ne | 8 | 84 | 0.968 | 5 | teslim |
| man | sieve | nw | 9 | 84 | 0.935 | 5 | teslim |
| man | sieve | se | 8 | 84 | 0.999 | 4 | teslim |
| man | sieve | sw | 9 | 84 | 0.909 | 4 | teslim |
| man | spread | ne | 8 | 120 | 0.993 | 5 | teslim |
| man | spread | nw | 8 | 120 | 1.0 | 5 | teslim |
| man | spread | se | 8 | 96 | 1.0 | 3 | teslim |
| man | spread | sw | 8 | 100 | 0.999 | 2 | teslim |
| woman | plough | ne | 8 | 84 | 1.0 | 4 | teslim |
| woman | plough | nw | 8 | 84 | 1.0 | 5 | teslim |
| woman | plough | se | 8 | 84 | 1.0 | 2 | teslim |
| woman | plough | sw | 8 | 84 | 1.0 | 2 | teslim |
| woman | shake_pole | ne | 8 | 84 | 0.998 | 4 | teslim |
| woman | shake_pole | nw | 9 | 84 | 0.909 | 5 | teslim |
| woman | shake_pole | se | 8 | 84 | 1.0 | 0 | teslim |
| woman | shake_pole | sw | 8 | 88 | 0.959 | 0 | TESLİM EDİLMEDİ (53 px taşma) |
| woman | sieve | ne | 8 | 84 | 0.999 | 4 | teslim |
| woman | sieve | nw | 8 | 84 | 0.993 | 5 | teslim |
| woman | sieve | se | 9 | 84 | 0.868 | 2 | teslim |
| woman | sieve | sw | 9 | 84 | 0.937 | 2 | teslim |
| woman | spread | ne | 8 | 84 | 0.971 | 4 | teslim |
| woman | spread | nw | 8 | 120 | 1.0 | 5 | teslim |
| woman | spread | se | 9 | 84 | 0.805 | 0 | TESLİM EDİLMEDİ (32 px taşma) |
| woman | spread | sw | 8 | 116 | 1.0 | 2 | teslim |

## Doğrulama
| Ölçüt | Değer |
|---|---|
| validate_iso (229 dosya, --kind object) | 229/229 geçti |
| style.py check karakter | KALDI 0, UYARI 55, GEÇTİ 174. UYARI ailesinden doygunluk ağırlıklı; palet yakınlığı %100 (palet cand_se renkleri). Aile kilidi kurulmadı, palet uyumu uyarı düzeyi. |
| Ayak satırı | her karede en alt opak satır y=81 |
| Tepe kırpılması | teslim edilenlerde yok |

## Sorunlar ve notlar
- Elde tutulan alet okunurluğu (kalbur, sırık, saban kolları, sepet) ve kimlik kayması otomatik ölçülmedi; yan yana sayfasından ve GIF'lerden gözle bakılmalı. Saban klipleri yalnızca yürüme pozu verebilir (saban nesnesi PixelLab karakter klibinde çizilmeyebilir).
- Kapanış: IoU <= 0,95 olan klipler 9 kare; 08 -> 00 geçişine bakılmalı.
