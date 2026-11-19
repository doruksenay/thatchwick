unity-import: pending

# 213 Arayüz ekranları planı

PixelLab çağrılmadı (0 gen). Kalan (`get_balance`): 3182,9. Yön: 210'da `yon:` satırı yok, varsayılan `parsomen` (iş dosyalarında yazılı).

## Ekranlar ve işler
| Ekran | Yerleşim anahtarı | İş | Önizleme |
|---|---|---|---|
| Yapı modu | yapi | 213a | 213-yapi-yerlesim-x2.png |
| Eşya kartı | esya_karti | 213a | 213-esya_karti-yerlesim-x2.png |
| Pazar | pazar | 213a | 213-pazar-yerlesim-x2.png |
| Köy | koy | 213b | 213-koy-yerlesim-x2.png |
| Aile ağacı | aile | 213b | 213-aile-yerlesim-x2.png |
| Depo | depo | 213b | 213-depo-yerlesim-x2.png |
| Etkileşim | etkilesim | 213c | 213-etkilesim-yerlesim-x2.png |
| Gün sonu | gun_sonu | 213c | 213-gun_sonu-yerlesim-x2.png |
| Ayarlar (kaza ayarı dahil) | ayarlar | 213c | 213-ayarlar-yerlesim-x2.png |
| Teklif | teklif | 213d | 213-teklif-yerlesim-x2.png |
| Ayna | ayna | 213d | 213-ayna-yerlesim-x2.png |

İşler: 213a/b/c cost_est 45, cost_max 60; 213d cost_est 30, cost_max 40. Toplam tahmin 165 gen (üst sınır 220). Hepsi `depends_on: ["210"]`, `worker: yerel` (211 gibi base64 stil görseli).

## Doğrulama
- 11 ekran için `ui_mockup.py pieces` ve `mockup --placeholder` hatasız (çıkış 0; yalnızca Pillow DeprecationWarning).
- `queue_ready.py --all` dört yeni işi ayrıştırıyor, cost_max ≤ 60.
- Unity IMGUI panelleri okunmadı; içerik tarifteki tabloya göre, örnek metinler uydurma. Yerleşimler yaklaşık, Unity'ye geçişte ince ayar gerekebilir.
