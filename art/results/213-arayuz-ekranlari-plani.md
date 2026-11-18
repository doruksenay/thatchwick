unity-import: pending

# 213 Arayüz ekranları planı

Plan işi: PixelLab çağrılmadı, 0 generation. `get_balance` sonrası kalan: 4274,90 (Tier 2, plan `art/budget.md` ile uyumlu).
Yön: 210'da `yon:` satırı yok, varsayılan `parsomen` (iş dosyalarında yön satırı aynı kuralla okunur).
Masaüstü kopyası: yerel (gerek yok, yalnızca önizleme).

## Ekranlar ve işler
Yerleşimler `tools/art/ui_layouts.json` içine eklendi (416x192, mevcut `durum`, `kisi`, `yaratma` anahtarlarına dokunulmadı).

| Ekran | Anahtar | Parça | İş | Önizleme |
|---|---|---|---|---|
| Yapı modu | `yapi` | 22 | 213a | art/results/213-yapi-yerlesim-x2.png |
| Eşya kartı | `esya_karti` | 15 | 213a | art/results/213-esya_karti-yerlesim-x2.png |
| Pazar | `pazar` | 19 | 213a | art/results/213-pazar-yerlesim-x2.png |
| Köy | `koy` | 21 | 213b | art/results/213-koy-yerlesim-x2.png |
| Aile ağacı | `aile` | 15 | 213b | art/results/213-aile-yerlesim-x2.png |
| Depo | `depo` | 29 | 213b | art/results/213-depo-yerlesim-x2.png |
| Etkileşim menüsü | `etkilesim` | 14 | 213c | art/results/213-etkilesim-yerlesim-x2.png |
| Gün sonu | `gun_sonu` | 13 | 213c | art/results/213-gun_sonu-yerlesim-x2.png |
| Ayarlar | `ayarlar` | 15 | 213c | art/results/213-ayarlar-yerlesim-x2.png |
| Teklif | `teklif` | 17 | 213d | art/results/213-teklif-yerlesim-x2.png |
| Ayna | `ayna` | 19 | 213d | art/results/213-ayna-yerlesim-x2.png |

| İş | Ekran | cost_est | cost_max |
|---|---|---|---|
| 213a | yapi, esya_karti, pazar | 45 | 60 |
| 213b | koy, aile, depo | 45 | 60 |
| 213c | etkilesim, gun_sonu, ayarlar | 45 | 60 |
| 213d | teklif, ayna | 30 | 40 |
| **Toplam** | 11 ekran | **165** | **220** |

Her ekran bir `create_ui_asset` çağrısı (≤ 20 gen). İş dosyalarında `worker: yerel` var: araç stil görselini yalnızca base64 metni olarak alıyor, bulut işçisi bunu aktaramıyor (211'in takılma nedeni).

## Doğrulama
- Her ekran için `ui_mockup.py pieces <ekran>` ve `mockup <ekran> --placeholder`: 11/11 hatasız (çıkış kodu 0). Betik değişmedi.
- `queue_ready.py --all` 213a-d işlerini ayrıştırıyor; `cost_max <= 60`.
- Parçalar 416x192 sınırı içinde, kimlikler ekran içinde tekil (üretim betiği doğruladı).
- Depo ızgarası temsilidir (6x3 = 18 yuva); Unity kaydırma ve sayıyı kodda çoğaltır.
- Köy ekranındaki dostluk çubukları, depo kapasitesi vb. dolgular koddan gelir.

## Sorunlar
- `tools/art/ui_layouts.json` değişikliği bulutta PR'a gider (`cloud_guard`), iş tarifi bu dosyayı açıkça çıktı olarak istiyor.
- Yerleşimler IMGUI panellerinden (`VillagePanel`, `MarketPanel` vb.) alınan içerikle kuruldu; yazı tipinde "✓" yok, "bitti" yazıldı.
- Üretim betikleri `art/tmp/` altında (git dışı): `gen213.py`, `jobs213.py`.
