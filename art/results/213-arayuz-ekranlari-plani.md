unity-import: pending

# 213 Arayüz ekranları planı

Plan işi, PixelLab çağrılmadı (0 generation). `get_balance` sonrası kalan: 3616,5 (Tier 2, 1383,5 kullanılmış).

Yön: 210'da `yon:` satırı yok, varsayılan `parsomen` (türev işlerde aynı kural).

## Ekranlar ve işler
| Ekran | Yerleşim anahtarı | Parça | İş | Yerleşim önizlemesi |
|---|---|---|---|---|
| Yapı modu | yapi | 24 | 213a | art/results/213-yapi-yerlesim-x2.png |
| Eşya kartı | esya_karti | 13 | 213a | art/results/213-esya_karti-yerlesim-x2.png |
| Pazar | pazar | 18 | 213a | art/results/213-pazar-yerlesim-x2.png |
| Köy | koy | 16 | 213b | art/results/213-koy-yerlesim-x2.png |
| Aile | aile | 17 | 213b | art/results/213-aile-yerlesim-x2.png |
| Depo | depo | 34 | 213b | art/results/213-depo-yerlesim-x2.png |
| Etkileşim | etkilesim | 14 | 213c | art/results/213-etkilesim-yerlesim-x2.png |
| Gün sonu | gun_sonu | 12 | 213c | art/results/213-gun_sonu-yerlesim-x2.png |
| Ayarlar | ayarlar | 19 | 213c | art/results/213-ayarlar-yerlesim-x2.png |
| Teklif | teklif | 13 | 213d | art/results/213-teklif-yerlesim-x2.png |
| Ayna | ayna | 19 | 213d | art/results/213-ayna-yerlesim-x2.png |

| İş | Ekran | cost_est | cost_max |
|---|---|---|---|
| 213a | 3 | 45 | 60 |
| 213b | 3 | 45 | 60 |
| 213c | 3 | 45 | 60 |
| 213d | 2 | 30 | 40 |
| Toplam | 11 | 165 | 220 |

Tüm işler `worker: yerel` (211 ile aynı gerekçe: stil görseli base64 aktarımı), `depends_on: ["210"]`, `pixellab: true`.

## Doğrulama
- 11 ekran için `ui_mockup.py pieces` ve `mockup --placeholder` hatasız (çıkış 0). Tek uyarı Pillow `getdata` kullanımdan kalkma bildirimi.
- `queue_ready.py --all` dört yeni işi ayrıştırıyor, hepsi `cost_max <= 60`.
- Yapı ve depo önizlemelerine bakıldı. Yapı'da beşinci kategori sekmesi değer kartıyla çakışıyordu; sekmeler daraltılıp düzeltildi.

## Sorunlar
- `tools/art/ui_layouts.json` iş dosyasının çıktısı olarak değişti (veri dosyası, betik değil). Bulut kuralı `tools/art` değişikliğini PR'a yönlendiriyorsa bu sonuç PR olarak incelenir. Dosya özgün 2 boşluk girintisiyle yazıldı; `durum`, `kisi`, `yaratma` anahtarları değişmedi.
- Pazar kaydırma çubuğu ve etkileşim listesi boş kutu olarak çizildi; sürgü davranışı kodda.
- Ayarlar'daki kaza ayarı (Kapalı/Seyrek/Normal) üç segment düğmesi, ortadaki aktif.
- Önizlemelerdeki portre dosyası (`portrait_aldric_64.png`) yoksa yer tutucu çizilir.
- Masaüstü kopyası: yerel.
