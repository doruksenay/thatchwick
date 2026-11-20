unity-import: pending

# 443 Su ve dönem otomatları (rapor)

Yan yana sayfası: [443-yanyana.png](443-yanyana.png). Teslim edilmeyenler: `art/results/443-red/`. Masaüstü kopyası: yerel.

## Araç ve ayarlar
`create_map_object`, stil eşleme, `footprint_guide.py` kılavuzu + maske (1x1: 80x80, `--height 48`; havuz 2x2: 144x112, `--height 40`), `high top-down`, `low detail`, `basic shading`, `single color outline`. Açıklama: tarifteki metin + `no ground shadow, light from upper left`. 5 çağrı x 1 = 5 generation (cost_max 20 içinde), paralel gönderildi, yeniden deneme yok. `get_balance` iş sonu: 3382,9 (paralel şeritler de harcıyor).

## Sonuç özeti
| # | Eşya | Durum | Not |
|---|---|---|---|
| 1 | courtyard_pool_2x2 | TESLİM EDİLMEDİ | stil KALDI (palete yakın %79, palet dışı renkler); opak alan x24-119, 2x2 ayak izinin (128 px) altında, altta 30 satır boşluk; çerçeve havuz gibi tanınıyor |
| 2 | joke_fountain | teslim | GEÇTİ, doygunluk x1,24 |
| 3 | jazari_peacock | teslim (UYARI) | palete uzak (uzaklık 10,6), doygunluk x1,41; tavus kuşu ve sütun tanınıyor, hizmetkâr figürü ayırt edilmiyor |
| 4 | water_clock | TESLİM EDİLMEDİ | stil KALDI (palete yakın %54, doygunluk x1,68); `fit` sonrası da KALDI (palet dışı %22); sütun içinde cam/kum saati gibi, float tank ve kadran okunmuyor |
| 5 | eagle_lectern | teslim | GEÇTİ, doygunluk x1,05 |

## Dosyalar (Assets/Art/Iso/_aday/)
| Yol | Piksel | Pivot x | Pivot y (alttan) |
|---|---|---|---|
| iso_item_joke_fountain_se.png | 80x80 | 40 | 24 |
| iso_item_jazari_peacock_se.png | 80x80 | 40 | 24 |
| iso_item_eagle_lectern_se.png | 80x80 | 40 | 24 |

## Doğrulama (validate_iso, `--max-colors 48`)
| Dosya | Opak sınır kutusu | Renk | Yarı saydam | Sonuç |
|---|---|---|---|---|
| fountain | x27-52, y11-69 | 22 | 0 | geçti |
| peacock | x25-65, y7-71 | 37 | 0 | geçti |
| eagle | x14-65, y3-68 | 34 | 0 | geçti |
| pool (red) | x24-119, y33-81 | 47 | 0 | geçti (stil KALDI) |
| clock (red) | x27-52, y8-72 | 48 | 0 | geçti (stil KALDI) |

## Sorunlar
- Gölge pişirme, ışık yönü ve 2:1 ölçümü yapılmadı; nesne sprite'ları, görsel kontrol kullanıcıda.
- Havuz ve saat teslim edilmedi; yeniden deneme yok (`max_attempts: 1`). Yeni iş için havuza bir zemin karosu yaklaşımı (create_isometric_tile) önerilir; saat için daha yalın tarif (`tall wooden clock cabinet with brass dial`).
- Ham dosyalar `art/tmp443/` altında (commit dışı).
