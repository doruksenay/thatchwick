unity-import: pending

# 316 Envanter ve ürün ikonları: sonuç (durum: review)

Yan yana sayfası: `art/results/316-yanyana.png` (arayuz ailesinde kabul referansı yok, yalnızca yeni görseller).

## Araç ve ayarlar
- `create_image_pixen`, 32x32, `no_background=true`, `outline: single color outline`, `detail: low detail`. Gerekçe: 315'te sayfa (sprite sheet) üretimi ızgaraya uymadı ve ikon sayısı belirsiz çıktı; bu yüzden ikon başına bir çağrı (güvenilir kesim, ayrı betik gerekmez).
- 30 çağrı x 1 gen = **30 gen** (`cost_est` 24, `cost_max` 40: sınır içinde, 2 katı aşılmadı). Hız sınırı: 10/10 eşzamanlı iş (paylaşılan hesap) hatası alındı, çağrılar boşalınca yeniden gönderildi (tarifin tek istisnası); başarısız çağrı harcama yapmadı.
- `get_balance` sonrası: 4722,9 (paralel şeritlerin harcaması dahil).
- Tarif "36 ikon" diyordu, `outputs` listesi 30 dosya veriyor; listedeki 30 dosya üretildi.
- **Renk indirgeme:** ham çıktılar 21-60 renk (ölçüt ≤16). Pillow ile 16 renge median-cut (dither yok, alfa 0/255) indirildi; yeniden boyama yok. Ham hâller `art/tmp316/out/` altında (commit edilmedi).
- Masaüstü kopyası: yerel.

## Dosyalar (`Assets/Art/Iso/_aday/`, hepsi 32x32 RGBA, pivot x 16, pivot y 0 alttan; ikon, pivot önemsiz)
icon_item_seed_{turnip,cabbage,leek,pea,pumpkin}, icon_item_{turnip,cabbage,leek,pea,pumpkin}, icon_item_{pickled_cabbage,pickled_turnip,dried_pea,dried_leek}, icon_item_{stew_plain,stew_good,stew_fine,soup_pea,soup_pea_good,omelet}, icon_item_{berries,mushroom,nut,honey,herb,egg}, icon_item_{log,firewood,plank,sapling_oak}: toplam 30 dosya.

## Doğrulama
| Ölçüt | Sonuç |
|---|---|
| Boyut | 32x32 (30/30) |
| Yarı saydam piksel | 0 (30/30) |
| Renk sayısı | ≤16 (indirgeme sonrası; ham 21-60) |
| `style.py check --family arayuz` | KALDI yok; 1 GEÇTİ (seed_pumpkin), 29 UYARI (kilit kurulmadı / palete uzak) |
| `validate_iso.py` | uygulanmadı (izometrik değil, UI ikonu) |
| Yemek kalite basamakları | stew_plain < stew_good < stew_fine kase doluluğu/garnitürle göz kontrolünde ayırt ediliyor |

## Sorunlar
1. **Yüz çizilmiş ikonlar (kawaii eğilimi):** `egg`, `mushroom`, `turnip` (ve olasılıkla `nut`) yüz/göz ile geldi; envanter ikonu için uygunsuz, yeniden üretim önerilir.
2. **Stil dili birliği zayıf:** tohum kesesi ailesi (5) tutarlı; ancak `honey` (kavanoz yerine kutu/belirsiz şekil), `stew_*` ve `soup_*` kaselerinde boyut ve perspektif farkı var. Yeşil sebzeler (leek, cabbage) biçim olarak tanınıyor.
3. 310'daki olgun ekin renkleriyle uyum göz kontrolü: genel olarak uyumlu, kesin renk karşılaştırması yapılmadı.
4. 2x koyu/açık zemin testi yalnızca yan yana sayfasındaki koyu zeminde yapıldı.
5. Öneri: yüzlü ikonlar için tarife "no face, no eyes" eklenip yeniden üretim (5 gen).
