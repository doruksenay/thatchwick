unity-import: pending

# 727 Ön yüzlü eşyalar: yönü belli görseller, bölüm 2 (rapor)

Yan yana sayfası: [727-yanyana.png](727-yanyana.png) (yalnızca teslim edilen 6). Teslim edilmeyenler: `art/results/727-red/` (2 dosya). Masaüstü kopyası: yerel.

## Araç ve ayarlar
`create_map_object`, stil eşleme, `footprint_guide.py` kılavuzu + maske (2x1: 112x112 `--n 2 --axis x --height 48`, pivot 56,32; 1x1: 80x96 `--height 48`, pivot 40,24; 2x2: 160x160 `--n 2 --m 2 --axis x --height 64`, pivot 72,40). `--axis x` uzun ekseni SE-NW (sağ-aşağıdan sol-yukarıya) yapar, şartla aynı. `high top-down`, `low detail`, `basic shading`, `single color outline`. İstem: tarif + ortak ek + `no face, no creature, no person, no text, no ground shadow`.
8 başarılı çağrı = 8 generation (cost_max 42 içinde). 3 çağrı, kılavuz base64'ünün aktarımda bozulması nedeniyle araç tarafından reddedildi (ücret yok) ve aynı tarifle yeniden gönderildi; hız sınırı hatası yok. `get_balance` iş sonu: 873,6 (paralel şeritler de harcıyor).
Not: kılavuz/maske base64'ü araç argümanı olarak elle aktarıldığı için kopyalama sırasında bozulma riski var; sonuçlar yine de geçerli görünüyor ama kılavuzun tam sadık işlendiği garanti değil. Öneri: kılavuzu yükleme URL'siyle vermek.

## Sonuç özeti (6 teslim, 2 teslim edilmedi)
| Eşya | Durum | Not |
|---|---|---|
| fireplace_large_2x1 | teslim (UYARI) | ailesinden soluk (x0.45) |
| kneading_trough_2x1 | teslim | GEÇTİ |
| cutting_table_2x1 | teslim | GEÇTİ |
| sewing_table | teslim | GEÇTİ; görsel olarak sivri direkli, oyuncak gibi (kullanıcı bakmalı) |
| alembic_2x1 | teslim (UYARI) | palete uzak (%84) |
| glass_bench_2x1 | teslim (UYARI) | ailesinden soluk (x0.59) |
| cloth_bolt_shelf_2x1 | TESLİM EDİLMEDİ | validate_iso BAŞARISIZ: 59 renk > 48 (stil GEÇTİ). `fit`/renk indirme bu iş için denenmedi |
| glass_furnace_2x2 | TESLİM EDİLMEDİ | stil GEÇTİ, validate geçti ama en alt opak satır y=129 (alt boşluk 30 satır; beklenen ~8): taban ayak izine oturmadı, eşya havada |

## Dosyalar (Assets/Art/Iso/_aday/)
| Yol | Piksel | Pivot x | Pivot y (alttan) |
|---|---|---|---|
| iso_item_fireplace_large_2x1_sw.png | 112x112 | 56 | 32 |
| iso_item_kneading_trough_2x1_sw.png | 112x112 | 56 | 32 |
| iso_item_cutting_table_2x1_sw.png | 112x112 | 56 | 32 |
| iso_item_alembic_2x1_sw.png | 112x112 | 56 | 32 |
| iso_item_glass_bench_2x1_sw.png | 112x112 | 56 | 32 |
| iso_item_sewing_table_sw.png | 80x96 | 40 | 24 |

## Doğrulama (validate_iso --kind object --max-colors 48)
| Dosya | Opak sınır kutusu | Renk | En alt opak satır | Sonuç |
|---|---|---|---|---|
| alembic | x8-103, y13-104 | 43 | 104 | geçti |
| cutting_table | x8-103, y9-103 | 25 | 103 | geçti |
| fireplace_large | x12-101, y7-104 | 42 | 104 | geçti |
| glass_bench | x10-102, y7-104 | 35 | 104 | geçti |
| kneading_trough | x9-102, y7-104 | 35 | 104 | geçti |
| sewing_table | x8-71, y12-83 | 47 | 83 | geçti |
| cloth_bolt_shelf (red) | x9-103, y7-104 | 59 | 104 | BAŞARISIZ |
| glass_furnace (red) | x33-110, y32-129 | 17 | 129 | geçti, ama alt boşluk 30 |

## Stil denetimi (`style.py check --family esya`)
| Dosya | Palete yakın | Ort. uzaklık | Doygunluk | Sonuç |
|---|---|---|---|---|
| alembic | %84 | 11.4 | x1.16 | UYARI |
| cloth_bolt_shelf (red) | %92 | 9.1 | x1.18 | GEÇTİ |
| cutting_table | %100 | 5.1 | x0.84 | GEÇTİ |
| fireplace_large | %98 | 6.3 | x0.45 | UYARI |
| glass_bench | %97 | 5.4 | x0.59 | UYARI |
| glass_furnace (red) | %99 | 8.4 | x0.64 | GEÇTİ |
| kneading_trough | %100 | 6.8 | x1.11 | GEÇTİ |
| sewing_table | %95 | 5.3 | x1.18 | GEÇTİ |

## Yön notu
Yan yana sayfasına bakışla (ölçülmedi, kullanıcı doğrulamalı): ön yüz sol-aşağıda, ışık sol-üstten, simetrik. Uzun kenar için kılavuz `--axis x` (SE-NW) kullanıldı; piksel düzeyinde bir ölçüm yapılmadı.
- fireplace_large: yön sw, uzun kenar: y (duvar tipi, ön yüz sol-aşağı)
- kneading_trough: yön sw, uzun kenar: y
- cutting_table: yön sw, uzun kenar: y
- alembic: yön sw, uzun kenar: y (ocak ve kova yan yana; sağ-sol simetri tam değil: kova sağ-önde)
- glass_bench: yön sw, uzun kenar: y (tek yana sarkan boru var, ayna simetrisi tam değil)
- sewing_table: yön sw, 1x1

## Sorunlar
- cloth_bolt_shelf ve glass_furnace teslim edilmedi (yukarıda); tek deneme kuralı gereği yeniden denenmedi. Yeniden iş açılabilir: kumaş rafı için `fit`+renk indirme, ocak için daha sıkı taban kılavuzu.
- Zemin parçası olmadığı için döşeme testi yok.
