unity-import: pending

# 734 Çalı ve ahşap bodrum merdiveni (rapor)

Yan yana sayfası: [734-yanyana.png](734-yanyana.png) (merdiven + çalı 2. deneme + esya referansları). Teslim edilmeyen: `art/734-red/` (çalı, 2 deneme).
Masaüstü kopyası: yerel (bulutta yapılamaz).

## Araç ve ayarlar
`create_map_object`, stil eşleme, kılavuz + maske (`tools/art/footprint_guide.py`; çalı 1x1 yükseklik 28, merdiven `--n 2 --axis y` yükseklik 8, taş kardeşle aynı 112x96 ve yön). `high top-down`, `low detail`, `basic shading`, `single color outline`, tarifteki ek. Çağrılar: merdiven 1, çalı 2 (max_attempts 2; 2. denemede "muted olive and brown-green leaves" eklendi) = 3 generation (cost_max 8). `get_balance` iş sonu: 1010,6 (paralel şeritler de harcıyor; bu işin payı 3).

## Sonuç özeti
| # | Eşya | Durum | Not |
|---|---|---|---|
| 1 | çalı (shrub_v2) | TESLİM EDİLMEDİ | deneme 1: 53 renk (>48), %55 palet, doygunluk x1.78 KALDI; deneme 2: 48 renk geçti ama %72 palet, doygunluk x1.60 KALDI. esya kilidi kurulu, `fit` yok |
| 2 | ahşap bodrum merdiveni | teslim (UYARI) | kare çukur, kenarda tahta çerçeve, küçük basamaklar; ip korkuluk seçilmiyor |

## Dosyalar (Assets/Art/Iso/_aday/)
| Yol | Piksel | Pivot x | Pivot y (alttan) |
|---|---|---|---|
| iso_item_stairs_down_wood_1x2_se.png | 112x96 | 56 | 32 |

## Doğrulama
| Dosya | Opak sınır kutusu | Renk | Yarı saydam | Sonuç |
|---|---|---|---|---|
| merdiven | x10-102, y35-83 | 41 | 0 | geçti |
| (red) çalı d1 | x10-69, y7-55 | 53 | 0 | BAŞARISIZ (renk) |
| (red) çalı d2 | x9-69, y6-61 | 48 | 0 | geçti (stil KALDI) |

## Stil denetimi
| Dosya | Palete yakın | Ort. uzaklık | Doygunluk | Sonuç |
|---|---|---|---|---|
| merdiven | %91 | 10.3 | x1.26 | UYARI |
| (red) çalı d1 | %55 | 23.0 | x1.78 | KALDI |
| (red) çalı d2 | %72 | 15.6 | x1.60 | KALDI |

## Sorunlar
- Merdiven: basamaklar küçük, çukur geniş; taş kardeşe göre basamak payı az, ip korkuluk yok. Kullanıcı gözle karar versin.
- Çalı: yeşil doygunluğu esya paletinden uzak; çalı için yeşil referans yok. Öneri: esya referanslarına yeşil bir bitki (ör. saksı) eklenmesi ya da çalının `bitki` ailesine alınması.
- Ölçülmedi: ışık sırası, ayna uygunluğu. Gölge pişirilmemiş görünüyor (gözle).
