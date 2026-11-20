unity-import: pending

# 481 Mevsim Defteri takımı (rapor): 2/4 teslim, review

Yan yana sayfası: [481-yanyana.png](481-yanyana.png) (sırayla: yaz demeti, kış feneri, ilkbahar kurdelesi, sonbahar sepeti; son ikisi teslim EDİLMEDİ, yalnızca karşılaştırma için). Teslim edilmeyenler ve ham hâller: `art/results/481-red/`. Masaüstü kopyası: yerel.

## Araç ve ayarlar
`create_map_object`, stil eşleme (özel maske), `low detail`, `basic shading`, `single color outline`. Duvar parçaları (1, 2): `view: side`, 48x64, "/" eğimli duvar paralelkenarı kılavuzu (renk 184,160,122; `tools/art/wall_guide.py` yok, eşdeğeri `art/tmp481/` altında, commit edilmez), açıklamaya `mounted flat against a wall that slopes up to the right (isometric), seen from the room side` eklendi. Masa üstü parçalar (3, 4): `view: high top-down`, 32x32, küçük taban elmas kılavuzu (28x14, yuva S), maske elmasın 20 px yukarı uzatılmışı. 4 çağrı paralel, 1 gen = 4 generation (cost_max 12). Tek deneme. Doygunluk düzeltmesi `style.py fit` (ücretsiz).
`get_balance` sonrası: 2480.90 (paralel şeritler de harcıyor).

## Dosyalar (teslim, `Assets/Art/Iso/_aday/`)
| Yol | Piksel | Pivot x | Pivot y (alttan) |
|---|---|---|---|
| iso_wall_item_defter_summer_sheaf_sw_ne.png | 48x64 | 24 | 32 |
| iso_item_defter_winter_lantern_se.png | 32x32 | 16 | 16 |

Pivotlar görselin merkezi (duvar eşyası kuralı); masa üstü parçada taban merkezi y≈7 (alttan) olabilir, kod oturumu karar verir.

## Doğrulama
| Dosya | validate_iso | Stil denetimi | Not |
|---|---|---|---|
| yaz demeti | geçti (33 renk, 0 yarı saydam) | UYARI (%94 palete yakın, ort. 10.4, x1.15) | ham hâl KALDI (%9 palete yakın, x2.18); `fit` ile doygunluk x0.53 |
| kış feneri | geçti (24 renk) | UYARI (`--light`; ışık pikseli %11 denetim dışı, %85 palete yakın, x0.42 soluk) | |
| ilkbahar kurdelesi | BAŞARISIZ: 51 renk > 48 | `fit` sonrası GEÇTİ (%92, x1.08); ham KALDI (%77, x1.75) | renk sınırı aşıldığı için TESLİM EDİLMEDİ |
| sonbahar sepeti | geçti (20 renk) | `fit` sonrası KALDI (%78, palet dışı renkler); ham KALDI (%51, x2.31) | TESLİM EDİLMEDİ |

Döşeme testi geçerli değil (zemin değil). Kış fenerinde alev (mum) var, `--light` kullanıldı.

## Sorunlar
- İlkbahar kurdelesi: görünüm uygun (kurdele, yeşil yaprak), ama 51 renk; tek deneme kuralı gereği renk indirgeme yapılıp teslim edilmedi. Kod oturumu/kullanıcı `reduce_colors` ile 48'e indirgemeyi onaylarsa `fit_...ribbon` dosyası (`481-red/`) kullanılabilir.
- Sonbahar sepeti: palet dışı renkler (kırmızı elma, üzüm); `fit` yetmedi. Yeniden üretim gerekir (yeni iş).
- Yaz demeti: kırmızı yün bağ tarife uygun; fit ile soluklaştırıldı. Işık/ayna ölçümü yapılmadı, yan yana sayfasına bakın.
