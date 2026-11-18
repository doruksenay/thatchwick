unity-import: pending

# 352 Su yolu borular: 2/9 teslim, 7 teslim edilmedi

Yan yana sayfası: [352-yanyana.png](352-yanyana.png). Ham sayfa: [art/352-red/352-ham-sayfa.png](../352-red/352-ham-sayfa.png).

## Araç ve ayarlar
- `create_tiles_pro` stil modu (351 ile aynı), `style_images` = `iso_ground_grass_a.png` (64x32), renk paleti kapalı, outline/detail/shading açık. 1 çağrı, 16 karo döndü; **10 gen** (cost_est 10, cost_max 20). Hız sınırı hatası yok.
- `get_balance` çağrı öncesi 4313,9; çağrı sonrası okunmadı (paralel şeritler hesabı paylaşıyor).

## Teslim edilenler (Assets/Art/Iso/_aday/)
| Yol | Boyut | Pivot x | Pivot y (alttan) | validate | Stil denetimi | Doygunluk | Dikiş oranı |
|---|---|---|---|---|---|---|---|
| iso_conduit_pipe_clay_center.png | 64x32 | 32 | 16 | geçti | GEÇTİ | x1.13 | 1.10 |
| iso_conduit_pipe_lead_center.png | 64x32 | 32 | 16 | geçti | GEÇTİ | x0.95 | 1.06 |

Kurşun center: üst kata çıkan dikine kolon ucu var (tarif). Aile kilidi kurulu değil, palet uyumu uyarıdır. Döşeme testi 8x8 (32,16) adımı: dikişler tutuyor (oran yukarıda).

## Teslim EDİLMEYENLER (tek deneme)
- **Tüm kollar (arm_ne / arm_se, 3 boru)**: model yarım kol yerine karoyu baştan başa kateden tam çizgi (KD-GB ya da KB-GD) ve T kavşak çizdi; boru ayrıca toprağa gömülü değil, üstte duran 3B silindir gibi. Ayak izi geçiyor (validate geçti) ama tarifteki "ortadan kenar ortasına yarım kol" değil; maskeyle yarıya kesmek içeriği değiştirmek olurdu. Teslim edilmedi.
- **Kütük boru center**: model ağaç kütüğü (kök ayaklı) çizdi, boru ucu değil; teslim edilmedi.
- Ham 16 karo `art/352-red/` altında.

## Öneri
Kollar için yeni iş: teslim edilen center parçalarını referansa ekleyip tarifi "tek yarım boru, kenar ortasında biter, toprakla yarı örtülü" diye netleştir; ya da `pixellab: false` ile ham tam çizgilerden yarım kol türetme işi. Masaüstü kopyası: yerel.
