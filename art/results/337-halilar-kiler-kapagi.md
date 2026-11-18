unity-import: pending

# 337 Halılar ve kiler kapağı (rapor)

Yan yana sayfası: [337-yanyana.png](337-yanyana.png). Döşeme testi (rush kilim, 8x8): [337-doseme-testi.png](337-doseme-testi.png), dikiş parlaklık oranı 0,94. Teslim edilmeyenler: `art/results/337-red/`.
Masaüstü kopyası: yerel.

## Araç ve ayarlar
`create_map_object` stil eşleme, referanssız kılavuz + maske (`tools/art/footprint_guide.py`; 2x2 ve 3x2 için `--m` ikinci eksen parametresi eklendi), `high top-down`, `low detail`, `basic shading`, `single color outline`, ortak ek metin (koyu kahve ahşap ibaresi yalnızca kapakta). 7 çağrı x 1 gen = 7 (cost_max 12). Yeniden deneme yok (max_attempts 1). `get_balance` sonrası 885,7 (paralel şeritler de harcıyor).

## Sonuç
| Eşya | Durum | Not |
|---|---|---|
| rush kilim 1x1 | TESLİM EDİLMEDİ | stil KALDI (palete uzak, doygunluk x1,58); tahta gibi göründü |
| küçük yün halı 2x1 | TESLİM EDİLMEDİ | KALDI (doygunluk x1,56) |
| yolluk 3x1 | teslim | GEÇTİ; tahta pano gibi, kilim dokusu zayıf |
| post 1x2 | TESLİM EDİLMEDİ | KALDI (doygunluk x1,84) |
| yün halı 2x2 | TESLİM EDİLMEDİ | KALDI (doygunluk x2,01); düz krem, desen yok |
| desenli dokuma 3x2 | teslim | GEÇTİ; koyu kahve (tarifteki "açık ton" değil), kullanıcı bakmalı |
| kiler kapağı | teslim (UYARI) | ailesinden soluk (x0,57) |

## Dosyalar (Assets/Art/Iso/_aday/)
| Yol | Piksel | Pivot x | Pivot y (alttan) |
|---|---|---|---|
| iso_item_rug_runner_3x1_se.png | 144x80 | 72 | 40 |
| iso_item_rug_woven_large_3x2_se.png | 176x96 | 88 | 48 |
| iso_item_cellar_hatch_2x1_se.png | 112x80 | 56 | 32 |

Not: kapak adı tarifte `2x1` ama ayak izi SW-NE ekseninde (`--axis y`) üretildi.

## Doğrulama (validate_iso, ayak izi, en çok 48 renk)
| Dosya | Renk | Yarı saydam | Kapsama | Maske dışı |
|---|---|---|---|---|
| rush | 9 | 0 | %100 | 0 |
| küçük yün | 29 | 0 | %100 | 0 |
| yolluk | 19 | 0 | %100 | 0 |
| post | 28 | 0 | %89 | 0 |
| yün 2x2 | 29 | 0 | %100 | 0 |
| desenli 3x2 | 22 | 0 | %100 | 0 |
| kapak | 27 | 0 | %100 | 0 |

Hepsi `validate_iso` geçti; kırpma gerekmedi.

## Stil denetimi (`style.py check --family esya`)
| Dosya | Palete yakın | Ort. uzaklık | Doygunluk | Sonuç |
|---|---|---|---|---|
| kapak | %100 | 7,5 | x0,57 | UYARI |
| post | %100 | 7,8 | x1,84 | KALDI |
| yolluk | %100 | 8,8 | x0,83 | GEÇTİ |
| rush | %100 | 10,8 | x1,58 | KALDI |
| yün 2x2 | %97 | 4,8 | x2,01 | KALDI |
| küçük yün | %100 | 7,7 | x1,56 | KALDI |
| desenli 3x2 | %100 | 6,4 | x0,61 | GEÇTİ |

## Sorunlar
Dört açık tonlu (krem) halı aile doygunluğunun üstünde kaldı; "açık ton" isteği ile stil kapısı çelişiyor. Yeniden deneme hakkı yok; yeni deneme (örn. gri-bej, soluk tarif) kullanıcı kararı.
