unity-import: pending

# 213 Arayüz ekranları planı

PixelLab çağrılmadı (0 gen). `get_balance`: 4826,9 / 5000, Tier 2 (plan `art/budget.md` ile aynı). Yön: 210'da `yon:` satırı yok, varsayılan `parsomen`. Masaüstü kopyası: yerel.

`tools/art/ui_layouts.json` içine 11 ekran eklendi (416x192, boş kutular + örnek metin). Önizleme betiği değişmedi. `pieces` ve `mockup --placeholder` 11 ekranda hatasız çalıştı. `queue_ready.py --all` yeni işleri ayrıştırıyor.

| Ekran | İş | Yerleşim önizlemesi |
|---|---|---|
| yapi, esya_karti, pazar | 213a | art/results/213-<ekran>-yerlesim-x2.png |
| koy, aile, depo | 213b | aynı biçim |
| etkilesim, gun_sonu, ayarlar | 213c | aynı biçim |
| teklif, ayna | 213d | aynı biçim |

| İş | cost_est | cost_max |
|---|---|---|
| 213a | 45 | 60 |
| 213b | 45 | 60 |
| 213c | 45 | 60 |
| 213d | 30 | 45 |

Toplam tahmini: 165 gen (en çok 225).

## Notlar
- Dört iş dosyası `worker: yerel`: `create_ui_asset` stil görselini base64 ister, 211 bulutta bu yüzden bloklandı.
- Yapı ekranında kategori sekmesi 6 değil 5 (altıncı kat seçiciyle çakışıyordu).
- Ayarlar ekranındaki kaydırıcılar örnektir; kaza ayarı (Kapalı/Seyrek/Normal) üç pill düğmedir.
- `tools/art/ui_layouts.json` değişti: bulutta bu, sonucu PR'a yönlendirir (iş çıktısı olarak zorunluydu).
- Yardımcı betikler `art/tmp/` altında (git dışı).
