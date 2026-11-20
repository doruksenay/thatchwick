unity-import: pending

# 831 Balık ve tatlı katmanı (_v2): balık teslim (kontrast sınırda), tatlı teslim edilmedi

Yan yana: [831-yanyana.png](831-yanyana.png) (yeni balık, 797 hâli ve referanslar). Ham/reddedilenler: `art/results/831-red/`. Masaüstü kopyası: yerel.

## Araç ve ayarlar
- `create_map_object` x2 (paralel), stil eşleme, high top-down, basic shading, low detail, single color outline, 32x32 (797 ile aynı). Arka plan = kap (oval tabak / tahta), maske = kap silüetinin 5x5 aşındırılmışı (balık bbox 7,10,25,22; tahta 6,9,26,22). Tek deneme, 2 çağrı.
- Harcanan: ~2 gen (`cost_est` 2). `get_balance` 9859 (diğer şeritlerle ortak sayaç; bu işin payı ayrılamıyor).
- Balık ham çıktı x1.52 doygun (KALDI); `style.py fit` ile x0.76'ya indirildi, sonuç UYARI (x1.31 doygun, palete yakın %100, uzaklık 5.3).

## Dosyalar (pivot x 16, pivot y 10 alttan)
| Yol | Piksel | Durum |
|---|---|---|
| Assets/Art/Iso/_aday/iso_serve_food_fish_4_v2.png | 32x32 | teslim (UYARI: doygunluk; validate_iso geçti, 22 renk) |
| iso_serve_food_sweet_4_v2.png | - | teslim edilmedi |

## Doğrulama
| Dosya | Stil | validate_iso | Parlaklık yemek/kap |
|---|---|---|---|
| fish_4_v2 | UYARI (fit sonrası) | geçti | 55 / 74 (ham hâl) |
| sweet_4_v2 (ham) | GEÇTİ | - | 98 / 95 |

## Sorunlar
- ⚠ Ek ölçüt (yemek kaptan ayırt edilmeli): **tatlı sağlanmadı**; ortalama parlaklık kapla aynı (98 / 95), 797 hâlinden (103) farkı yok. Teslim edilmedi, ham hâl `831-red/iso_serve_food_sweet_4_v2_dusuk-kontrast.png`.
- ⚠ Balık: kabın kara iç yüzeyinde gri-beyaz parçalar var, 797 hâlinden daha okunur; ama "üç bütün balık" değil, kırık gri/kırmızı bir şerit. Ortalama parlaklık kaptan koyu (kap içi çok karanlık). Kullanıcı `831-yanyana.png`'ye bakmalı.
- Model "clearly brighter" ifadesine uymadı; yeniden deneme yok (max_attempts 1).

## Sorular
- Tatlı için: daha koyu tahta kap ya da yemeği kodla tonlama; ya da yeni bir deneme işi (~2 gen) onaylanır mı?
