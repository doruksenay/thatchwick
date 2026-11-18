unity-import: pending

# 210 Karakter arayüzü stil pilotu (Durum paneli)

Durum: `review`. Masaüstü kopyası: yerel (bulutta yapılamaz).

## Araç ve ayarlar
- `create_ui_asset`, 416x192 (araç kabul etti), `no_background=true`, `max_colors=48`, `seed=0`, `style_image_base64` = `iso_char_villager_se.png`, `color_palette` tarifteki gibi.
- `pieces`: `ui_mockup.py pieces durum --scale 2` çıktısı; `rect` -> `rounded_rect` (radius 0), 832 koordinatları 512/832 ile ölçeklendi (araç sanal tuvali 512x236). Geçici dosya `art/tmp/pieces.json` (git yok sayar).
- Çağrı başına maliyet (araç bildirdi): parşömen 15, ahşap 15 gen. Toplam **30** (cost_est 40, cost_max 50). İki çağrı paralel gönderildi; hız sınırı hatası yok.
- `get_balance` sonrası: 4849,3 / 5000 (hesap paylaşımlı; aynı sırada başka şeritler de harcadı, bu işin payı 30).

## Dosyalar
| Yol | Boyut | Pivot |
|---|---|---|
| Assets/Art/Iso/_aday/ui_panel_durum_parsomen_1.png | 416x192 | yok (arayüz paneli) |
| Assets/Art/Iso/_aday/ui_panel_durum_ahsap_1.png | 416x192 | yok (arayüz paneli) |
| art/results/210-durum-parsomen-onizleme-x2.png | 832x384 | - |
| art/results/210-durum-ahsap-onizleme-x2.png | 832x384 | - |
| art/results/210-yanyana.png | - | - |

Yan yana sayfası: [210-yanyana.png](210-yanyana.png) (aile referansı yok, yalnızca iki aday).

## Doğrulama
| Aday | Boyut | Opak renk | Tam saydam | Tam opak | Yarı saydam | Köşeler |
|---|---|---|---|---|---|---|
| parşömen | 416x192 | 48 | 18893 | 60979 | 0 | ⚠ sol-alt köşe opak (diğer üçü saydam) |
| ahşap | 416x192 | 48 | 6842 | 73030 | 0 | dördü saydam |

`style.py check --family arayuz` (kilit yok, KALDI uygulanmaz): parşömen GEÇTİ (palete yakın %100, ort. uzaklık 6,1); ahşap GEÇTİ (%100, 6,0).

### Şablona uyum
Aracın bildirdiği parça kutuları, şablon merkezleriyle (416 ölçeğinde) karşılaştırıldı: 30 parçanın hepsi "var", en büyük kayma 1 px. Bu karşılaştırma aracın bildirdiği yerleşimdir; boyanmış pikselleri ölçmedi. Önizlemede görülenler:
- **parşömen:** ⚠ gövde zemini çoğunlukla saydam (kartlar, yuvalar ve çubuklar arasında parşömen yok; önizlemede arka plan yeşili görünüyor). Çerçeve, sekmeler, kartlar yerinde; yazılar kutulara sığıyor; aktif sekme (açık) ayırt ediliyor.
- **ahşap:** ⚠ ruh hali parşömen kartı şablondan dar ve sağa kaymış boyanmış (önizlemede "RUH HALİ" ve satır başları kartın soluna taşıyor); gösterge çerçevesi kartın alt kısmında (şablondaki `gauge` yerinde değil). İkon yuvaları, çubuklar, 5 kart ve kaydırma izi yerinde; çubuk dolguları çerçevenin içinde; aktif sekme ayırt ediliyor.

### Görsel kontrol (önizleme üzerinden)
- Yazı/harf benzeri şekil, yüz, ikon: yok. Kartlar ve yuvalar boş.
- Dithering/gürültü: belirgin yok (ahşapta damar dokusu var).
- Dış hat koyu ve net; ışık sol-üstten.
- Bilinen oyun arayüzüne benzerlik: genel "deri/ahşap RPG paneli" dili; belirli bir motif görülmedi.

### Önizleme betiği
Panel 416x192, boyut uyarısı yok. Eksik, yer tutucu çizilenler (`_aday/`): ui_icon_moodlet_good_meal, ui_icon_moodlet_harmony, ui_icon_moodlet_injured, ui_icon_moodlet_rested, ui_icon_need_energy, ui_icon_need_fun, ui_icon_need_hunger, ui_icon_need_hygiene, ui_icon_need_social, ui_icon_need_warmth (`.png`). Portre için `portrait_aldric_64.png` kullanıldı.

## Sorunlar ve öneriler
- Parşömen: gövde zemini saydam; oyunda arkasına zemin gerekir ya da yeniden üretim. Ahşap daha bütün ama ruh hali kartı yerleşimi sapıyor.
- Yeniden örnekleme/boyama yapılmadı. Ham indirmeler `art/tmp210/` (commit edilmez).
- Öneri: `ui_mockup.py pieces` için `--pixellab` bayrağı (512 tuvale ölçekle, rect -> rounded_rect). `tools/art` değiştirilmedi.
