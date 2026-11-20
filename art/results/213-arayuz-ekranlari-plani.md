unity-import: pending

# 213 Arayüz ekranları planı

Plan işi, PixelLab çağrılmadı (0 generation). Varsayılan yön: `parsomen` (210'da `yon:` satırı yok). `get_balance` okunmadı (harcama yok).

| Ekran | İş | Önizleme |
|---|---|---|
| yapi, esya_karti, pazar | 213a | 213-<ekran>-yerlesim-x2.png |
| koy, aile, depo | 213b | aynı |
| etkilesim, gun_sonu, ayarlar | 213c | aynı |
| teklif, ayna | 213d | aynı |

| İş | cost_est | cost_max |
|---|---|---|
| 213a | 45 | 60 |
| 213b | 45 | 60 |
| 213c | 45 | 60 |
| 213d | 30 | 40 |
| Toplam | 165 | 220 |

## Notlar
- Yerleşimler `art/results/213-ui_layouts-ek.json` içinde (bulutta `tools/art` yazılamadığı için). `tools/art/ui_layouts.json`'a birleştirilmesi gerekir; 213a-d ilk adımda bunu yapar (`worker: yerel`).
- Doğrulama: birleştirilmiş geçici kopyayla 11 ekranda `ui_mockup.py pieces` ve `mockup --placeholder` hatasız; `queue_ready.py --all` dört yeni işi ayrıştırıyor (cost_max <= 60).
- IMGUI panelleri okunmadı; ekran listesi iş dosyasındaki tablodan alındı.
- Masaüstü kopyası: yerel.
