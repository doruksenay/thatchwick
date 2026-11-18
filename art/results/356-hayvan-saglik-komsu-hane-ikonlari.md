unity-import: pending

# 356 Hayvan sağlığı ve komşu hane ikonları: sonuç (durum: review)

Yan yana sayfası: `art/results/356-yanyana.png` (arayuz ailesinde kabul referansı yok).

## Araç ve ayarlar
- `create_image_pixen`, 16x16, `no_background=true`, `outline: single color outline`, `detail: low detail` (315 ile aynı; 16x16 yalnızca pixen'de mümkün). 8 çağrı x 1 gen = **8 gen** (cost_max 8). Hız sınırı (10/10 iş) nedeniyle ilk toplu gönderim reddedildi (harcama yok); 40-45 sn bekleyip yeniden gönderildi, 2 tur.
- `get_balance` sonrası: 4694,9 (diğer şeritlerin harcaması dahil). Masaüstü kopyası: yerel.

## Dosyalar (`Assets/Art/Iso/_aday/`; pivot x 8, pivot y 0 alttan, ikon, önemsiz)
| Dosya | Boyut | Pivot x | Pivot y |
|---|---|---|---|
| ui_icon_animal_{sick,cold,cured} | 16x16 | 8 | 0 |
| ui_icon_house_{head,heir,extinct,wedding,our_blood} | 16x16 | 8 | 0 |

## Doğrulama
| Dosya | Opak renk | Yarı saydam | Stil denetimi |
|---|---|---|---|
| animal_sick | 54 | 0 | UYARI (palet dışı) |
| animal_cold | 33 | 0 | UYARI |
| animal_cured | 19 | 0 | UYARI |
| house_head | 26 | 0 | UYARI |
| house_heir | 27 | 0 | UYARI (palete uzak) |
| house_extinct | 18 | 0 | GEÇTİ |
| house_wedding | 32 | 0 | UYARI |
| house_our_blood | 25 | 0 | UYARI |

KALDI yok; arayuz ailesinin kilidi kurulmadı (uyarı).

## Sorunlar
1. **Renk sayısı ≤12 sağlanamadı** (18-54, ışık/gölge degradeleri). `reduce_colors` uygulanmadı (tarifte yok); kullanıcı kararı.
2. **house_heir** çerçeveli (koyu mor kare çerçeve), ötekiler çerçevesiz: dil birliği yok; "küçük anahtar" okunuyor ama çerçeve fazla. Yeniden üretim önerilir.
3. **house_head** altın anahtar, çapraz duruyor; okunur. **house_wedding** iki halka yerine iç içe tek halka gibi okunuyor, zayıf.
4. **house_our_blood** 36 px'lik küçük bordo kalp; çapraz dikiş dokusu 16 px'te okunmuyor (düz kalp).
5. sick yaprak+damla belirsiz (damla sarı nokta); cold kar tanesi, cured filiz net, extinct söndürülmüş mum duman ile okunur.
6. validate_iso.py zemin/2:1 aracıdır, ikonlara uygulanmadı; 3x koyu/açık zemin önizlemesi göz kontrolüyle: çoğu okunur (`art/tmp356/preview.png`, commit edilmedi).
