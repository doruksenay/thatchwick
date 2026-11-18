unity-import: pending

# 018b Yürüme seti: erkek ve kadın 84 px, 8 yön

- Araç: `animate_character`, `mode=pixminimax`, `frame_count=8`, `keep_first_frame=true`, `end_frame_url` = ilgili yönün durma karesi (`get_upload_url` ile yüklendi), yön başına tek çağrı. Metin: "walking forward at a calm, steady pace, one complete step cycle" (018a ile aynı). Aynalama yok.
- Çağrılar: kadın 8 yön + erkek 7 yön (erkek SE 018a'dan, yeniden üretilmedi) = 15 çağrı x 1 gen = **15 generation** (tahmin 16, tavan 24). Tek deneme. PixelLab hesap genelinde 10 iş yuvası var ve diğer şeritler de kullandığından çoğu çağrı "need 1 job slots but only 0 available" verdi; yuva açılınca aynı çağrılar yeniden gönderildi (eşzamanlılık istisnası, 5 dalga).
- `get_balance` sonrası: 4188,9 (diğer şeritler eşzamanlı harcıyor, bu sayı yalnızca bu işe ait değil).
- Ham çıktı 84x112 (9 kare/yön). İşleme: yalnızca kaydırma. Yatay: yönün birleşik opak kutusu ortası durma karesi kutusu ortasına (ofset -4..+4); dikey: her karenin en alt opak satırı durma karesinin satırına. Ölçekleme yok. Palet: yönün durma karesinin renkleri, en yakın RGB, alfa ikili. 9 kare de teslim (00-08); döngüde son kare atılmadı (018a ile aynı), kapanış IoU aşağıda.
- **Doygunluk:** erkek durma karelerinin kendisi (PixelLab rotasyonları) aileden çok doygun çıktı (x1,5-1,75; karakter ailesi kilidi kurulmadı). `style.py fit --family karakter` ile 70 erkek dosyası (SE hariç) doygunluğu x1,16-1,22'ye indirildi (doygunluk fit); yeniden denetimde 70/70 GEÇTİ. Ham hâller `art/results/018b-red/` içinde. Kadın dosyaları fit gerektirmedi.
- Yan yana sayfası: `art/results/018b-yanyana.png` (8 yön, durma + yürüme kare 04). GIF: `art/results/018b-yurume-man.gif`, `018b-yurume-woman.gif` (8 yön yan yana, x2).
- Masaüstü kopyası: yerel

## Dosyalar
| Yol | Boyut | Pivot x | Pivot y (alttan) |
|---|---|---|---|
| Assets/Art/Iso/_aday/anim/iso_char_{man,woman}_r84_walk_{s,se,e,ne,n,nw,w,sw}_00..08.png (erkek SE 018a'dan) | 84x84 | 42 | yönün durma karesinin ayak boşluğu (2-5, aşağıya bakın) |
| Assets/Art/Iso/_aday/anim/iso_char_{man,woman}_r84_idle_{yön}.png | 84x84 (rotasyon = durma karesi) | 42 | aynı |

Toplam 160 dosya (erkek SE'nin 10 dosyası 018a'dan). Ayak boşluğu yön başına durma karesinden gelir: 2 satır (70 dosya), 3 (20), 4 (50), 5 (20); yön içinde sabit. Pivot y bu yüzden yönde 2 yerine o yönün ayak boşluğudur.

## Doğrulama
| Ölçüt | Sonuç |
|---|---|
| validate_iso | 160/160 geçti (maks. 64 renk; fit sonrası erkek) |
| style.py check karakter | 160/160 GEÇTİ (erkek fit sonrası x1,16-1,22; kadın x0,79-1,03) |
| Kapanış IoU (00-08) | 0,968-1,000 (en düşük erkek NW 0,968, kadın SW 0,973) |
| Ardışık IoU | 0,74-0,95; en düşük erkek SW 0,74, erkek E 0,75, kadın SE 0,76 |
| Tepe kırpılması | kadın N: 98 opak piksel, kadın NE: 25 piksel 84x84'ün üstüne taşıp silindi (kafa tepesi); diğer yönlerde 0 |

## Sorunlar
- Kadın N ve NE yönlerinde kafa tepesi kırpıldı (yürürken zıplama). Kullanıcı GIF'e bakarak karar versin; gerekirse tuval büyütülmeli.
- Kimlik (yüz, giysi): yan yana sayfasında yönler arası tutarlı görünüyor, otomatik ölçülmedi.
- Idle yalnızca durma karesi kopyası.

## Sorular
- Öneri: PixelLab hesabında 10 iş yuvası şeritler arasında paylaşılıyor; paralel çağrılar sık "0 yuva" hatası alıyor. `tools/art` altında yuva bekleme yardımcısı ya da şerit başına iş sınırı değerlendirilebilir.
