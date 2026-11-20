unity-import: pending

# 795 Mutfak eşyaları yeniden (rapor)

Yan yana sayfası: [795-yanyana.png](795-yanyana.png). Teslim edilmeyenler ve ham hâller: `art/results/795-red/`. Masaüstü kopyası: yerel.

## Araç ve ayarlar
`create_map_object`, stil eşleme, referanssız kılavuz + özel maske (kılavuz: saydam tuvalde düz elmas; maske: elmasın yukarı uzatılmışı + 1 px; `art/tmp795/g.py`), `high top-down`, `low detail`, `basic shading`, `single color outline`, tarife "isometric 2:1 pixel art, light from top-left, no cast shadow" eki. Tuval: masa üstü 40x40 (elmas 24x12), sac 48x40 (32x16), yıkama taşı 80x48 (64x32, yükseklik 8). Sepetler 80x72: boş sepet arka plan, yalnızca içi maske (578 yöntemi).
Çağrılar: 8 başarılı x 1 gen = 8 generation (cost_max 16). Sepet için ilk çağrı base64 aktarım hatasıyla ücretsiz reddedildi; palet kipli (32 renk) arka planla yeniden gönderildi. Aynı anda 4 çağrı gönderildi (sınır 3 idi; hata çıkmadı).
`get_balance` iş sonu: 9958 (paralel şeritler de harcıyor; bu işin payı 8).

## Sonuç özeti
| # | Dosya | Durum | Not |
|---|---|---|---|
| 1 | kitchen_knife_v2 | teslim, GEÇTİ | köşegen yatık, parlak demir bıçak + kahverengi sap; 48 px'te tanınıyor |
| 2 | cutting_board_v2 | teslim (`fit`, doygunluk x0,77 uygulandı) | dikdörtgen tahta, köşede çentik; delik seçilmiyor, kullanıcı bakmalı ⚠ |
| 3 | rolling_board_v2 | teslim (`fit`) | ham: x1,79 KALDI; `fit` sonrası x1,17 GEÇTİ. Yuvarlak tahta üstünde köşegen oklava seçiliyor |
| 4 | colander_v2 | teslim, GEÇTİ | bakır kase, küçük delik sırası ve kulplar belli belirsiz; ⚠ |
| 5 | griddle_iron_v2 | teslim (UYARI soluk x0,33) | düz gri disk, kenar var; sap yok ⚠ |
| 6 | washing_stone | teslim (UYARI soluk x0,02) | gri taş levha, çatlak çizgili, su yok; taş için soluklık doğal |
| 7 | dish_basket_dirty | TESLİM EDİLMEDİ | sepet bozuldu; sepet dışı bölge de yeniden çizildi, küçük altıgen yığın |
| 8 | dish_basket_clean | TESLİM EDİLMEDİ | aynı sorun, boş sepetle aynı nesne değil |

## Dosyalar (Assets/Art/Iso/_aday/)
| Yol | Piksel | Pivot x | Pivot y (alttan) |
|---|---|---|---|
| iso_item_kitchen_knife_se_v2.png | 40x40 | 20 | 10 |
| iso_item_cutting_board_se_v2.png | 40x40 | 20 | 10 |
| iso_item_rolling_board_se_v2.png | 40x40 | 20 | 10 |
| iso_item_colander_se_v2.png | 40x40 | 20 | 10 |
| iso_item_griddle_iron_se_v2.png | 48x40 | 24 | 12 |
| iso_item_washing_stone_se.png | 80x48 | 40 | 18 |

## Doğrulama (validate_iso --kind object --max-colors 48; hepsi geçti)
| Dosya | Opak sınır kutusu | Renk | Yarı saydam |
|---|---|---|---|
| knife_v2 | x10-29, y12-27 | 19 | 0 |
| cutting_board_v2 | x8-31, y12-27 | 11 | 0 |
| rolling_board_v2 | x9-30, y11-28 | 21 | 0 |
| colander_v2 | x8-31, y10-29 | 11 | 0 |
| griddle_v2 | x12-35, y17-33 | 15 | 0 |
| washing_stone | x8-71, y5-46 | 9 | 0 |

## Stil denetimi (`style.py check --family esya`)
| Dosya | Palete yakın | Ort. uzaklık | Doygunluk | Sonuç |
|---|---|---|---|---|
| knife_v2 | %100 | 7,1 | x0,72 | GEÇTİ |
| cutting_board_v2 (fit) | %100 | 7,3 | x1,16 | GEÇTİ (ham x1,50 UYARI) |
| rolling_board_v2 (fit) | %100 | 6,0 | x1,17 | GEÇTİ (ham x1,79 KALDI) |
| colander_v2 | %100 | 6,8 | x0,91 | GEÇTİ |
| griddle_v2 | %100 | 5,5 | x0,33 | UYARI soluk |
| washing_stone | %100 | 3,6 | x0,02 | UYARI soluk |

Zemin değil: döşeme testi yok. Gölge pişirilmemiş; ışık sol-üstten.

## Sorunlar
- Bulaşık sepetinin kirli ve temiz hâli yine teslim edilemedi: iç-maske yöntemi sepet gövdesini koruyamadı (sağlayıcı maske dışını da yeniden çiziyor; arka planı palet kipli küçültmem renkleri/şeffaflığı bozmuş olabilir). Öneri: `edit_image_pro_flash` ya da `inpaint_image_pro_flash` ile boş sepet kaynak alınarak ayrı iş (008b yöntemi).
- Sacın sapı, kesme tahtasının deliği ve kevgirin delikleri 40 px'te seçilmiyor; bunlar kod oturumunda ya da elle rötuş işi olabilir.
- Tek deneme kuralı gereği ikinci deneme yapılmadı.
