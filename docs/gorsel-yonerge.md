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

## Yön kuralı (yerleşim sistemi, belge 44)
Tek tanım: `Assets/Scripts/Core/Layout/Facing.cs` (testli). Izgaraya göre dört mantıksal yön:

| Yön | Izgara | Ekranda | Dosya eki | Kamera |
|---|---|---|---|---|
| N | -y | sağ-aşağı | `_se` | ön yüz görünür |
| E | +x | sağ-yukarı | `_ne` | arkası dönük |
| S | +y | sol-yukarı | `_nw` | arkası dönük |
| W | -x | sol-aşağı | `_sw` | ön yüz görünür |

- Bir yön ancak o yönün görseli (`items.json` → `art.n/e/s/w`) ya da sol/sağ simetrik eşyada (`art.mirror`) aynasının görseli
  varsa geçerlidir. Ayna yalnızca se ↔ sw (N ↔ W) ve ne ↔ nw (E ↔ S) çevirir; **arkası dönük görünüm aynayla elde edilmez**,
  ayrı görsel ister. Görseli olmayan yön seçilemez (yapı modunda Döndür yalnızca geçerli yönler arasında döner).
- Ön yüzü olan eşyada (oturma, sandık/dolap, ocak/tezgâh) dönüş 0'ın yönü: çok karoluda ayak izinden (yatay eşya N, dikey eşya W),
  tek karoda kendi görselinin yönü. Her dönüş saat yönünde bir adım (N → E → S → W).
- Ön yüzü olmayan eşya eksen görselleriyle çizilir (`art.alongX` çift dönüş, `art.alongY` tek dönüş).
- Görsel boyutu (`art.pxW/pxH`) ve pivot (`art.pivotX/pivotY`, alttan) ayak izinden (`width/height`) ayrı tutulur. Görsel adındaki ölçü
  ayak iziyle uyuşmuyorsa görsel eşlenmez (örn. 1x1 üretilmiş kurutma askısı, 2x1 tanım).
- Kameraya dönük yönler N ve W olduğundan oturma eşyası ve dolaplar odanın arka duvarlarına (doğu +x ve güney +y) sırt verir;
  ön duvarlar (kuzey ve batı) kesitte saydamlaşır.
