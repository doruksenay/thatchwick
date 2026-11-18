unity-import: pending

# 381 Duvar örtüleri (rapor): 2/3 teslim, review

Yan yana sayfası: [381-yanyana.png](381-yanyana.png). Reddedilen: `art/results/381-red/`. Masaüstü kopyası: yerel.

## Araç ve ayarlar
- `create_map_object` stil eşleme (306 yöntemi): `side` görünüm, `low detail`, `basic shading`, `single color outline`; açıklama + duvar eki + takım eki + ortak ek. 3 çağrı, paralel, çağrı başına 1 gen = 3 generation. Tek deneme.
- Kılavuz ve maske: `tools/art/wall_guide.py` depoda yok; `art/tmp/mk.py` ile (commit edilmez) üretildi: 96x64 tuval, duvar rengi (184,160,122) "/" eğimli paralelkenar, x 16-79, 24 px yüksek, 32 px yükselen; maske aynı şekil. İki kenar uzunluğu (64 px).
- get_balance sonrası: 4810,9 / 5000 (paralel şeritler de harcıyor).

## Dosyalar (pivot = görselin merkezi, ALTTAN)
| Dosya (`_aday/`) | Boyut | Pivot x | Pivot y (alttan) |
|---|---|---|---|
| iso_wall_item_painted_cloth_2w_sw_ne.png | 96x64 | 48 | 32 |
| iso_wall_item_kilim_hanging_2w_sw_ne.png | 96x64 | 48 | 32 |

Teslim edilmedi: `iso_wall_item_applique_hanging_2w_sw_ne.png` (aşağıda).

## Doğrulama
| Eşya | Opak kutu | Renk | Yarı saydam | Stil denetimi |
|---|---|---|---|---|
| boyalı bez | x16-79 y5-58 | 39 | 0 | GEÇTİ (%87, ort. uzaklık 9.4, x0.93) |
| kilim | x18-77 y7-54 | 19 | 0 | GEÇTİ (%93, 9.0, x0.86) |
| aplike | x16-79 y5-59 | 45 | 0 | KALDI (%27, 24.2, x1.43; palet dışı, ailesinden doygun) |

`validate_iso.py` üçü de geçti. Aplike için `style.py fit` denendi (doygunluk x0.81): hâlâ KALDI (%68 palete yakın, palet dışı renkler); teslim edilmedi. Ham ve fit hâli `381-red/` içinde (`applique_hanging.png`, `applique_fit.png`). Görsel olarak aplikede parlak turuncu figürler lacivert zeminde, palet dışı; şekil tanınır.

## Sorunlar / gözlem
- Aplike için yeniden üretim ya da kullanıcı kararı gerekli (tarifte figürler için daha soluk bir yün rengi, `muted ochre and faded red figures`). Bu iş tek deneme (`max_attempts: 1`), yeniden denenmedi.
- Işık/ayna: "\" yüzü aynayla, ışık sağ-üstten gelir. Işık yönü ve takım renk uyumu yan yana sayfasında göz kararı bakılmalı.
- Kilim: desen küçük (19 renk), elmas desen 96x64'te seçilir ama sade. Çerçeve takım eki nedeniyle koyu ahşap çıta.
- Kılavuz betiği kalıcı olmalıysa kod/PR oturumu `tools/art/wall_guide.py` eklemeli.
