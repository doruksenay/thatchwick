unity-import: pending

# 351 Su yolu karoları: 5/18 teslim (kısmi), 13 teslim edilmedi

Yan yana sayfası: [351-yanyana.png](351-yanyana.png). Ham sayfalar: [kuru](351-ham-kuru.png), [sulu](351-ham-sulu.png).

## Araç ve ayarlar
- `create_tiles_pro`, stil modu (340 ile aynı), `style_images` = `iso_ground_grass_a.png` (64x32, satır içi base64), stil seçenekleri: renk paleti kapalı, outline/detail/shading açık. Yalnızca çimen referansı verildi.
- 2 paralel çağrı (kuru 9 parça, sulu 9 parça), her biri 16 karo döndü. Maliyet: çağrı başına **10 gen**, toplam **20** (cost_est 20, cost_max 40). Hız sınırı hatası yok.
- `get_balance` sonrası: 4968 (paralel şeritler aynı hesabı kullandığından yalnızca bu işi yansıtmaz).

## Teslim edilenler (Assets/Art/Iso/_aday/)
| Yol | Boyut | Pivot x | Pivot y (alttan) | Not |
|---|---|---|---|---|
| iso_conduit_ditch_dry_center.png | 64x32 | 32 | 16 | doygunluk x0.75 (fit) |
| iso_conduit_ditch_dry_arm_ne.png | 64x32 | 32 | 16 | doygunluk x0.75 (fit) |
| iso_conduit_ditch_dry_arm_se.png | 64x32 | 32 | 16 | |
| iso_conduit_stone_dry_center.png | 64x32 | 32 | 16 | UYARI |
| iso_conduit_flume_dry_center.png | 64x32 | 32 | 16 | UYARI |

Maske dışı opak pikseller yalnızca maskeyle kırpıldı (içerik boyanmadı).

## Doğrulama
| Dosya | validate_iso | Stil denetimi | Doygunluk | Dikiş oranı |
|---|---|---|---|---|
| ditch_dry_center | geçti | GEÇTİ (fit sonrası; ham KALDI x1.54) | x1.16 | 1.13 |
| ditch_dry_arm_ne | geçti | GEÇTİ (fit sonrası; ham KALDI x1.54) | x1.16 | 1.05 |
| ditch_dry_arm_se | geçti | GEÇTİ | x1.06 | 1.09 |
| stone_dry_center | geçti | UYARI (palet, doygun) | x1.38 | 1.07 |
| flume_dry_center | geçti | UYARI (doygun) | x1.33 | 1.11 |

Dikiş oranı = kenar bandı / iç parlaklık (fit öncesi renkler). Aile kilidi kurulu değil, palet uyumu uyarıdır.

## Teslim EDİLMEYENLER ve nedeni (tek deneme, boşluk doldurulmadı)
- **Tüm 9 sulu parça**: maske içinde 62-63 boş piksel (validate BAŞARISIZ). Sulu sayfada karolar 1 piksel küçük çizilmiş (x 1-62, y 0-30); kuru sayfada yok. Çizimler görsel olarak iyi (mavi şerit, taş kenar, ahşap oluk), yalnızca kenar boşluğu var.
- `flume_dry_arm_ne` (1 boş piksel), `flume_dry_arm_se` (27 boş piksel): validate BAŞARISIZ.
- `stone_dry_arm_ne`: 1 boş piksel ve U biçimli, kol değil. `stone_dry_arm_se`: validate geçti ama tam çapraz çizgi, kol değil; teslim edilmedi.
- Kırpılmış hâlleri `art/351-red/` altında (ham sayfalar yukarıdaki bağlantılarda).
- Sayfadaki ek parçalar (tam hat, T kavşak) tarifte olmadığı için kullanılmadı.

## Sorunlar ve öneri
- 3x3 birleşim testi (düz hat, L, T, artı) yapılamadı: kol parçaları eksik. Döşeme testi (8x8, (32,16) adımı) kuru parçalarda yapıldı, dikiş oranları 1.05-1.13.
- Devam işi önerisi: sulu 9 parça ile eksik kuru taş/ahşap kollarını yeniden üret; stil referansı olarak teslim edilen ditch parçaları ve 340 yol karoları eklenmeli, doygunluk için `fit`.
- Masaüstü kopyası: yerel.
