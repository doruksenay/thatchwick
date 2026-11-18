unity-import: pending

# 400 Cam eşyalar, aynalar, kutu yatak ve cam ikonlar (rapor): 6/15 teslim, review

Yan yana sayfası: [400-yanyana.png](400-yanyana.png) (teslim edilen 4 eşya + referanslar). İkon sayfası: [400-ikonlar.png](400-ikonlar.png) (7 ikonun hepsi, alfabetik: ash, clear_glass, fine_sand, glass, leaded_glass, mirror_glass, mirror_sphere). Teslim edilmeyenler: `art/results/400-red/`. Masaüstü kopyası: yerel.

## Araç ve ayarlar
- Eşyalar: `create_map_object`, stil eşleme, referanssız kılavuz + özel maske (`high top-down`; duvar aynaları `side`), `low detail`, `basic shading`, `single color outline`, tarif açıklaması + 301 ortak eki (cam eşyalarda "dark brown wood" çıkarıldı). Masa üstü: 32x32 tuval, 12x6 taban elması alt kenardan 4 px yukarıda, maske eşya yüksekliği + 1 px. Duvar: "/" eğimli paralelkenar kılavuz (duvar rengi 184,160,122): küçük ayna 48x56, büyük ayna 96x64. Kutu yatak: `footprint_guide.py --w 112 --h 96 --n 2 --axis x --height 40`. Kılavuz betikleri `art/tmp400/` altında (commit edilmez). 8 çağrı paralel, tek deneme, çağrı başı 1 gen = 8 gen.
- İkonlar: `create_image_pixen`, 32x32, `no_background`, `single color outline`, `low detail`, tarif açıklaması + "game inventory icon, single object, no face, no eyes". 7 çağrı x 1 gen = 7 gen. Ham çıktılar 38-55 renkti; teslim edilenler 316'daki gibi 16 renge median-cut (dither yok, alfa 0/255) indirildi.
- Toplam 15 gen (cost_est 15, cost_max 22). `get_balance` iş sonu: 4283,9 (paralel şeritler dahil; işten önce 4303,9).
- Çıktı yolu notu: iş dosyası ikonları `Assets/Art/Icons/` altına istiyor, ama işçi yalnızca `Assets/Art/Iso/` altına yazabilir; teslim edilen 2 ikon `Assets/Art/Iso/_aday/` altında (316 ile aynı). Taşıma kod/kullanıcı işidir.

## Sonuç özeti
| # | Dosya | Durum | Not |
|---|---|---|---|
| 1 | glass_bottle | TESLİM EDİLMEDİ | stil KALDI (palet %42, parlak zümrüt yeşili); `fit` işe yaramadı (%42). Şekil tanınıyor |
| 2 | glass_cup | TESLİM EDİLMEDİ | KALDI (palet %29, x1,44); fit sonrası hâlâ %29. Parlak yeşil, çıkıntı ayrıntısı seçilmiyor |
| 3 | glass_jar | teslim (4 px kaydırma) | GEÇTİ. En alt opak satır y=22 idi (taban elması alt yarısı y 25-27), 4 px aşağı kaydırıldı (piksel taşıma): y=26 |
| 4 | glass_goblet | TESLİM EDİLMEDİ | KALDI (palet %72, ort. 14,6); fit sonrası aynı. Kadeh tanınıyor, altın tonlu |
| 5 | hourglass | teslim | GEÇTİ |
| 6 | convex_mirror | TESLİM EDİLMEDİ | KALDI (palet %51, x1,88; fit x0,61 sonrası %70, hâlâ KALDI). Altın çerçeveli, "/" eğimsiz düz oval |
| 7 | large_mirror_2w | teslim (UYARI) | palete uzak (%95, 12,7). Dört kemerli cam panel, "dışbükey yuvarlak parça" gibi okunmuyor (pencere gibi): dönem açısından göz kararı |
| 8 | box_bed_2x1 | teslim | GEÇTİ. Koyu bordo-kırmızımsı ahşap, mavi-yeşil yorgan; 47 renk |
| 9 | icon_ash | TESLİM EDİLMEDİ | yosunlu taş yığını gibi, kül değil |
| 10 | icon_fine_sand | TESLİM EDİLMEDİ | yaratık/sivri kabuk gibi, kum yığını değil |
| 11 | icon_glass | teslim (UYARI) | zümrüt kristal kümesi; "ham yeşil cam parçası" olarak okunabilir, palet dışı (%41, kilit yok) |
| 12 | icon_clear_glass | TESLİM EDİLMEDİ | yüz çizilmiş gri küre (no face verilmesine rağmen) |
| 13 | icon_leaded_glass | teslim (UYARI) | gri çerçeveli elmas cam, tanınıyor (palet %67, kilit yok) |
| 14 | icon_mirror_sphere | TESLİM EDİLMEDİ | renkli desenli top, kurşun astarlı cam küre değil |
| 15 | icon_mirror_glass | TESLİM EDİLMEDİ | yüzlü/kafa gibi disk, ayna parçası değil |

