# Görsel yönerge: İzometrik (2:1 dimetrik) varlıklar

Bu dosya, varlık üreten yapay zekâ (PixelLab, Claude Code oturumu) için bağlayıcı şartnamedir.
Oyun artık **izometrik**. Eski "yüksek top-down / 3/4" testleri (Assets/Art/Test) **geçersizdir**.

## Projeksiyon
- 2:1 dimetrik. Zemin karosu bir **elmas: 64 px geniş x 32 px yüksek**. Unity PPU = 32 (1 dünya birimi = 32 px).
- Eksenler: ızgara +x ekranda sağ-yukarı (Iso.GridToWorld ile aynı), +y sol-yukarı. Yukarı yön kuzey-batı/kuzey-doğu köşeleri.
- Kenar çizgileri tam 2:1 (2 piksel yatay, 1 piksel dikey) basamaklı olmalı; yumuşatma (anti-alias) yok.
- Işık sol-üstten: üst yüz en açık, sol yüz orta, sağ yüz en koyu. **Gölge sprite'a pişirilmez** (oyun ekler).
- Arka plan tamamen saydam. Düz, az tonlu pikselart; önceki test karakterleriyle aynı stil/palet ailesi.

## Teslimat (Assets/Art/Iso/ altına)
1. **Zemin seti**: çimen <-> toprak geçişli iso tileset, her karo 64x32 elmas, dikişsiz. + 2 çimen varyasyonu.
2. **Köylü**: 1 karakter, 8 yön (en az 4: SE, SW, NW, NE). Boy yaklaşık 44-52 px. Ayak altı karo merkezinde.
3. **Eşyalar**: yatak (2x1 karo), ocak taşı, tabure (1x1). Taban ayak izine uyar, yükseklik serbest.
4. **Duvar parçaları**: her iki eksen için (SE-NW ve SW-NE yönlü) düz duvar + köşe.

## Adlandırma
`iso_<tür>_<ad>_<yön>.png` (ör. `iso_char_villager_se.png`, `iso_item_bed_straw.png`, `iso_ground_grass_a.png`).
Yönler: se, sw, nw, ne. Çok karolu nesne: `_2x1` eki.

## Pivot kuralı
- Pivot her zaman **ayak izi taban elmasının merkezi** (karakterde ayakların altı, karo merkezi).
- 1x1 nesnede: yatay merkez, dikeyde elmasın orta noktası. Sprite boyutunu ve pivotu raporla (px).

## Kurallar
- Eski 3/4 testlerini `Assets/Art/Test_34/` altına taşı; silme.
- `Assets/Scripts` altına dokunma.
- İş bitince her dosyanın piksel boyutunu ve pivot (px) değerini listele.
