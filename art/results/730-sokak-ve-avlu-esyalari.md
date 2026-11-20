unity-import: pending

# 730 Sokak ve avlu eşyaları (rapor)

Yan yana sayfası: [730-yanyana.png](730-yanyana.png) (7 aday: 5 teslim + notice/hitching çakışanlar; sol: esya referansları). Teslim edilmeyenler: `art/results/730-red/`.
Masaüstü kopyası: yerel (bulutta yapılamaz).

## Araç ve ayarlar
`create_map_object`, stil eşleme, kılavuz + özel maske (`tools/art/footprint_guide.py`, `--axis x`), `high top-down`, `low detail`, `basic shading`, `single color outline`, tarifteki açıklama + ortak ek. Her çağrı 1 generation, 9 çağrı = 9 (cost_max 24 içinde). Kılavuz yükseklikleri: tabela/pano 70, bağlama direği 30, ip askılar 56, kereste 36, taş 36, tekne 28.
Hız sınırı: bitişik bir çağrı "rate limit exceeded" aldı (üretim yok, ücret yok), bekleyip 1 kez yeniden gönderildi. Base64 aktarımında iki çağrı (tabela, pano) kodlama hatasıyla reddedildi (ücret yok), yeniden kodlanıp gönderildi.
Tabela ve pano: kılavuz 64 px tuvale sığmadığı için 80x112 üretildi, ortadan 64x112'ye (x 8-71) maskeyle kırpıldı (dışında opak piksel yok). Pivot y: kılavuzun verdiği 24 (tarifteki 16 değil; elmas merkezi), pivot x 32.
`get_balance` iş sonu: 1027,6 (paralel şeritler de harcıyor; bu işin payı 9).

## Sonuç özeti
| # | Eşya | Durum | Not |
|---|---|---|---|
| 1 | dükkân tabelası | TESLİM EDİLMEDİ | 50 renk (>48); `fit` sonrası da 50 |
| 2 | ilan panosu | ÇAKIŞMA, teslim edilmedi | `iso_item_notice_post_se.png` 432'den var (üzerine yazılmaz). Ham: stil KALDI (x1,72 doygun); `fit` (x0,67) sonrası GEÇTİ, 31 renk. `730-red/..._fit_432den_cakisir.png` |
| 3 | bağlama direği | ÇAKIŞMA, teslim edilmedi | `iso_item_hitching_post_se.png` 432'den var. Benim sürüm GEÇTİ, `730-red/` içinde |
| 4 | çamaşır ipi | teslim | GEÇTİ |
| 5 | boyalı kumaş askısı | TESLİM EDİLMEDİ | stil KALDI (%67 palet; `fit` sonrası da %68), boyalı renkler paletin dışı |
| 6 | ağ kurutma çerçevesi | teslim | GEÇTİ; altında ahşap zemin platformu ve fıçı/leğen var, ayak izi dışında içerik olabilir, bakılmalı |
| 7 | kereste yığını | teslim | GEÇTİ |
| 8 | taş blokları | teslim (UYARI) | ailesinden soluk x0,39 |
| 9 | çamaşır teknesi | teslim (UYARI) | palete uzak %87 |

## Dosyalar (Assets/Art/Iso/_aday/)
| Yol | Piksel | Pivot x | Pivot y (alttan) |
|---|---|---|---|
| iso_item_clothes_line_2x1_se_nw.png | 112x96 | 56 | 32 |
| iso_item_net_drying_rack_2x1_se_nw.png | 112x96 | 56 | 32 |
| iso_item_lumber_pile_2x1_se_nw.png | 112x80 | 56 | 32 |
| iso_item_stone_blocks_se.png | 80x80 | 40 | 24 |
| iso_item_wash_tub_se.png | 80x64 | 40 | 24 |

## Doğrulama (validate_iso, `--max-colors 48`)
| Dosya | Opak sınır kutusu | Renk | Yarı saydam | Sonuç |
|---|---|---|---|---|
| çamaşır ipi | x29-83, y13-80 | 31 | 0 | geçti |
| ağ çerçevesi | x10-102, y7-89 | 45 | 0 | geçti |
| kereste | x8-104, y7-73 | 22 | 0 | geçti |
| taş blokları | x13-68, y4-73 | 21 | 0 | geçti |
| çamaşır teknesi | x18-62, y12-54 | 45 | 0 | geçti |
| (red) tabela | x10-49, y3-105 | 50 | 0 | BAŞARISIZ (renk) |
| (red) bağlama direği | x15-63, y9-59 | 43 | 0 | geçti (çakışma) |
| (red) ilan panosu (fit) | x5-61, y7-98 | 31 | 0 | geçti (çakışma) |
| (red) boyalı kumaş | x17-95, y6-87 | 46 | 0 | geçti (stil KALDI) |

## Stil denetimi (`style.py check --family esya`)
| Dosya | Palete yakın | Ort. uzaklık | Doygunluk | Sonuç |
|---|---|---|---|---|
| çamaşır ipi | %100 | 4.1 | x0.93 | GEÇTİ |
| ağ çerçevesi | %100 | 6.6 | x1.11 | GEÇTİ |
| kereste | %100 | 4.6 | x0.64 | GEÇTİ |
| taş blokları | %100 | 7.4 | x0.39 | UYARI |
| çamaşır teknesi | %87 | 10.8 | x1.25 | UYARI |
| (red) bağlama direği | %99 | 7.8 | x1.05 | GEÇTİ |
| (red) ilan panosu ham | %97 | 10.5 | x1.72 | KALDI |
| (red) ilan panosu fit | %100 | 5.7 | x1.21 | GEÇTİ |
| (red) tabela | %92 | 10.6 | x1.18 | UYARI |
| (red) boyalı kumaş | %67 | 15.1 | x1.26 | KALDI |

## Yapılmayanlar / sorunlar
- `fit` yalnızca tarifteki "fit sonrası" izniyle denendi (esya kilitli ailedir; 301 kuralına göre normalde yapılmaz). Tabela ve kumaş askısı onunla da geçmedi.
- Çakışma: 2 ve 3 için dosya adı 432'den dolu. Karar kullanıcıda: yeni ad (ör. `_730` soneki) ya da 432'dekiler kalsın.
- Tabela ve boyalı kumaş için yeni iş (renk indirgeme ya da yeniden üretim) gerekir; tek deneme kuralı gereği yapılmadı.
- Ölçülmedi: ayna uygunluğu, ayak izi kaplaması sayısal değeri; zemin değil, döşeme testi yok.
- Öneri: `footprint_guide.py` 64 px tuvale sığan elmas üretmiyor (8 px kenar boşluğu); 64 genişlikli nesneler için araca `--margin 0` eklenebilir.
