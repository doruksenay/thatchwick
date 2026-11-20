---
id: "739"
title: "Yol geçiş seti c çeşidi: çim tonunu ana çime eşitle (renk düzeltme, yeniden çizim yok)"
status: pending
approved_by: user        # kullanıcı onayı 2026-11-20 (sohbet: "kıyı karosu ve çim tonu için PixelLab işi hazırla"; belge 65b)
priority: 10
family: zemin
style_gate: false
cost_est: 0
cost_max: 0
depends_on: []
max_attempts: 1
pixellab: false
outputs:
  - Assets/Art/Iso/iso_path_dirt_wang_<NN>_c_v2.png   (NN 00-14, 15 karo)
  - art/results/739-yanyana.png
created: 2026-11-20
---
# 739 Yol geçiş seti c çeşidi: çim tonu düzeltmesi

## Neden
Oyunda (belge 65b, MCP turu) yolun iki yanında düz, cetvelle çizilmiş gibi koyu bir çim şeridi görünüyor. Kaynağı 004e'nin
c çeşidi: c karolarındaki çim pikselleri ana çimden belirgin koyu. Ölçüm (yalnızca çim pikselleri, ortalama):

| Karo | Ortalama |
|---|---|
| `iso_ground_grass_a/b/c` (ana çim) | (115-117, 174-178, 94-99) |
| `wang_06_a`, `wang_09_a` | (109-110, 171-172, 91) |
| `wang_06_b`, `wang_09_b` | (109-113, 166-170, 93-96) |
| **`wang_06_c`, `wang_09_c`** | **(102-105, 159-163, 86-89)** |
| `wang_00_c` (saf çim köşesi) | (101, 158, 86) |

Yol kenarındaki karoların ~%40'ı c çeşidi (sayım: a 13, b 20, c 22). a ve b kabul edilebilir aralıkta.

## Tarif
- PixelLab çağrılmaz. Yeniden boyama yok; yalnızca renk eşleme.
- c çeşidinin 15 karosunda (`wang_00_c` … `wang_14_c`) yalnızca **çim piksellerinin** (yeşil kanal kırmızı ve maviden 8+ büyük)
  tonunu `iso_ground_grass_a` ortalamasına kaydır: kanal başına fark kadar ekle, kontrast ve desen aynı kalsın. Toprak
  pikselleri ve saydamlık değişmez.
- Araç: `tools/art/style.py fit` (referans `iso_ground_grass_a`, yalnızca çim maskesi). 
- Dosyalar `_c_v2` adıyla yazılır (bulut işçisi var olanı değiştiremez).

## Doğrulama
- Her çıktıda çim pikselleri ortalaması `iso_ground_grass_a` ile kanal başına ±5.
- `validate_iso.py --kind ground` (maske değişmemeli: opak 1024, elmas dışı 0).
- Yan yana sayfası: önce/sonra, 12 karoluk yol önizlemesinde koyu şerit görünmemeli.

## Notlar
- Kabulden sonra kod işi: `IsoArt.PathWang` c çeşidinde `_c_v2`'yi tercih etsin (yoksa eskisine düşsün).
- Geçici çözüm (iş gelene kadar): kodda c çeşidini atlayıp yalnızca a/b kullanmak da şeridi kaldırır.

<!-- İşçi bundan sonrasına dokunmaz; kullanıcı kararı:
status: review → accepted ya da rejected yap, rejected ise nedenini buraya yaz. -->
