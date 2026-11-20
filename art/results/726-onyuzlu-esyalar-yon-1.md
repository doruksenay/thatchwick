unity-import: pending

# 726 Ön yüzlü eşyalar, bölüm 1 (rapor)

Yan yana: [726-yanyana.png](726-yanyana.png) (teslim edilen 4). Teslim edilmeyenler: `art/results/726-red/`. Masaüstü kopyası: yerel.

## Araç ve ayarlar
`create_map_object`, stil eşleme, `footprint_guide.py` kılavuz+maske (1x1: 80x96 `--height 56`; 2x1: 112x112 `--n 2 --axis y --height 48`; 3x1: 144x112 `--n 3 --axis y --height 48`), high top-down, low detail, basic shading, single color outline. İstem: tarif + `no face, no person, no text, no ground shadow`. 8 çağrı = 8 generation. Divan çağrısı base64 bozulması yüzünden hata verdi (ücretsiz), kılavuz yeniden kodlanıp bir kez gönderildi. get_balance sonu: 874,6 (paralel şeritler de harcıyor).

## Sonuç
| Eşya | Durum | Not |
|---|---|---|
| chair_painted | teslim | GEÇTİ; yön: sw |
| throne_chair | teslim (UYARI) | doygun x1.29; yön: sw |
| bedside_stand | teslim | GEÇTİ; yön: sw |
| bread_rack | teslim (UYARI) | doygun x1.29; ayağında koyu zemin plakası var (pişmiş zemin sayılabilir, kullanıcı baksın); yön: sw |
| bench_wood_2x1 | TESLİM EDİLMEDİ | uzun kenar sol-aşağıdan sağ-yukarıya: yön şartı ihlali (dokuma tezgâhı hatası) |
| cushioned_divan_3x1 | TESLİM EDİLMEDİ | aynı yön hatası |
| chest_iron_2x1 | TESLİM EDİLMEDİ | uzun yüz sağ-yukarı yükseliyor (yön hatası) |
| spice_chest | TESLİM EDİLMEDİ | 54 renk > 48; stil KALDI (x1.56), `fit` ile stil GEÇTİ (x1.15) ama renk yine 54 |

## Dosyalar (Assets/Art/Iso/_aday/)
| Yol | Piksel | Pivot x | Pivot y (alttan) |
|---|---|---|---|
| iso_item_chair_painted_sw.png | 80x96 | 40 | 24 |
| iso_item_throne_chair_sw.png | 80x96 | 40 | 24 |
| iso_item_bedside_stand_sw.png | 80x96 | 40 | 24 |
| iso_item_bread_rack_sw.png | 80x96 | 40 | 24 |

## Doğrulama (validate_iso --kind object --max-colors 48)
| Dosya | Renk | Alt opak satır | Sonuç |
|---|---|---|---|
| chair_painted | 25 | 86 | geçti |
| throne_chair | 37 | 87 | geçti |
| bedside_stand | 41 | 84 | geçti |
| bread_rack | 38 | 88 | geçti |
| bench_wood_2x1 (red) | 31 | 104 | geçti (yön reddi) |
| cushioned_divan_3x1 (red) | 35 | 104 | geçti (yön reddi) |
| chest_iron_2x1 (red) | 47 | 104 | geçti (yön reddi) |
| spice_chest (red) | 54 | 83 | BAŞARISIZ |

## Stil denetimi (esya)
| Dosya | Sonuç |
|---|---|
| chair_painted | GEÇTİ (x1.17) |
| throne_chair | UYARI (x1.29) |
| bedside_stand | GEÇTİ (x0.86) |
| bread_rack | UYARI (x1.29) |
| bench / divan / chest_iron | GEÇTİ (yön nedeniyle ret) |
| spice_chest | KALDI (x1.56) |

## Sorular / öneri
- 2x1 ve 3x1 kılavuzlarını `--axis y` ile yaptım; uzun kenar sağ-yukarı yükseldi (yanlış yön). Tek deneme kuralı gereği tekrarlamadım. Yeni iş (726b): bench, divan, chest_iron için `--axis x` kılavuzu denensin; spice_chest renk sınırı için de yeniden.
- 1x1'lerde ön yüz yönü görsel kontrolle sw sayıldı; kullanıcı yan yana sayfasına baksın.
- Döşeme/dikiş testi: zemin işi değil.
