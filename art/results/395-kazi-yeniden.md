unity-import: pending

# 395 Kazı yeniden (rapor)

Yan yana sayfası: [395-yanyana.png](395-yanyana.png) (sağdaki 5 görsel yeni; ahşap çatkı teslim edilmedi). Teslim edilmeyen: `art/results/395-red/`. Masaüstü kopyası: yerel.

## Araç ve ayarlar
- Çatlak taş: `create_tiles_pro` (360 ayarları: isometric, 64, tile_view_angle 30, depth 0.3, flat_top 2), 10 gen. 16 varyasyon döndü, hiçbirinde turuncu/kor piksel yok (sayıldı, 0); en koyu ve çatlak/is lekeli 14. varyasyon seçildi. Kullanılmayanlar PixelLab'de: tile id `b865ca09-94f8-4918-84bc-ae832b433329`.
- Çatkı, dren, kandil, kristal: `create_map_object` stil eşleme, kılavuz + maske (çatkı 80x96/60, dren 80x48/10 `footprint_guide.py`; kandil 48x48 ve kristal 40x40 için 32x16 / 16x8 elmas, maske +20 px, `art/tmp395/mk.py`), `high top-down`, `low detail`, `basic shading`, `single color outline`, 301 ortak eki. 4 çağrı x 1 gen = 4 gen. Toplam 14 gen (cost_max 16).
- Kandil: ham çıktı doygun (x1.65, ışık %47); `style.py fit --family esya` ile doygunluk x0.63 (aile hedefi 0.467) uygulandı. Ham hâl `395-red/`de.
- `get_balance` iş sonu: 4384,9 (paralel şeritler dahil).
- Hız sınırı hatası yok. İki çağrı base64 kopya hatasıyla başlamadan reddedildi (harcama yok) ve yeniden gönderildi.

## Dosyalar
| Yol | Piksel | Pivot x | Pivot y (alttan) | Durum |
|---|---|---|---|---|
| Assets/Art/Iso/iso_rock_cracked_b.png | 64x64 | 32 | 37 | teslim |
| Assets/Art/Iso/_aday/iso_item_drain_pit_se.png | 80x48 | 40 | 24 | teslim (UYARI) |
| Assets/Art/Iso/_aday/iso_item_mine_lamp_se.png | 48x48 | 24 | 12 | teslim (UYARI, fit) |
| Assets/Art/Iso/_aday/iso_decor_relic_crystal.png | 40x40 | 20 | 8 | teslim (UYARI) |
| iso_item_mine_support_wood_b_se.png | 80x96 | - | - | TESLİM EDİLMEDİ (`395-red/`) |

## Doğrulama (`validate_iso --kind object --max-colors 48`)
| Dosya | Opak sınır | Renk | Yarı saydam | Sonuç |
|---|---|---|---|---|
| rock_cracked_b | x0-63, y11-52 | 14 | 0 | geçti |
| drain_pit | x10-69, y7-40 | 21 | 0 | geçti |
| mine_lamp (ham) | x8-39, y9-39 | 39 | 0 | geçti |
| crystal | x12-27, y7-35 | 13 | 0 | geçti |
| mine_support_wood_b | x11-70, y6-88 | 23 | 0 | geçti (şekil kabul edilmedi) |

## Stil denetimi
| Dosya | Palete yakın | Ort. uzaklık | Doygunluk | Sonuç |
|---|---|---|---|---|
| rock_cracked_b (zemin, kilit yok) | %100 | 6.1 | x0.34 | UYARI soluk |
| drain_pit | %99 | 9.0 | x0.37 | UYARI soluk |
| mine_lamp ham | %77 | 16.1 | x1.65 | KALDI (teslim edilmedi) |
| mine_lamp fit | %100 | 11.2 | x1.06 | UYARI (ışık %25, palete uzak) |
| crystal | %99 | 13.6 | x0.63 | UYARI palete uzak |
| mine_support_wood_b | %100 | 3.5 | x0.71 | GEÇTİ (şekil sorunu) |

## Sorunlar
- **Ahşap çatkı teslim edilmedi:** metin "tam iki dikme ve bir kiriş" idi; çıktı dört dikmeli, üstte kiriş çerçeveli kafes/kule gibi, çatkı okunmuyor. Tek deneme kuralı, yeniden denenmedi. Öneri: 395b, daha dar kılavuzla ya da 362'deki çürüyen çatkıyı `fit` ile yeniden renklendirmek.
- Çatlak taş: kor yok, soluk koyu gri; ışık sol-üstten, gölge pişirilmemiş. Blok olduğundan döşeme testi uygulanmadı.
- Dren çukuru: kahverengi çerçeveli düz kutu içinde gri çakıl ve ortada küçük delik; çukur olarak okunur ama çerçeve kenarı baskın.
- Kandil: kil kase, ortada küçük alev; kase kırmızımsı. `fit` sonrası hâlâ UYARI.
- Kristal: üç değil tek berrak sivri kristal (taban mor-gri), kırmızı yok; "üç küme" tarifi tutmadı ama eski kırmızı çıktıdan iyi.