## Dosyalar (Assets/Art/Iso/_aday/)
| Yol | Piksel | Pivot x | Pivot y (alttan) |
|---|---|---|---|
| iso_item_glass_jar_se.png | 32x32 | 16 | 4 (taban elmasının merkezi, y alttan 7) |
| iso_item_hourglass_se.png | 32x32 | 16 | 4 (taban elmasının merkezi, y alttan 7) |
| iso_wall_item_large_mirror_2w_sw_ne.png | 96x64 | 48 | 32 (görsel merkezi) |
| iso_item_box_bed_2x1_se.png | 112x96 | 56 | 32 |
| icon_glass.png | 32x32 | 16 | 0 (ikon) |
| icon_leaded_glass.png | 32x32 | 16 | 0 (ikon) |

## Doğrulama (validate_iso, `--max-colors 48`)
| Dosya | Opak kutu | Renk | Yarı saydam | Alta değme | Sonuç |
|---|---|---|---|---|---|
| jar (kaydırma sonrası) | x10-21 y11-26 | 12 | 0 | yok | geçti |
| hourglass | x10-21 y3-28 | 15 | 0 | yok | geçti |
| large_mirror | x11-85 y0-55 | 29 | 0 | yok | geçti |
| box_bed | x10-101 y0-88 | 47 | 0 | yok | geçti |
| icon_glass | 14 renk | | 0 | | ≤16 |
| icon_leaded_glass | 15 renk | | 0 | | ≤16 |
Teslim edilmeyenler de validate_iso'dan geçti; elenme nedeni stil/içerik.

## Stil denetimi (`style.py check --family esya`; ikonlar `arayuz`, kilit yok)
| Dosya | Palete yakın | Ort. uzaklık | Doygunluk | Sonuç |
|---|---|---|---|---|
| jar | %98 | 8.6 | x0.84 | GEÇTİ |
| hourglass | %94 | 9.4 | x0.73 | GEÇTİ |
| large_mirror | %95 | 12.7 | x1.02 | UYARI |
| box_bed | %89 | 8.5 | x1.11 | GEÇTİ |
| bottle (teslim değil) | %42 | 27.9 | x1.17 | KALDI |
| cup (teslim değil) | %29 | 34.3 | x1.44 | KALDI |
| goblet (teslim değil) | %72 | 14.6 | x1.15 | KALDI |
| convex (teslim değil) | %51 | 26.4 | x1.88 | KALDI |
| icon_glass | %41 | 27.6 | - | UYARI |
| icon_leaded_glass | %67 | 14.9 | - | UYARI |

## Sorunlar ve öneriler
- Berrak/yeşil cam ailenin ahşap-ağırlıklı paletinden dışarıda kalıyor: dört cam eşya palet nedeniyle elendi. Cam için palet istisnası (örn. `--light` benzeri bir "cam" bayrağı ya da yeşil/mavi tonlarını paleti genişleten referans) kullanıcı kararıdır; kabul edilirse bottle/cup/goblet/convex `400-red/` içinde hazır.
- Duvar aynaları "/" eğimini taşımıyor (düz önden görünüm); büyük ayna yuvarlak parçalar yerine kemerli paneller. Yeniden deneme için tarif ("round convex roundels") ve kılavuz sadeleştirilmeli.
- Ikon yeniden denemeleri: kül (`pile of grey ash powder, flat, no rocks`), kum, berrak cam, ayna parçası, ayna küre için 5 gen; "no face" yine yüz çizdirdi (clear_glass), pixen ikon yerine pixflux ya da `create_map_object` 32x32 (399a'daki yöntem) denenebilir.
- Kutu yatak koyu kırmızımsı kahve (300'deki yatak ton referansıyla karşılaştır); göz kararı.
- Gölge pişmemiş, ışık sol-üst (gözle). Döşeme testi eşya için uygulanmadı. Seed parametresi yok.
