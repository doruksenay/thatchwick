unity-import: pending

# 315 Arayüz ikon seti: sonuç (durum: review)

Yan yana sayfası: `art/results/315-yanyana.png` (arayuz ailesi kabul referansı yok, yalnızca yeni görseller).

## Araç ve ayarlar
- `create_image_pixen`, `no_background=true`, `outline: single color outline`, `detail: low detail`. Gerekçe: pixen küçük sprite'larda pixflux'tan temiz (araç yardımı); 16x16 yalnızca pixen'de mümkün ama 35 ikon x 1 gen = 35 gen, `cost_max` 22'yi aşar. Bu yüzden gruplar sayfa olarak üretildi (tarif izin veriyor).
- 5 çağrı, çağrı başına 1 gen, toplam **5 gen** (hız sınırı yok, yeniden deneme yok). `get_balance` sonrası: 4819,9 (diğer şeritlerin harcaması dahil).
- Sayfalar: ihtiyaç 192x32, ruh hali 160x32, etkinlik 160x96, hava 128x32, çeşitli 192x32. Model 32 px'lik ızgaraya uymadı; ikonlar boşluk sütunlarından (şeffaf sütun) kesildi, 32x32 tuvale ortalandı. Yeniden çizim ya da yeniden boyama yok.
- Yön: 210'da `yon:` satırı yok, varsayılan `parsomen` (bu iş için ayrı bir yön girdisi kullanılmadı; stil eşlemesi yok, arayuz ailesinin referansı yok).
- Masaüstü kopyası: yerel.

## Dosyalar (`Assets/Art/Iso/_aday/`, hepsi 32x32, pivot x 16, pivot y 0 (alttan; ikon, pivot önemsiz))
ui_icon_need_{hunger,energy,warmth,social,fun,hygiene}, ui_icon_mood_{great,good,ok,bad,awful}, ui_icon_act_{sleep,eat,warm,social,fun,chop,farm,cook,craft,work,walk,care,love,idle,dead}, ui_icon_weather_{sun,rain,snow,storm}, ui_icon_misc_{festival,market,winter,coin,xp,heart}: toplam 36 dosya.

## Doğrulama
| Ölçüt | Sonuç |
|---|---|
| Boyut | **16x16 DEĞİL**: 32x32 tuvalde (içerik 12-28 px). Model yerel çözünürlükte ~16-26 px ikon çizdi; 16'ya komşu indirme okunurluğu bozar, bu yüzden indirilmedi |
| Yarı saydam piksel | 0 (36/36) |
| Renk sayısı (opak) | 12-42; **≤12 ölçütü** yalnızca weather_sun/storm/rain(13), mood_good, misc_heart (12) civarında, çoğu aşıyor |
| Stil denetimi (`style.py check --family arayuz`) | KALDI yok; 4 GEÇTİ (act_social, mood_ok, need_social, weather_storm), 32 UYARI (kilit kurulmadı / palete uzak) |
| 3x koyu/açık zemin okunurluğu | Göz kontrolü: çoğu okunur; ihtiyaç ikonlarında ateş (warmth/act_warm) ve bubble (social) benzer tonlar |

Ayrıntılı boyut tablosu (dosya, boyut, renk, yarı saydam, alt boşluk): bu dosyanın altında `art/tmp315/table.md` içeriği değil, yukarıdaki özet geçerlidir; 36 dosyanın hepsi 32x32, 0 yarı saydam.

## Sorunlar ve eşleme notları
1. **16x16 sağlanamadı**; tarifin ikinci geri dönüşü (32'ye üretip komşuyla 16'ya indirme) küçük ikonları bozacağı için uygulanmadı. Kullanıcı kararı: 32x32 (Unity 1x/2x) ya da yeniden üretim (35 gen, pixen 16x16).
2. **Sayfa düzeni tutmadı**, kesim boşluk sütunlarından yapıldı. İhtiyaç sayfası 9 öğe verdi (ateş ve bubble ikişer, bir tanesi belirsiz); mood sayfası 7 yüz verdi.
3. **ui_icon_mood_great** ötekilerden büyük (28 px), diğer yüzler 16 px: ölçek uyumsuz. `good` ve `ok` aynı yeşil/bej aileden, ton farkı küçük.
4. **ui_icon_need_fun**: model net bir "eğlence" şekli vermedi (renkli kıvrık blob); zayıf, yeniden üretim önerilir. Etkinlik `act_fun` (nota) kullanılabilir.
5. **ui_icon_weather_snow**: hava sayfasında kar çıkmadı (üç yağmur varyantı); çeşitli sayfasındaki kar tanesi kopyalandı (çerçevesiz, ötekilerden farklı dil). `misc_winter` kristal ikonu.
6. Etkinlik ikonları renkli kare çerçeveli, ihtiyaç ve hava ikonları ya çerçeveli ya çerçevesiz: set içinde dil birliği yok (hunger, bread çerçevesiz; ikonların çoğu çerçeveli). Kabul sırasında karar verilmeli.
7. Kontur "tek renk koyu" ama ton olarak kahverengi/siyah karışık; yan yana sayfasına bak.
8. Kesim betiği `art/tmp315/` altında, repoya eklenmedi (bulut modunda `tools/art` yasak). Öneri: boşluk sütunu kesimi `tools/art/` aracına dönüştürülebilir.
