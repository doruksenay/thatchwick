unity-import: pending

# 366 Kalıntılar (rapor)

Yan yana sayfası: [366-yanyana.png](366-yanyana.png) (teslim edilen 8 kalıntı + 7 referans). Teslim edilmeyen: `art/results/366-red/` (kristal). Masaüstü kopyası: yerel.

## Araç ve ayarlar
`create_map_object`, stil eşleme, referanssız kılavuz + özel maske (382/301 yöntemi). Kılavuz ve maske Python ile `art/tmp366/` altında üretildi (`footprint_guide.py` küçük tabanlara uymuyor). S: 40x40 tuval, 16x8 taban elması (alt kenardan 4 px yukarıda, #786046), maske tabanın 20 px yukarı uzatılmışı + 1 px. M (temel taşı): 48x48, 22x11 taban, 28 px. `high top-down`, `low detail`, `basic shading`, `single color outline`, 301 ortak eki. 9 çağrı x 1 gen = **9 generation** (cost_max 16). İkinci deneme yok.
Hız sınırı: temel taşı ve gömülü küp çağrıları ilk gönderimde "rate limit exceeded" döndü (üretim başlamadı, harcama yok); 30 sn sonra her biri bir kez yeniden gönderildi. `get_balance` iş sonu: 4520,90 (paralel şeritler de harcıyor; bu işin payı 9).

## Sonuç özeti
| # | Eşya | Durum | Not |
|---|---|---|---|
| 1 | temel taşı (M) | teslim | GEÇTİ. Dikdörtgen taş, "mason işareti" okunmuyor |
| 2 | gömülü küp | teslim | UYARI: palete uzak (10.8), x1.32 |
| 3 | nal | teslim | GEÇTİ. Bordoya yakın koyu, paslı okunuyor |
| 4 | fosil | teslim | GEÇTİ. Yumurta/taş gibi, kabuk dokusu zayıf |
| 5 | madenci lambası | teslim | UYARI: x1.26; lamba yerine küçük testi gibi okunuyor |
| 6 | kazma başı | teslim | UYARI: palete uzak (14.6), pembemsi demir |
| 7 | sikke | teslim | UYARI: soluk (x0.39); gümüş için uygun |
| 8 | kemik tarak | teslim | GEÇTİ. Tarak yerine çivi/kama gibi okunuyor, zayıf |
| 9 | kristal | TESLİM EDİLMEDİ | ham: palet %39, x1.76 (KALDI); `fit` x0.65 sonrası %63, hâlâ KALDI (kırmızı/mercan, "berrak" değil). Ham ve fit `366-red/`de |

## Dosyalar (Assets/Art/Iso/_aday/)
Pivot = kılavuzdaki küçük taban elmasının merkezi.
| Yol | Piksel | Pivot x | Pivot y (alttan) |
|---|---|---|---|
| iso_decor_relic_foundation_stone.png | 48x48 | 24 | 9 |
| iso_decor_relic_buried_jar.png | 40x40 | 20 | 8 |
| iso_decor_relic_horseshoe.png | 40x40 | 20 | 8 |
| iso_decor_relic_fossil.png | 40x40 | 20 | 8 |
| iso_decor_relic_miner_lamp.png | 40x40 | 20 | 8 |
| iso_decor_relic_pick_head.png | 40x40 | 20 | 8 |
| iso_decor_relic_coin.png | 40x40 | 20 | 8 |
| iso_decor_relic_bone_comb.png | 40x40 | 20 | 8 |

## Doğrulama (`validate_iso --kind object --max-colors 48`)
| Dosya | Opak sınır | Renk | Yarı saydam | Alt boşluk | Kılavuz rengi | Sonuç |
|---|---|---|---|---|---|---|
| temel taşı | x12-36, y7-43 | 17 | 0 | 4 | 0 | geçti |
| gömülü küp | x11-29, y8-31 | 9 | 0 | 8 | 0 | geçti |
| nal | x11-29, y8-30 | 7 | 0 | 9 | 0 | geçti |
| fosil | x11-29, y8-31 | 15 | 0 | 8 | 0 | geçti |
| lamba | x12-28, y8-29 | 9 | 0 | 10 | 0 | geçti |
| kazma başı | x14-26, y8-31 | 6 | 0 | 8 | 0 | geçti |
| sikke | x11-29, y10-29 | 15 | 0 | 10 | 0 | geçti |
| tarak | x13-26, y9-30 | 10 | 0 | 9 | 0 | geçti |
| kristal (ham) | x12-29, y8-34 | 18 | 0 | 5 | 0 | geçti |

## Stil denetimi (`style.py check --family esya`)
| Dosya | Palete yakın | Ort. uzaklık | Doygunluk | Sonuç |
|---|---|---|---|---|
| temel taşı | %100 | 5.4 | x0.89 | GEÇTİ |
| gömülü küp | %95 | 10.8 | x1.32 | UYARI |
| nal | %100 | 5.2 | x0.73 | GEÇTİ |
| fosil | %100 | 6.8 | x1.11 | GEÇTİ |
| lamba | %100 | 6.9 | x1.26 | UYARI |
| kazma başı | %100 | 14.6 | x1.01 | UYARI |
| sikke | %100 | 8.1 | x0.39 | UYARI |
| tarak | %100 | 4.6 | x1.11 | GEÇTİ |
| kristal (ham) | %39 | 27.0 | x1.76 | KALDI |
| kristal (fit) | %63 | 17.7 | x1.15 | KALDI |

## Gözlem
- Tüm eşyalar 40x40 içinde ortalı, tabana değmiyor, yarı saydam yok, gölge pişirilmemiş, ışık sol-üstten (gözle). Ölçüm yapılmadı. Döşeme testi eşya olduğu için uygulanmadı.
- Okunabilirlik zayıf olanlar: lamba (testi gibi), tarak (kama gibi), temel taşı (işaret yok). Kullanıcı kararına bırakılır.

## Sorunlar ve öneriler
- Kristal için yeniden deneme önerisi: açıklamaya "pale grey-white translucent quartz, no red, no orange" eklemek.
- `footprint_guide.py` için `--base-w` önerisi (382 ile aynı).
