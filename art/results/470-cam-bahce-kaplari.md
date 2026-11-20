unity-import: pending

# 470 Cam bahçe kapları (rapor)

Yan yana sayfası: [470-yanyana.png](470-yanyana.png) (2 aday + 7 referans). Teslim edilmeyenler: `art/results/470-red/`. Masaüstü kopyası: yerel. **Hiçbir dosya `Assets/Art/Iso/` altına teslim edilmedi** (stil denetimi KALDI).

## Araç ve ayarlar
`create_map_object`, stil eşleme, referanssız kılavuz + maske (383/302 yöntemi; kılavuz Python ile, `art/tmp/mk470.py`). `high top-down`, `low detail`, `basic shading`, `single color outline`, 301 ortak eki. Küçük: 32x40 tuval, 16x8 taban elması (alttan 4 px), maske yükseklik 24. Büyük: 80x88 tuval, 64x32 elmas (alttan 8 px), maske yükseklik 52. İki çağrı paralel, 1 gen x 2 = 2 generation (cost_max 8), ikinci deneme yok (şekil ve tanınma sorunu yok; sorun yalnızca palet). `get_balance` iş sonu: 3030,90.

## Sonuç özeti
| # | Eşya | Durum | Not |
|---|---|---|---|
| 1 | glass_garden | TESLİM EDİLMEDİ | tanınıyor (yeşil kapaklı cam fanus, ahşap taban); stil KALDI: palet %62, uzaklık 20,3. `fit` palet sorununu çözmedi |
| 2 | glass_garden_large | TESLİM EDİLMEDİ | tanınıyor (ahşap sehpada cam kasa, yeşil bitki); stil KALDI (palet %70, uzaklık 20,4, doygunluk x1,59); `fit` x0,72 sonrası doygunluk düzeldi ama palet %73 / uzaklık 14,9 ile hâlâ KALDI; ayrıca renk sayısı 53 > 48 |

Neden: orman camı yeşili ve parlak yeşil bitki esya ailesi paletinin dışında; bu şekil değil renk sorunu olduğundan kural gereği teslim yok. Kullanıcı yan yana sayfasına bakıp karar verebilir (gerekirse paletin yeşil rampı genişletilir ya da `reduce_colors` ile yeniden denenir).

## Dosyalar (hepsi `art/results/470-red/`)
| Yol | Piksel | Pivot x | Pivot y (alttan) |
|---|---|---|---|
| iso_item_glass_garden_se.png | 32x40 | 16 | 8 |
| iso_item_glass_garden_large_se.png | 80x88 | 40 | 24 |
| fit_l.png (büyük, fit x0,72) | 80x88 | 40 | 24 |

## Doğrulama (validate_iso, `--kind object --max-colors 48`)
| Dosya | Opak sınır kutusu | Renk | Yarı saydam | Alta değme | Sonuç |
|---|---|---|---|---|---|
| glass_garden | x8-23, y4-35 | 27 | 0 | yok (4 satır) | geçti |
| glass_garden_large | x10-69, y11-80 | 53 | 0 | yok (7 satır) | BAŞARISIZ (53 > 48) |

## Stil denetimi (`style.py check --family esya`)
| Dosya | Palete yakın | Ort. uzaklık | Doygunluk | Sonuç |
|---|---|---|---|---|
| glass_garden | %62 | 20,3 | x0,82 | KALDI |
| glass_garden (fit) | %62 | 20,3 | x0,82 | KALDI |
| glass_garden_large | %70 | 20,4 | x1,59 | KALDI |
| glass_garden_large (fit x0,72) | %73 | 14,9 | x1,12 | KALDI |

## Sorunlar
Palet dışı yeşil. Öneri: yeşil cam için aile paletine açık bir yeşil rampı eklenmesi ya da tarifin "greenish" yerine soluk gri-yeşil/zeytin tonu istemesi; ikinci deneme yeni iş olarak yazılabilir.
