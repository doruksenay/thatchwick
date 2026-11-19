unity-import: pending

# 376b Yol karoları devam (1/3 teslim)

Yan yana sayfası: [376b-yanyana.png](376b-yanyana.png) (referanslar + path_b_v2, gravel_worn, gravel_a_v2, paved_worn, ve teslim edilmeyen paved_a / paved_broken_v2). Ham 16 karo: [376b-ham-16.png](376b-ham-16.png) (satır-öncelikli; çift numara sağlam kaldırım, tek numara bozuk). Döşeme: [gravel_a_v2](376b-doseme-gravel-a-v2.png).

## Araç ve ayarlar
- **Çakıl a v2:** PixelLab yok. `376-red/tile_1.png`, maske içi 12 boş piksel en yakın opak komşu rengiyle dolduruldu (`art/tmp/repair.py`, delik onarımı; limit 15).
- **Kaldırımlar:** `create_tiles_pro`, stil modu, `outline_mode=segmentation`, `style_images` = paved_worn + path_b_v2 (64x32), `style_options` = renk paleti kapalı, outline/detail/shading açık. Açıklamalar 376 tablosu + `, light grey stone, no brown tint, no dark border`. Tek çağrı, 2 açıklama, 16 karo. Maliyet **10 generation** (cost_est 8, cost_max 14 içinde; aracın bildirdiği aralık 10-25). Hız sınırı hatası yok.
- `get_balance`: çağrıdan önce 3723,5; paralel şeritler aynı hesabı kullandığı için sonrası yalnızca bu işi yansıtmaz.

## Seçim
| Dosya | Kaynak | Durum |
|---|---|---|
| iso_road_gravel_a_v2 | 376-red/tile_1 (12 delik onarıldı) | TESLİM |
| iso_road_paved_a | tile_2 (3 delik onarıldı) | TESLİM EDİLMEDİ: dikiş oranı 0,814 (hedef 0,95-1,05). `segmentation` ve "no dark border"a rağmen karoda koyu kontur kenarı sürüyor |
| iso_road_paved_broken_v2 | tile_5 (3 delik onarıldı) | TESLİM EDİLMEDİ: dikiş oranı 0,737, koyu kenar sürüyor |

Boşluk sorunu çözüldü (16 karonun hepsinde 3-10 boş piksel), ama asıl şikâyet olan koyu kenar giderilmedi. Tek deneme kuralı gereği yeniden üretilmedi. Reddedilen karolar `art/results/376b-red/`. Öneri: kaldırım kenarını kaldırmak için kenar bandını içe doğru örnekleyen bir betik ya da elle (pixelart_workbench) düzeltme işi; kullanıcı mevcut karoları gözle kabul etmek isterse yan yana sayfasına bakabilir.

## Dosyalar
| Yol | Boyut | Pivot x | Pivot y (alttan) |
|---|---|---|---|
| Assets/Art/Iso/iso_road_gravel_a_v2.png | 64x32 | 32 | 16 |

## Doğrulama
| Dosya | validate_iso (ground) | Renk | Stil denetimi | Doygunluk / aile | Dikiş oranı |
|---|---|---|---|---|---|
| gravel_a_v2 | geçti | 23 | UYARI (kilit kurulmadı; ailesinden soluk) | x0.49 | 1,005 |
| paved_a (teslim yok) | geçti | 16 | UYARI (soluk) | x0.16 | 0,814 |
| paved_broken_v2 (teslim yok) | geçti | 24 | UYARI (soluk) | x0.22 | 0,737 |

8x8 döşeme testi yapıldı (gravel_a_v2 tekrar eden yüzey olarak okunuyor, hedef içinde). Gölge pişirilmemiş, ışık sol-üstten. Gravel "ailesinden soluk" uyarısı: çakıl gri-bej, aile çimen/toprak ağırlıklı. Masaüstü kopyası: yerel.
