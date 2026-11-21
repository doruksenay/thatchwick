unity-import: pending

# 742 His ikonları: sonuç (durum: review, 10/12 teslim)

Yan yana sayfası: `art/results/742-yanyana.png` (arayuz ailesinin kabul referansı yok, kilit kurulmadı).

## Araç ve ayarlar
- `create_image_pixen` 32x32, `no_background=true`, `outline: single color outline`, `detail: low detail`, ikon başına 1 çağrı = 12 gen (cost_est 36 içinde). Gerekçe: 212'de kabul edilen yöntem (32 tek tek, 16'ya komşuyla indirme).
- 32x32 ham çıktı alfa ikili yapılıp `_32` ekiyle konuldu. 16x16: en yakın komşu indirme + yerel Pillow ile 12 renge kuantizasyon (mediancut, dither yok). PixelLab `reduce_colors` çağrılmadı.
- Çıktı yolu: iş dosyasındaki `Assets/Art/Ui/` klasörü yok ve 55b iş dosyası yok; işçi yalnızca `Assets/Art/Iso/` altına yazabildiği için 212 ikonlarının klasörü `Assets/Art/Iso/_aday/` ve adlandırması `ui_icon_feel_<anahtar>` kullanıldı. Kullanıcı farklı yol isterse taşınır.
- Hız sınırı hatası olmadı. Stil eşlemesi yok (referans yok).
- `get_balance` sonrası: 9224 (diğer şeritlerin harcaması dahil). Masaüstü kopyası: yerel.

## Dosyalar (`Assets/Art/Iso/_aday/`, 16x16 ve ayrıca `_32` 32x32; ikon, pivot x 8, pivot y 0 alttan, önemsiz)
ui_icon_feel_{grateful, trusts, admires, misses, hurt, jealous, betrayed, grudge, ashamed, shared_grief}

## Teslim edilmeyenler (stil denetimi KALDI, `art/results/742-red/` altında)
- feel_close (palet %78, palet dışı renkler; ayrıca iki figür okunmuyor)
- feel_heartbroken (palet %72, palet dışı renkler; şekil iyi okunuyor, kullanıcı isterse kabul edilebilir)
Yeniden üretim için ek iş gerekir (2 gen).

## Doğrulama
| Dosya | Renk | Palete yakın | Uzaklık | Sonuç |
|---|---|---|---|---|
| admires | 12 | %100 | 9.9 | GEÇTİ |
| ashamed | 12 | %96 | 8.1 | GEÇTİ |
| betrayed | 12 | %100 | 7.8 | GEÇTİ |
| grateful | 11 | %85 | 10.8 | UYARI (palete uzak) |
| grudge | 10 | %100 | 7.9 | GEÇTİ |
| hurt | 12 | %100 | 7.2 | GEÇTİ |
| jealous | 11 | %94 | 9.3 | GEÇTİ |
| misses | 11 | %100 | 8.4 | GEÇTİ |
| shared_grief | 12 | %93 | 11.7 | UYARI (palete uzak) |
| trusts | 11 | %100 | 10.3 | UYARI (palete uzak) |
| close | 11 | %78 | 13.8 | KALDI |
| heartbroken | 9 | %72 | 16.9 | KALDI |

Boyut 16x16 / 32x32, yarı saydam piksel yok, renk ≤ 12. validate_iso ikonlara uygulanmaz.

## Sorunlar
- Okunurluk (göz kontrolü, 16x16): zayıf olanlar misses (pencerede uzak figür görünmüyor), betrayed (gri yumru, mendil belirsiz), grateful (ayrıntılı). Daha iyisi için `_32` sürümleri kullanılabilir.
- Renk dili: olumsuzlar mor/gri-mavi, olumlular bal tonu; kalp ikonu (heartbroken) pembe-mavi çıktığı için KALDI.
