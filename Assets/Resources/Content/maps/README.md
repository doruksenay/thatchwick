# Harita taslakları (belge 64, 64a)

Her bölgenin iskeleti bir klasör: `maps/<bölge>/terrain.png` ve `maps/<bölge>/markers.json`. Bölge kimlikleri
`Regions.All` ile aynı (`road`, `town`, `forest`, `lake`, `marsh`, `hill`, `slopes`, `coast`, `farm`). Klasörü olmayan
bölge eski kod çizimiyle kurulur.

## terrain.png
- 1 piksel = 1 karo. Sol üst piksel bölgenin yerel (0,0) karosu; sağa +x, aşağı +y. Boyut bölgenin boyutuyla aynı olmalı.
- Yalnızca aşağıdaki renkler (tam değer; kenar yumuşatma, saydamlık yok). Tablo `palette.json` ve `Core/World/MapFiles.cs`
  içinde aynı (test denetler).

| Renk | Arazi | Oyunda |
|---|---|---|
| `#608C48` | grass | çim |
| `#8B6E50` | dirt | toprak |
| `#DEC896` | sand | kum (göl, deniz kıyısı) |
| `#82828A` | stone | taş döşeme |
| `#4078B0` | water | su |
| `#244884` | deep_water | derin su (şimdilik su ile aynı) |
| `#BA8C5A` | path | toprak yol |
| `#966038` | bridge | ahşap köprü |
| `#184022` | band | kenar bandı: sık ağaç öbekleri (bölge kenarı düz kesilmez) |
| `#5C5854` | rock | kaya sırtı: iri kaya ve kütük (yükseklik hissi) |

## Nasıl boyarım
1. Dosyayı bir piksel düzenleyicide aç (Aseprite, Paint.NET, Photoshop: kenar yumuşatma kapalı, kalem aracı).
2. Yukarıdaki renklerle boya. Yolu çapraz değil, dik komşulukla bağla (oyunda yürüme 4 yönlü).
3. Geçit karolarını (Regions.cs `PortalPairs`) yol bırak.
4. Kaydet, `tools/map-preview/run.sh <bölge>` ile önizlemeye bak. Testler (`HaritaTaslakTests`) bilinmeyen rengi ve kopuk yolu yakalar.

`tools/maps/draw.py` ilk taslağı sabit tohumla çizer; yeniden çalıştırırsan elle yaptığın değişikliğin üstüne yazar.

## markers.json
- `ponds`: balık noktası (`x`, `y` su karosu; `fish`: `id`, `n`).
- `items`: sabit eşyalar ve simge yapılar (`id`, `x`, `y`, `rot`, `grown`). `landmark` etiketli eşya görseli gelmeden konmaz.
- `keep`: ağaç ve bitki serpilmeyecek dikdörtgenler (`x0`, `y0`, `x1`, `y1`, yerel, kapsayıcı).
