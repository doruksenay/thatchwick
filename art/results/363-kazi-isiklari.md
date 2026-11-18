unity-import: pending

# 363 Kazı ışıkları (rapor)

Yan yana sayfası: [363-yanyana.png](363-yanyana.png) (üç yeni görsel: mum, meşale, kandil; kandil teslim edilmedi). Masaüstü kopyası: yerel.

## Araç ve ayarlar
`create_map_object`, stil eşleme, kılavuz + maske, `high top-down`, `low detail`, `basic shading`, `single color outline`, 301 ortak eki. 3 çağrı x 1 gen = 3 generation (cost_max 14). `get_balance` sonrası 4593,9 (paralel şeritler de harcıyor).
Not: `footprint_guide.py` 1x1 için 64 px geniş elmas çiziyor, 48 px tuvale sığmıyor. Kılavuz ve maske bu yüzden `art/tmp363/mk.py` ile 32x16 elmas (alt kenardan 4 px yukarı, ortalı) olarak üretildi; maske elmasın tarif yüksekliği (20/14/44 px) kadar yukarı uzatılmış, 1 px genişletilmiş hali. Araç değiştirilmedi; öneri: `footprint_guide.py`'ye `--diamond 32` seçeneği.
Işık halkası: betikle (`art/tmp363/ring.py`), 0 gen. 192x96 elips, açık sarı (255,236,150), alfa merkezde ~110 → kenarda 0.

## Dosyalar
| Yol | Piksel | Pivot x | Pivot y (alttan) |
|---|---|---|---|
| Assets/Art/Iso/_aday/iso_item_mine_candle_se.png | 48x56 | 24 | 8 |
| Assets/Art/Iso/_aday/iso_item_mine_torch_se.png | 48x80 | 24 | 8 |
| Assets/Art/Iso/iso_fx_light_ring.png | 192x96 | 96 | 48 |

(Pivot, kılavuz elmasının alt ucuna göre: taban merkezi, alt kenardan 4+8=12 px değil elmas merkezi 12 px; 1x1 elmasın alt köşesi 4 px. Pivot tablosu eşya ön ekinden gelir; burada elmas merkezi alttan 12 px.)

## Doğrulama
| Dosya | Sınır kutusu | Renk | Yarı saydam | Sonuç |
|---|---|---|---|---|
| mum | x9-38, y17-48 | 27 | 0 | geçti |
| meşale | x11-37, y16-71 | 43 | 0 | geçti |
| kandil (teslim edilmedi) | x14-34, y15-36 | 55 | 0 | BAŞARISIZ (renk 55 > 48) |
| ışık halkası | tüm elips | 0 opak | 12712 | validate_iso "BAŞARISIZ" (hiç opak piksel yok, yarı saydam var): bu dosya tasarım gereği yarı saydam; ayak izi/2:1 denetimi uygulanmaz. ⚠ kullanıcı kararı |

## Stil denetimi (`style.py check --family esya --light`)
| Dosya | Palete yakın | Ort. uzaklık | Doygunluk | Sonuç |
|---|---|---|---|---|
| mum | %100 | 9.8 | x0.45 | UYARI (soluk; ışık pikseli %11 denetim dışı) |
| meşale | %95 | 6.4 | x0.87 | GEÇTİ (ışık %10) |
| kandil | %92 | 11.8 | x1.46 | KALDI (ışık %41 > %35, palete uzak, doygun) |

## Teslim edilmeyen
- **Kandil:** tek deneme; 55 renk, alev/ışık payı %41 (parlak turuncu halka), palete uzak. Bu işte `reduce_colors`/`fit` denenmedi (tek deneme kuralı, renk sınırında `reduce_colors` yok). Dosya `art/results/363-red/iso_item_mine_lamp_se.png`. Yeniden iş gerekir (ör. 363b, `description`'a "no glowing halo, small flame only").

## Gözlem
Meşale: taş taban koyu mor-gri bloklu, tarifteki "taşlar arasına sıkıştırılmış" ile uyumlu; mum: düz taş üstünde mum, tanınıyor. Ayak izi doluluğu ve ışık yönü ölçülmedi (görsel küçük; kullanıcı yan yana sayfasına bakmalı). Pişmiş gölge ve zemin yok.
