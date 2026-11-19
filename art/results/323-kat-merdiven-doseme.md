unity-import: pending

# 323 Katlar: merdivenler ve kat zemin parçaları

Durum: `review`. 8/8 dosya teslim edildi (`Assets/Art/Iso/_aday/`). Masaüstü kopyası: yerel.
Yan yana sayfaları: `art/results/323-yanyana.png` (eşyalar), `art/results/323-zemin-yanyana.png` (zemin).

## Araç ve ayarlar
- **Merdivenler (1-5):** `create_map_object`, stil eşleme, 301 yöntemi. Referanssız kılavuz + maske (`tools/art/footprint_guide.py`), `view=high top-down`, `detail=low detail`, `shading=basic shading`, `outline=single color outline`. Tuvaller işteki karara göre: merdiven 80x112 (`--n 1`, boy 64), çıkan 112x104 (`--n 2 --axis x`, boy 44), inen 112x80 (boy 24). Ortak ek metin 301'deki gibi (taş merdivende "dark brown wood" ibaresi çıkarıldı).
- **Zeminler (6-8):** `create_tiles_pro`, stil modu, `style_images` = kabul edilmiş `iso_floor_oak_a.png` + `iso_floor_earth_a.png` (64x32), `style_options` hepsi açık. Tek çağrı, 3 numaralı tarif, araç 16 varyasyon döndürdü; maske içi boşluk 0 olan ve içerik olarak uyan 3 tanesi seçildi. Maske dışı opak pikseller yalnızca elmas maskesiyle kırpıldı (içerik yeniden boyanmadı): plank_a 14 px, plank_b 9 px, cellar 45 px.
- Hız sınırı: inen ahşap merdiven çağrısı iki kez "rate limit exceeded" aldı (diğer şeritler eşzamanlı çalışıyordu). Başka işlerin bitmesini bekleyip yeniden gönderildi (tarifin tek istisnası); ilk iki gönderim ücretlendirilmedi.

## Harcama
| Çağrı | Generation |
|---|---|
| map_object x5 (ladder, up, down, up_stone, down_stone) | 5 |
| create_tiles_pro x1 (16 varyasyon) | 10 |
| Toplam | 15 (cost_est 8, cost_max 16; 2x sınırı 16 içinde) |

`get_balance` sonrası kalan: **3570,5** (ortak bakiye, paralel şeritler dahil).

## Dosyalar
| Dosya | Piksel | Pivot x | Pivot y (alttan) |
|---|---|---|---|
| iso_item_ladder_se.png | 80x112 | 40 | 24 |
| iso_item_stairs_up_2x1_se.png | 112x104 | 56 | 32 |
| iso_item_stairs_down_2x1_se.png | 112x80 | 56 | 32 |
| iso_item_stairs_up_stone_2x1_se.png | 112x104 | 56 | 32 |
| iso_item_stairs_down_stone_2x1_se.png | 112x80 | 56 | 32 |
| iso_floor_upper_plank_a.png | 64x32 | 32 | 16 |
| iso_floor_upper_plank_b.png | 64x32 | 32 | 16 |
| iso_floor_cellar_dirt_a.png | 64x32 | 32 | 16 |

## Doğrulama (`validate_iso.py`)
| Dosya | Renk | Yarı saydam | Alt boşluk | Sonuç |
|---|---|---|---|---|
| ladder | 18 | 0 | 13 | geçti |
| stairs_up | 13 | 0 | 7 | geçti |
| stairs_down | **57 (>48)** | 0 | 7 | renk sınırı aşıldı; kural gereği `reduce_colors` yapılmadı, yalnızca raporlandı |
| stairs_up_stone | 22 | 0 | 7 | geçti |
| stairs_down_stone | 19 | 0 | 7 | geçti |
| plank_a / plank_b / cellar | 14 / 22(ham) / 12 | 0 | 0 | geçti (kırpmadan sonra maske dışı 0, içi boş 0) |

## Stil denetimi (`style.py check`)
| Dosya | Aile | Palete yakın | Doygunluk / aile | Sonuç |
|---|---|---|---|---|
| ladder | esya | %100 | x0.64 | GEÇTİ |
| stairs_up | esya | %100 | x0.85 | GEÇTİ |
| stairs_down | esya | %100 | x1.21 | GEÇTİ |
| stairs_up_stone | esya | %100 | x0.43 | UYARI: palete uzak, ailesinden soluk (gri taş) |
| stairs_down_stone | esya | %100 | x0.27 | UYARI: palete uzak, ailesinden soluk (gri taş) |
| plank_a | zemin | %100 | x1.35 | UYARI: ailesinden doygun |
| plank_b (ham) | zemin | %32 | x2.24 | KALDI: ham hâli `art/results/323-red/` |
| plank_b (fit) | zemin | %100 | x1.16 | GEÇTİ: **doygunluk x0,51** ile `style.py fit`, teslim edilen bu |
| cellar_dirt | zemin | %100 | x0.38 | UYARI: ailesinden soluk (koyu toprak, bilinçli) |

Not: `zemin` ailesinin kilidi kurulmadı (referans < 3), palet uyumu yalnızca uyarıdır.

## Döşeme testi (8x8, (32,16) adımı)
Dikiş parlaklık oranı (kenar bandı / karo ortalaması; 1,0 = dikiş yok): plank_a 0,979; plank_b 0,788 (ham ölçüm, fit öncesi); cellar 0,854. plank_b'de kenarlar belirgin koyu, dikiş çizgisi görünebilir; kullanıcı yan yana sayfasında bakmalı. Test PNG'leri geçici klasörde (commit edilmedi).

## Sorunlar ve notlar
- Tuvaller kararlaştırıldığı gibi (80/112 geniş) kullanıldı; ayak izi ve pivotlar 301 tablosuyla aynı.
- `stairs_down` (ahşap) 57 renk: sınır 48'i aştı, yalnızca raporlandı.
- Merdivenlerde ayak izi kaplaması/ışık/zemin kat duvarıyla birleşim ölçümleri otomatik yapılmadı; kullanıcı görsel onayı gerekir. Inen merdivenlerin "karanlık açıklık"ı sprite içinde taban elması rengiyle karışmış olabilir (down_wood'da taban koyu gri olarak çizildi; kılavuz rengi #786046 kalıntısı kontrol edilmedi).
- Üst kat zemini ile zemin kat zemini ardışık katmanda ayırt edilirliği ve bodrum zemini ile çimen karışmaması: bodrum zemini koyu gri-kahve (ortalama parlaklık 62), çimenle karışmaz; üst kat tahtası açık kahve (127/178).
- Araç eksiği önerisi: `footprint_guide.py` zemin karoları için yükseklik 0'da kullanılamaz; zemin yöntemi 335 gibi `create_tiles_pro` kaldı.
