unity-import: pending

# 430 Yaban haritaları: maden ağzı ve saz kümesi (rapor)

Yan yana sayfası: [430-yanyana.png](430-yanyana.png). Masaüstü kopyası: yerel (bulutta yapılamaz). Ham saz (doygunluk düzeltmesi öncesi): `art/results/430-red/reed_raw.png`.

## Araç ve ayarlar
`create_map_object`, stil eşleme, referanssız kılavuz + maske (`footprint_guide.py`), `high top-down`, `low detail`, `basic shading`, `single color outline`, tarifteki ortak ek. Maden ağzı: 144x112, `--n 2 --m 2` (araçta `--axis both` yok; 2x2 için n=2, m=2), yükseklik 48. Saz: 80x72, 1x1, yükseklik 40. İki çağrı paralel, her biri 1 generation; ilk iki gönderim base64 aktarım bozulması nedeniyle üretime girmeden reddedildi (ücret yok), kılavuz 1 bit palete indirilip yeniden gönderildi.
Harcanan 2 generation. `get_balance` sonrası: 3497.90 (paralel şeritler de harcıyor).

## Dosyalar (Assets/Art/Iso/_aday/)
| Yol | Piksel | Pivot x | Pivot y (alttan) |
|---|---|---|---|
| iso_item_mine_mouth_2x2_se.png | 144x112 | 72 | 40 |
| iso_item_forage_reed_se.png | 80x72 | 40 | 24 |

## Doğrulama
| Dosya | validate_iso | Renk | Opak sınır | Stil denetimi |
|---|---|---|---|---|
| maden ağzı | geçti | 41 | x10-133, y5-104 | UYARI (palete %86, ort. uzaklık 13.2, doygunluk x0.58 soluk) |
| saz (fit sonrası) | geçti | - | x12-67, y5-64 | UYARI (palete %97, uzaklık 10.7, doygunluk x1.14) |
| saz (ham) | geçti (45 renk) | 45 | | KALDI (palete %50, x1.86 çok doygun) → teslim edilmedi |

Saz: `style.py fit` ile doygunluk x0.62 indirildi (ham `430-red/`).
Işık: maden ağzı sol 73 / sağ 84 (sağ biraz açık, ⚠ sol-üst ışıkla tam uyumlu değil), saz sol 41 / sağ 41. Maden ağzı iç karanlığı: en koyu piksel parlaklığı 3 (kontur olabilir); iç boşluğun tam siyah olup olmadığına kullanıcı yan yana sayfasından bakmalı (şart: koyu kahve-gri).
Döşeme testi: eşya olduğu için yapılmadı.

## Sorunlar
- İki görselde de altta zemin elması var (311'deki "no ground" tutmama sorunu): maden ağzında taş zemin, sazda çamur zemin. Çamur tarife uygun; maden ağzındaki taş zemin yerinde zaten taş zemin olduğundan kabul edilebilir. Kullanıcı karar versin.
- Saz tanınıyor (kahverengi başaklı kamış); maden ağzı tanınıyor (kaya, ahşap kasa, kova; ray ve elektrik yok).
- `seed` yok, ikinci deneme yapılmadı (UYARI ile geçti).
