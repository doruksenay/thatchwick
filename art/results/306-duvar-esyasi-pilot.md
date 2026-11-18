unity-import: pending

# 306 Duvar eşyası pilotu (rapor): TESLİM YOK, blocked

Yan yana sayfası: [306-yanyana.png](306-yanyana.png). Reddedilen denemeler: `art/results/306-red/`. Masaüstü kopyası: yerel.

## Araç ve ayarlar
`create_map_object` stil eşleme, `side` görünüm (duvar yüzü için; `high top-down` yerine), `low detail`, `basic shading`, `single color outline`, tarifteki açıklama + duvar eki + Köylü eki. Kılavuz ve maske: yeni `tools/art/wall_guide.py` (48x64, 32 px genişlikte "/" eğimli paralelkenar, maske 40 px yüksek). 2 çağrı x 1 gen = 2 generation. get_balance iş başı 983,70.

## Sonuç
| Deneme | Durum |
|---|---|
| 1 | meşale olarak tanınmıyor (sap yok, bordo ahşap, dev alev); ikinci deneme gerekçesi |
| 2 | tanınır meşale; validate_iso geçti (48x64, 40 renk, yarı saydam 0, maske dışı 0/316); stil denetimi KALDI |

## Stil denetimi (`style.py check --family esya`)
| Dosya | Palete yakın | Ort. uzaklık | Doygunluk | Sonuç |
|---|---|---|---|---|
| deneme 2 | %79 | 12.1 | x1.17 | KALDI (palet dışı renkler) |

Ölçümler: opak kutu x12-32, y13-44 (alttan 19 satır boş), ışık sol ort. 60 / sağ 96 (sağ açık, beklenenin tersi). Döşeme testi geçerli değil (duvar eşyası).

## Dosyalar
Teslim edilen yok (`Assets/Art/Iso` değişmedi).

## Sorunlar
Pilot tutmadı, diğer dört eşya denenmedi; `## Sorular` iş dosyasında.
