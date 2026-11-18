unity-import: pending

# 370 Harita kenarı ormanı (rapor)

Yan yana sayfası: [370-yanyana.png](370-yanyana.png). Teslim edilmeyen denemeler: `art/results/370-red/`.
Masaüstü kopyası: yerel.

## Araç ve ayarlar
`create_map_object`, stil eşleme, referanssız kılavuz + maske (`footprint_guide.py`, 301 yöntemi), `high top-down`, `low detail`, `basic shading`, `single color outline`, tarifteki ortak ek. 6 çağrı + 4 ikinci deneme (a, c, e, f) = 10 generation (b'nin ilk gönderimi base64 hatasıyla reddedildi, yeniden gönderildi; ücretlendirilmedi). `get_balance` iş sonu: 4666,9 (paralel şeritler de harcıyor; bu işin payı 10). `bitki` ailesinin stil kilidi kurulmamış (referans < 3); 372/371 görselleri tuvale konmadı (301 yöntemi referanssız).

## Sonuç özeti
| # | Dosya | Durum | Not |
|---|---|---|---|
| 1 | iso_edge_trees_a | teslim (fit) | Küme değil, tek koyu meşe (ilk deneme). 2. deneme zemin elması çıktı, teslim değil. Ham doygunluk x2.95, fit sonrası x1.15 |
| 2 | iso_edge_trees_b | teslim (fit) | Üç çam kümesi. Ham x2.73, fit sonrası x1.17 |
| 3 | iso_edge_trees_c | TESLİM EDİLMEDİ | İki denemede de altında yeşil zemin elması (zemin parçası), şekil sorunu |
| 4 | iso_edge_trees_d | teslim (fit) | Genç meşe/fındık öbeği; renk sayısı 49 > 48 (301 kuralı: yalnızca raporla), doygunluk x1.15 |
| 5 | iso_edge_undergrowth_2x1_a | TESLİM EDİLMEDİ | İki denemede de toprak yanlı yükseltilmiş çimen bloğu (zemin tuğlası), çalılık şeridi değil |
| 6 | iso_edge_undergrowth_2x1_b | TESLİM EDİLMEDİ | Aynı sorun (çit bloğu, toprak yan) |

## Dosyalar (Assets/Art/Iso/_aday/)
| Yol | Piksel | Pivot x | Pivot y (alttan) |
|---|---|---|---|
| iso_edge_trees_a.png | 80x112 | 40 | 24 |
| iso_edge_trees_b.png | 80x112 | 40 | 24 |
| iso_edge_trees_d.png | 80x112 | 40 | 24 |

## Doğrulama (validate_iso, --max-colors 48, footprint 8,72,64,32)
| Dosya | Renk | Yarı saydam | Alta değme | Sonuç |
|---|---|---|---|---|
| a (fit) | <=48 | 0 | yok (alt boşluk 22) | geçti |
| b (fit) | <=48 | 0 | yok (alt boşluk 18) | geçti |
| d (fit) | 49 | 0 | yok (alt boşluk 23) | renk sınırı 1 fazla (⚠) |

## Stil denetimi (`style.py check --family bitki`, fit sonrası)
| Dosya | Palete yakın | Ort. uzaklık | Doygunluk | Sonuç |
|---|---|---|---|---|
| a | %100 | 12.0 | x1.15 | UYARI (palete uzak) |
| b | %98 | 10.4 | x1.17 | UYARI (palete uzak) |
| d | %45 | 22.5 | x1.15 | UYARI (palet dışı) |

Ham hâller (a x2.95, b x2.73, d x1.44; ilk üçü KALDI/UYARI) `370-red/` altında; düzeltme `style.py fit` ile.

## Sorunlar
- 12x6 şerit dizilim sayfası yapılmadı: yalnızca 3 öbek teslim edildi, çalılık şeritleri ve c yok.
- Öbekler beklendiği kadar koyu ve ayrıntısız değil; a tek ağaç. Kullanıcı karar verir.
- Çalılık şeritleri ve c için yeni tarif gerekir: stil eşleme modu zemin/blok çıkarıyor. Öneri: `create_map_object` basic modu (referanssız, saydam arka plan) ya da 1x1 küçük çalı öbekleri.
- Zemin döşeme testi gerekmedi (nesne).
