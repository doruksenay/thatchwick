unity-import: pending

# 570t Kement tuzağı (rapor)

Yan yana sayfası: [570t-yanyana.png](570t-yanyana.png) (referanslar + yeni dosya; balık sepeti ve korkuluk v2 aynı sayfada).
Masaüstü kopyası: yerel (bulutta yapılamaz).

## Araç ve ayarlar
`create_map_object`, stil eşleme, referanssız kılavuz + maske (`footprint_guide.py --w 80 --h 56 --n 1 --height 14`), `high top-down`, `low detail`, `basic shading`, `single color outline`; 301 ortak eki + Köylü eki (`rough light pine wood, rope lashing`). 1 çağrı = 1 generation (cost_est 1, cost_max 3). Yeniden deneme gerekmedi. `get_balance` sonrası: 1148,6 (paralel şeritler de harcıyor).

## Dosya
| Yol | Piksel | Pivot x | Pivot y (alttan) |
|---|---|---|---|
| Assets/Art/Iso/_aday/iso_item_snare_se.png | 80x56 | 40 | 24 |

## Doğrulama
| Denetim | Sonuç |
|---|---|
| validate_iso (--max-colors 48) | geçti: 26 renk, yarı saydam 0, opak x14-65 y3-48, alt boşluk 7 satır |
| style.py check --family esya | GEÇTİ: palete yakın %93, ort. uzaklık 7,7, doygunluk x0,96 |
| Kılavuz rengi (#786046) kalıntısı | 0 piksel |
| Kırpma | yok (80x56 tam) |

## Sorunlar / notlar
- Görselde eğik fındık dalı ve toprak yaması var; yama "no ground" isteğine rağmen çıktı (taban elması toprak rengi). İlmek ve iki kazık küçük boyutta zor okunuyor; kullanıcı yan yana sayfasında ilmeğin okunup okunmadığına bakmalı.
- Yön: asimetrik değil, `_se` olarak adlandırıldı.
- Döşeme testi eşya için gerekmez.
