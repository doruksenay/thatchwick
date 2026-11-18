unity-import: pending

# 341 Ambar, buzluk, buzhane (rapor)

Yan yana sayfası: [341-yanyana.png](341-yanyana.png). Teslim edilmeyen: `art/results/341-red/`.
Masaüstü kopyası: yerel.

## Araç ve ayarlar
`create_map_object`, stil eşleme, referanssız kılavuz + maske (`footprint_guide.py`), `high top-down`, `low detail`, `basic shading`, `single color outline`, 301 ortak eki. 3 çağrı x 1 gen = 3 generation (tek deneme, `max_attempts: 1`). Buzluk kılavuzu ilk gönderimde istemci tarafında kesildi (base64 "incomplete", üretim başlamadı, harcama yok) ve aynı girdiyle yeniden gönderildi. `get_balance` iş sonu: 4995 (paralel şeritler de harcıyor; bu işin payı 3).
Kılavuzlar: ambar 176x144 `--n 3 --m 2 --height 80` (pivot 88, 48); buzluk 80x64 `--n 1 --height 24` (pivot 40, 24); buzhane 144x120 `--n 2 --m 2 --height 56` (pivot 72, 40).

## Sonuç özeti
| # | Eşya | Durum | Not |
|---|---|---|---|
| 1 | ambar | teslim (UYARI: ayak izi) | taş ayaklar üstünde yükseltilmiş ahşap kulübe, saman çatı, merdiven; tanınıyor. Gövde x42-138 (96 px), ayak izi 160 px: tabanın ~%60'ı, plan kutusu 3x2 değil ~2x2 okunuyor |
| 2 | buzluk | teslim (UYARI: palet) | demir şeritli ahşap sandık, kapak aralık, içinde buz blokları. Kapak kırmızımsı kahve (ton kaymış), palete uzak |
| 3 | buzhane | TESLİM EDİLMEDİ | tanınıyor (konik saman çatılı taş/kulübe) ama çıktıya çimen zemin ve toprak yol pişirilmiş ("no ground" çiğnendi), palet dışı renkler, KALDI. `style.py fit` şekil/zemin sorununu çözmez |

## Dosyalar (Assets/Art/Iso/_aday/)
| Yol | Piksel | Pivot x | Pivot y (alttan) |
|---|---|---|---|
| iso_item_granary_3x2_se.png | 176x144 | 88 | 48 |
| iso_item_ice_box_se.png | 80x64 | 40 | 24 |

## Doğrulama (validate_iso, `--max-colors 48`)
| Dosya | Opak sınır kutusu | Renk | Yarı saydam | Alta değme | Sonuç |
|---|---|---|---|---|---|
| ambar | x42-138, y9-125 | 23 | 0 | yok (18 satır boşluk) | geçti |
| buzluk | x10-67, y6-56 | 40 | 0 | yok | geçti |
| buzhane (red) | x9-134, y9-112 | 25 | 0 | yok | geçti (ama zemin pişmiş) |

## Stil denetimi (`style.py check --family esya`)
| Dosya | Palete yakın | Ort. uzaklık | Doygunluk | Sonuç |
|---|---|---|---|---|
| ambar | %100 | 5.8 | x0.91 | GEÇTİ |
| buzluk | %84 | 11.2 | x1.17 | UYARI (palete uzak) |
| buzhane (red) | %71 | 16.4 | x0.87 | KALDI (palet dışı) |

## Sorunlar
- Buzhane teslim edilmedi; yeniden üretim için ayrı iş gerek (ör. açıklamaya "no grass, no dirt path, no ground tile" eklemek, daha dar maske ya da referanssız kılavuz yerine ayak izi elmasını saydam bırakmak). Kullanıcı kararı.
- Ambar 3x2 ayak izini doldurmuyor; gerekirse kabul sırasında küçük ayak izi (2x2) kabul edilebilir ya da yeniden üretilir.
- Işık, ayna ve döşeme ölçümleri yapılmadı (zemin değil; tek deneme).
- Bulut: `seed` yok.
