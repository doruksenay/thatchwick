unity-import: pending

# 213 Arayüz ekranları planı

PixelLab çağrılmadı (0 generation). Bulut modu; masaüstü kopyası: yerel. `get_balance` okunmadı (harcama yok).
Yön: 210'da `yon:` satırı yok, varsayılan `parsomen` (iş dosyalarına yazıldı).

## Yerleşimler (`tools/art/ui_layouts.json`, 11 yeni anahtar, 416x192)
Hepsi `ui_mockup.py pieces` ve `mockup --placeholder` ile hatasız çalıştı (14 ekran, mevcut durum/kisi/yaratma dahil).

| Ekran | Parça | Placeholder önizleme | İş |
|---|---|---|---|
| yapi | araç şeridi, 4 kategori sekmesi, kat seçici, ev değeri, 6 katalog kartı | `art/results/213-yapi-yerlesim-x2.png` | 213a |
| esya_karti | simge, fiyat, takım, açıklama, konfor çubuğu, al/sat | `213-esya_karti-yerlesim-x2.png` | 213a |
| pazar | 5 satırlı liste, teklif, akçe, onay/vazgeç | `213-pazar-yerlesim-x2.png` | 213a |
| koy | halk listesi + ilişki çubukları, festival, projeler | `213-koy-yerlesim-x2.png` | 213b |
| aile | portre düğümleri, hanedan listesi | `213-aile-yerlesim-x2.png` | 213b |
| depo | 8x4 ızgara, kapasite, ayrıntı | `213-depo-yerlesim-x2.png` | 213b |
| etkilesim | kaydırmalı menü, hedef kartı | `213-etkilesim-yerlesim-x2.png` | 213c |
| gun_sonu | özet kartları, kış göstergesi, sonraki gün | `213-gun_sonu-yerlesim-x2.png` | 213c |
| ayarlar | ayar satırları, kaza ayarı (Kapalı/Seyrek/Normal), devam/çıkış | `213-ayarlar-yerlesim-x2.png` | 213c |
| teklif | portre, huy kartları, çocuk kartı (alel gizli), kabul/ret | `213-teklif-yerlesim-x2.png` | 213d |
| ayna | önizleme, 6 seçenek, elmas fiyat yuvası | `213-ayna-yerlesim-x2.png` | 213d |

## İş dosyaları
| İş | Ekranlar | cost_est | cost_max |
|---|---|---|---|
| 213a | yapi, esya_karti, pazar | 45 | 60 |
| 213b | koy, aile, depo | 45 | 60 |
| 213c | etkilesim, gun_sonu, ayarlar | 45 | 60 |
| 213d | teklif, ayna | 30 | 40 |

Toplam tahmin 165 gen (üst sınır 220). `queue_ready.py --all` dört işi ayrıştırıyor. İşler `worker: yerel` (211'deki base64 stil görseli engeli nedeniyle).

## Sorunlar / notlar
- Metinler yalnızca örnek; yerleşimi Unity tarafı kod belirler. `esya_karti` ve `teklif` portre/ikon yuvaları boş bırakıldı.
- `ui_mockup.py` yalnızca DeprecationWarning veriyor (Pillow getdata); işlevi etkilemiyor, araca dokunulmadı.
- Üretim araçları değişmedi; geçici betikler `art/tmp/` altında.
