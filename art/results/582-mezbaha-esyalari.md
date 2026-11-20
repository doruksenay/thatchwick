unity-import: pending

# 582 Mezbaha eşyaları (rapor)

Yan yana sayfası: [582-yanyana.png](582-yanyana.png). Teslim edilmeyen/ham denemeler: `art/results/582-red/`. Masaüstü kopyası: yerel.

## Araç ve ayarlar
`create_map_object`, stil eşleme, `footprint_guide.py` kılavuzu + maske (referanssız; 301/302 yöntemi), `high top-down`, `low detail`, `basic shading`, `single color outline`, 301 ortak eki. Tuval/yükseklik: kanca 80x80/44, dolap 112x96/52 (`--n 2 --axis x`), buz tekne 80x56/20, giderli 80x56/12, askı 80x80/40.
Çağrılar: 5 ilk deneme + 2 ikinci deneme (kanca: salıncak gibi tanınmadı; gider: oda köşesi çıktı) = 7 generation (cost_max 10 içinde). Hız sınırı hatası yok.
`get_balance` iş sonu: 1844,70 (paralel şeritler de harcıyor; bu işin payı 7).

## Sonuç özeti
| # | Eşya | Durum | Not |
|---|---|---|---|
| 1 | slaughter_hook | teslim (2. deneme) | 1. deneme salıncak (kırmızı listede). 2. denemede iki direk, üstte kancalar, önde tahta blok; blok yassı tahta gibi, kütüğe az benziyor ⚠ |
| 2 | meat_locker (2x1) | teslim | kapalı ahşap dolap; üstte tuz teknesi seçilmiyor (küçük yuvarlak ayrıntı), "kapaklı sandık" gibi |
| 3 | ice_trough | teslim | taş tekne (tuğla/ahşap gibi kahverengi yüz), üstü buz blokları; saman yok |
| 4 | drain_channel | TESLİM EDİLMEDİ | iki denemede de oda köşesi/zemin parçası (duvar + zemin), nesne değil. Şekil sorunu, `fit` çözmez |
| 5 | hide_rack | teslim (doygunluk x0,64 `fit`) | ham hâl x1,72 ile KALDI; `fit` ile GEÇTİ. Şekil, havlulu ve kâseli bir bank/tabure gibi; halat bobini yerine kâse ⚠ |

## Dosyalar (Assets/Art/Iso/_aday/)
| Yol | Piksel | Pivot x | Pivot y (alttan) |
|---|---|---|---|
| iso_item_slaughter_hook_se.png | 80x80 | 40 | 24 |
| iso_item_meat_locker_se.png | 112x96 | 56 | 32 |
| iso_item_ice_trough_se.png | 80x56 | 40 | 24 |
| iso_item_hide_rack_se.png | 80x80 | 40 | 24 |

## Doğrulama (validate_iso, `--max-colors 48`)
| Dosya | Opak sınır kutusu | Renk | Yarı saydam | Alta değme | Sonuç |
|---|---|---|---|---|---|
| kanca | x14-67, y1-68 | 25 | 0 | yok | geçti |
| dolap | x23-92, y14-88 | 44 | 0 | yok | geçti |
| buz tekne | x14-65, y6-48 | 37 | 0 | yok | geçti |
| askı | x11-68, y13-68 | 42 | 0 | yok | geçti |

## Stil denetimi (`style.py check --family esya`)
| Dosya | Palete yakın | Ort. uzaklık | Doygunluk | Sonuç |
|---|---|---|---|---|
| kanca | %98 | 5.6 | x0.82 | GEÇTİ |
| dolap | %100 | 5.5 | x0.63 | GEÇTİ |
| buz tekne | %93 | 8.9 | x1.06 | GEÇTİ |
| askı (fit) | %100 | 4.2 | x1.09 | GEÇTİ |
| gider (ilk deneme, teslim edilmedi) | %97 | 8.3 | x0.58 | UYARI |

## İstenmeyen ayrıntı
Et, kan ve hayvan yok; pişmiş gölge ve zemin lekesi yok (gider hariç: zemin ve duvar çizilmişti, o yüzden elendi). Ayak izi kaplaması ayrıca ölçülmedi; kullanıcı yan yana sayfasına bakmalı. Işık yönü ölçülmedi (yüzler görsel olarak sol-üstten).

## Sorunlar / öneri
- Gider kanalı için tarif ("shallow stone floor gutter") zemin parçası üretiyor. Öneri: zemin dekoru olarak ayrı bir yöntemle (yüzey karosu) ya da "stone drain grate with a bucket" tarifiyle yeni iş.
- Askı ve kanca şekli yan yana sayfasında değerlendirilmeli; gerekirse 582b ile yenilenir.
- PixelLab araçlarında `seed` yok; ikinci denemeler aynı kılavuz ve maskeyle yapıldı.
