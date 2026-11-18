unity-import: pending

# 322 Toplama düğümleri ikinci set (rapor)

Sonuç: **1/8 teslim** (kozalak). Diğer 7'si `style.py check --family bitki` denetiminde KALDI (310/311 ile aynı neden: bitki ailesinde 2 referans, palet/doygunluk kapısı). Yan yana: [322-yanyana.png](322-yanyana.png). Reddedilenler `art/results/322-red/*_raw.png`. Masaüstü kopyası: yerel.

## Araç ve ayarlar
`create_map_object` stil eşleme, `footprint_guide.py` kılavuz+maske (tuval 80x72, 1x1, yükseklik tarifteki: 16/20/30/12/18/26/10/8), high top-down, low detail, basic shading, single color outline, 311'deki ortak ek. Tuval 64x56 yerine 80x72 (311 gibi). Pivot x=40, y(alttan)=24.
Çağrı: 8 x 1 gen = 8. Üç çağrı hız sınırına takıldı (ücretsiz), bir maske base64 bozulması reddedildi; hepsi bir kez yeniden gönderildi. İkinci deneme yapılmadı (stil kapısı 310/311'de ikinci denemeyle de geçilmedi). `get_balance` iş başı 885,70; iş sonu okunmadı (paralel şeritler harcıyor), bu işin payı 8.

## Stil denetimi
| Parça | Sonuç | Palete yakın | Doygunluk | Not |
|---|---|---|---|---|
| garlic | KALDI | %11 | x2.55 | tanınır, zemin yok |
| nettle | KALDI | %82 | x1.96 | çok koyu, ayrıntı seçilmiyor |
| elder | KALDI | %74 | x1.25 | çalı değil ağaç, altında çim elması |
| strawberry | KALDI | %35 | x1.96 | toprak bloğu üstünde |
| chestnut | KALDI | %13 | x2.22 | kestane değil, mor meyve + yapraklar; tanınmıyor |
| rosehip | KALDI | %16 | x2.14 | gül çiçekleri, kuşburnu değil |
| pinecone | GEÇTİ | %84 | x1.03 | 2-3 uzun kahverengi koni, kar yok; kakao kabuğuna benziyor, zayıf |
| cress | KALDI | %20 | x2.36 | tanınır ama parlak yeşil |

## Dosyalar
| Yol | Boyut | Pivot x | Pivot y (alttan) |
|---|---|---|---|
| Assets/Art/Iso/_aday/iso_forage_pinecone_se.png | 80x72 | 40 | 24 |

validate_iso (pinecone): geçti, 25 renk, yarı saydam 0, opak x15-67 y22-53 (tabana değmiyor: alt 10 px boşluk).

## Sorunlar
- Bitki stil kapısı gevşetilmeli ya da referans eklenmeli (310/311/322 aynı).
- Kestane, kuşburnu, elder (çalı), strawberry (zemin) yeniden üretim ister; 14 düğümlük yan yana ve döşeme testi teslim olmadığından yapılmadı.
- Kozalak tanınırlığı zayıf; kullanıcı kararı.
