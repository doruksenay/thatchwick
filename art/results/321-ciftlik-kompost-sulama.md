unity-import: pending

# 321 Çiftlik eşyaları (rapor)

Yan yana sayfası: [321-yanyana.png](321-yanyana.png) (yalnızca teslim edilen 3 dosya). Teslim edilmeyenler: `art/results/321-red/`.
Masaüstü kopyası: yerel.

## Araç ve ayarlar
`create_map_object`, stil eşleme, 301 yöntemi (referanssız kılavuz + maske, `footprint_guide.py`), `high top-down`, `low detail`, `basic shading`, `single color outline`, tarifteki ortak ek. Çağrı başına 1 gen: 5 ilk + 1 ikinci deneme (kompost) = 6 gen (cost_max 9). Oluk çağrısının ilki base64 hatasıyla üretime girmeden reddedildi (ücret yok). `get_balance` sonrası: 905,7 (paralel şeritler de harcıyor).

## Sonuç
| # | Eşya | Durum | Not |
|---|---|---|---|
| 1 | kompost yığını | teslim (2. deneme) | 1. deneme: kırmızı domatesle dolu sandık (yığın değil, Yeni Dünya bitkisi); 2. deneme koyu bitki yığını, tanınıyor |
| 2 | tohum yapıcı 2x1 | teslim | |
| 3 | sulama hendeği | TESLİM EDİLMEDİ | stil denetimi KALDI (palete %44, doygunluk x1.73: mavi su palet dışı) |
| 4 | kova-makara | teslim (UYARI) | palete %80; boy 69 px > köylü 48 px ⚠ kullanıcı bakmalı |
| 5 | su oluğu 2x1 | TESLİM EDİLMEDİ | stil denetimi KALDI (%55, x1.59, mavi su) |

## Dosyalar
| Yol | Piksel | Pivot x | Pivot y (alttan) |
|---|---|---|---|
| Assets/Art/Iso/_aday/iso_item_compost_heap_se.png | 80x64 | 40 | 24 |
| Assets/Art/Iso/_aday/iso_item_seed_maker_2x1_se.png | 112x80 | 56 | 32 |
| Assets/Art/Iso/_aday/iso_item_bucket_pulley_se.png | 72x88 | 40 | 24 |

## Doğrulama (validate_iso, max 48 renk) ve stil denetimi
| Dosya | Opak kutu | Renk | Yarı saydam | Alta değme | Palete yakın | Doygunluk | Stil |
|---|---|---|---|---|---|---|---|
| kompost (2.) | x13-67, y8-52 | 13 | 0 | yok | %96 | x1.06 | GEÇTİ |
| tohum yapıcı | x16-95, y5-72 | 33 | 0 | yok | %100 | x0.85 | GEÇTİ |
| kova-makara | x15-58, y7-75 | 15 | 0 | yok | %80 | x0.91 | UYARI |
| hendek (red) | x9-59, y6-32 | 21 | 0 | yok | %44 | x1.73 | KALDI |
| oluk (red) | x24-91, y5-48 | 31 | 0 | yok | %55 | x1.59 | KALDI |

## Sorunlar
- Palet referansında mavi yok; su içeren eşyalar stil denetiminden geçemiyor. Kullanıcı kararı: palete su mavisi eklenmesi ya da bu iki dosyanın (`_red`) yine de kabulü.
- Ayak izi kaplaması ve ışık/ayna ölçümü yapılmadı (otomatik betik yok); kullanıcı yan yana sayfasına bakmalı. Hendek ve oluğun çimen üzerindeki ayırt edilirliği denenmedi.
- Zemin döşeme testi bu iş için geçerli değil (eşya).
