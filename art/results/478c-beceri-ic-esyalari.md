unity-import: pending

# 478c Beceri tamamlama: iç mekân eşyaları ve bayram süslemesi (rapor): 3/4 teslim, review

Yan yana sayfası: [478c-yanyana.png](478c-yanyana.png). Teslim edilmeyen ve ham hâller: `art/results/478c-red/`. Masaüstü kopyası: yerel.

## Araç ve ayarlar
`create_map_object`, stil eşleme, referanssız kılavuz + maske (`footprint_guide.py --n 1`: peynir/ipek böceği rafı 80x88 yükseklik 44, davul 80x64 yükseklik 24), `high top-down`, `low detail`, `basic shading`, `single color outline`, 301 ortak eki + Köylü eki. Flama: `side`, duvar kılavuzu (paralelkenar 64x48, duvar rengi (184,160,122), maske +1 px; `art/tmp478c/wall.py`, commit edilmez), 306/405 yöntemi.
Çağrılar: 4 ilk deneme + 2 ikinci deneme (davul: sandık gibi çıktı, tanınmadı; flama: stil KALDI) = **6 generation** (cost_max 10 içinde). Paralel gönderildi.
`get_balance` iş sonu: 2505,9 (paralel şeritler de harcıyor; bu işin payı 6).

## Sonuç özeti
| # | Eşya | Durum | Not |
|---|---|---|---|
| 1 | peynir rafı | teslim (UYARI) | tekerlek peynirler okunuyor; çerçeve kafes gibi; palete uzak %86 |
| 2 | ipek böceği rafı | teslim | GEÇTİ; yaprak ve beyaz koza okunuyor |
| 3 | davul | teslim (2. deneme, doygunluk fit x0.62) | 1. deneme düz sandık (tanınmadı); 2. deneme varil biçimli davul, ip germe okunuyor, sopa seçilmiyor; 63 renk > 48 (raporlandı) |
| 4 | bayram flaması | TESLİM EDİLMEDİ | iki denemede de stil KALDI (palete yakın %26 ve %58: kırmızı/sarı palet dışı), `fit` düzeltmedi; ayrıca 55-56 renk. Soluk/toprak tonlu ya da farklı bir tarif gerek |

## Dosyalar (Assets/Art/Iso/_aday/; pivot x ve y ALTTAN, `footprint_guide` çıktısı)
| Dosya | Boyut | Pivot x | Pivot y (alttan) |
|---|---|---|---|
| iso_item_cheese_rack_se.png | 80x88 | 40 | 24 |
| iso_item_silkworm_rack_se.png | 80x88 | 40 | 24 |
| iso_item_drum_se.png | 80x64 | 40 | 24 |

## Doğrulama (validate_iso, `--max-colors 48`)
| Dosya | Renk | Yarı saydam | Opak kutu | Sonuç |
|---|---|---|---|---|
| peynir rafı | 38 | 0 | x9-70 y6-80 | geçti |
| ipek böceği rafı | 45 | 0 | x11-68 y7-80 | geçti |
| davul (fit) | 63 | 0 | x22-57 y10-54 | renk sınırı aşıldı, raporlandı |
| flama (1.) | 56 | 0 | x9-53 y3-45 | BAŞARISIZ (renk) |
| flama (2.) | 55 | 0 | x7-56 y4-47 | BAŞARISIZ (renk) |

## Stil denetimi (`style.py check --family esya`)
| Dosya | Palete yakın | Ort. uzaklık | Doygunluk | Sonuç |
|---|---|---|---|---|
| peynir rafı | %86 | 10.3 | x1.13 | UYARI |
| ipek böceği rafı | %89 | 9.8 | x1.11 | GEÇTİ |
| davul (fit) | %100 | 9.6 | x1.15 | GEÇTİ (ham x1.87, %56 KALDI) |
| davul 1. deneme (teslim değil) | %99 | 6.6 | x0.88 | GEÇTİ ama şekil sandık |
| flama 1. (teslim değil) | %26 | 39.5 | x1.66 | KALDI |
| flama 2. (teslim değil) | %58 | 20.1 | x1.36 | KALDI |
| flama 2. fit (teslim değil) | %61 | 15.8 | x1.14 | KALDI |

## Sorunlar
- Bayram flaması üretilemedi; yeni tarif ya da stil eşiği kararı kullanıcıda.
- Ölçülmedi: ışık sırası, ayna, ahşap ton tablosu; yan yana sayfasına bakın. Davulda sopa seçilmiyor.
- Döşeme testi eşyalar için geçerli değil.
- Araç önerisi: `tools/art/wall_guide.py` hâlâ yok.
