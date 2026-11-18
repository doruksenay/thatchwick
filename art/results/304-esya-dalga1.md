unity-import: pending

# 304 Dalga 1: hijyen, bahçe, çocuk ve yüzey dekoru (rapor)

Yan yana sayfası: [304-yanyana.png](304-yanyana.png). Teslim edilmeyen denemeler ve elenen ilk denemeler: `art/results/304-red/`.
Masaüstü kopyası: yerel (bulutta yapılamaz).

## Araç ve ayarlar
`create_map_object`, stil eşleme, referanssız kılavuz + maske (`tools/art/footprint_guide.py`; yüzey dekoru için 32x32 tuvalde 12x6 elmas, alttan 4 px yukarıda, 301 bardak kılavuzu), `high top-down`, `low detail`, `basic shading`, `single color outline`, 301 ortak eki + takım eki (Köylü: `rough light pine wood, rope lashing`, Zanaatkâr: `dark oak wood with iron fittings, neat craftsmanship`). Araçta `seed` yok; ikinci denemeler aynı kılavuz ve maskeyle.

Çağrılar (her biri 1 gen): 14 ilk deneme + 9 ikinci deneme (ekmek, dama, topaç: tabana değmedi; leğen, kova, korkuluk, bank, at, çocuk sandalyesi: ayak izi %80 altı) = 23 generation (cost_max 25 içinde). Kuyu, korkuluk, dama ve bazı maskelerin ilk gönderimleri base64 aktarım hatasıyla üretime girmeden reddedildi (ücret yok), yeniden kodlanıp gönderildi. `get_balance` iş sonu: 995,70 (paralel şeritler de harcıyor; bu işin payı 23).

## Sonuç özeti
| # | Eşya | Durum | Not |
|---|---|---|---|
| 1 | leğen sehpası | teslim (2. deneme) | 25 renk; ayak izi %66/%59 (⚠) |
| 2 | su kovası | teslim (2. deneme, UYARI) | 47 renk; ayak izi %75/%74 (⚠) |
| 3 | fıçı küvet | teslim (UYARI) | 55 renk > 48 (⚠, 301 kuralı: yalnızca raporla) |
| 4 | hela | teslim | 53 renk > 48 (⚠) |
| 5 | kuyu | teslim (UYARI soluk) | |
| 6 | korkuluk | teslim (2. deneme, UYARI) | 59 renk > 48 (⚠); ayak izi %17/%13 (direk; dar) |
| 7 | bahçe bankı (2x1) | teslim | ayak izi %37/%72 (⚠) |
| 8 | oyuncak at | TESLİM EDİLMEDİ | iki deneme de stil denetimi KALDI (doygunluk x1.8 / x1.5, palet dışı) |
| 9 | çocuk sandalyesi | teslim (2. deneme) | koyu kahve (tarifteki açık çam değil, ⚠); 49 renk |
| 10 | oyuncak sandık | teslim (UYARI) | 49 renk (⚠); kapak açık, top ve bebek görünüyor |
| 11 | ekmek kasesi | TESLİM EDİLMEDİ | iki denemede de tabana değmedi (en alt satır y=22 / 21; gerekli ≥25) ve doygunluk KALDI |
| 12 | testi | teslim | en alt satır y=25 (tabana değiyor) |
| 13 | dama tahtası | TESLİM EDİLMEDİ | iki denemede de tabana değmedi (y=17 / 23) |
| 14 | topaç | TESLİM EDİLMEDİ | iki denemede de tabana değmedi (y=23 / 21) |

## Dosyalar (Assets/Art/Iso/_aday/)
| Yol | Piksel | Pivot x | Pivot y (alttan) |
|---|---|---|---|
| iso_item_basin_ewer_se.png | 80x72 | 40 | 24 |
| iso_item_water_bucket_se.png | 80x56 | 40 | 24 |
| iso_item_barrel_tub_se.png | 80x72 | 40 | 24 |
| iso_item_outhouse_se.png | 80x120 | 40 | 24 |
| iso_item_well_se.png | 80x112 | 40 | 24 |
| iso_item_scarecrow_se.png | 80x112 | 40 | 24 |
| iso_item_garden_bench_2x1_se_nw.png | 112x88 | 56 | 32 |
| iso_item_child_chair_se.png | 80x64 | 40 | 24 |
| iso_item_toy_chest_se.png | 80x72 | 40 | 24 |
| iso_item_jug_se.png | 32x32 | 16 | 7 (küçük taban elmasının merkezi) |

## Doğrulama (validate_iso, `--max-colors 48`)
| Dosya | Opak sınır kutusu | Renk | Yarı saydam | Alta değme | Sonuç |
|---|---|---|---|---|---|
| leğen | x16-64, y3-64 | 25 | 0 | yok | geçti |
| kova | x18-61, y4-46 | 47 | 0 | yok | geçti |
| küvet | x12-67, y7-61 | 55 | 0 | yok | renk ⚠ |
| hela | x9-71, y13-112 | 53 | 0 | yok | renk ⚠ |
| kuyu | x10-69, y3-101 | 35 | 0 | yok | geçti |
| korkuluk | x20-58, y6-103 | 59 | 0 | yok | renk ⚠ |
| bank | x17-97, y8-80 | 26 | 0 | yok | geçti |
| çocuk sandalyesi | x18-67, y6-54 | 49 | 0 | yok | renk ⚠ |
| oyuncak sandık | x12-65, y5-64 | 49 | 0 | yok | renk ⚠ |
| testi | x9-22, y7-25 | 15 | 0 | taban elmasının alt yarısında | geçti |

Maske dışı opak piksel ve kılavuz rengi kalıntısı otomatik ölçülmedi (kılavuz rengi #786046 `validate_iso` renk dökümünde yok sayıldı; görselde kalıntı görülmedi). Ayak izi kaplaması (elmas içi opak oranı sol/sağ) betikle ölçüldü, tablo "Sonuç özeti"nde ⚠ ile işaretli. Tam 2:1 olmayan çıktı yok; kırpma yapılmadı. Döşeme testi eşya için gerekmez.

## Stil denetimi (`style.py check --family esya`)
| Dosya | Palete yakın | Ort. uzaklık | Doygunluk | Sonuç |
|---|---|---|---|---|
| küvet | %82 | 14.4 | x1.04 | UYARI |
| leğen | %100 | 6.4 | x1.16 | GEÇTİ |
| çocuk sandalyesi | %100 | 5.6 | x0.99 | GEÇTİ |
| bank | %100 | 4.1 | x0.65 | GEÇTİ |
| testi | %100 | 9.1 | x1.10 | GEÇTİ |
| hela | %99 | 8.9 | x0.82 | GEÇTİ |
| korkuluk | %82 | 11.2 | x1.46 | UYARI |
| oyuncak sandık | %97 | 11.2 | x1.31 | UYARI |
| kova | %84 | 13.9 | x1.04 | UYARI |
| kuyu | %100 | 6.8 | x0.50 | UYARI (soluk) |
| oyuncak at (teslim edilmedi) | %71 | 15.0 | x1.51 | KALDI |

## Işık ve ayna (ilk denemelerden ölçüm, ortalama parlaklık sol/sağ, şekil simetrisi)
Leğen 60/74 (sağ daha açık ⚠, ilk deneme), kova 119/121 %100, küvet 100/80 %99, hela 47/83 %93 (sağ yüz açık, ışık sol-üstten değil ⚠), kuyu 61/66 %94, korkuluk 90/85 %96, bank 58/58 %60, çocuk sandalyesi ~170/168 %70, sandık 42/66 %77, testi 90/61 %90. Simetrik olanlar (kova, küvet, kuyu, korkuluk) tek sprite ile her yöne gider. Ayna dosyası Assets'e konmadı; aynada ışık sağ-üstten gelir. Yön ölçümü (1x1 asimetrik): sandalye ve sandık SE görünümlü.

## Sorunlar
- Birleşik sınıf: 5 teslim edilen dosyada renk 48 üstü (301 kuralı, `reduce_colors` yapılmadı).
- Aynı takımın renk uyumu (ton/doygunluk tablosu) ölçülmedi; çocuk sandalyesi koyu kahve çıktı, Köylü açık çam ailesinden ayrışıyor (kullanıcı yan yana sayfasına bakmalı).
- Yüzey dekoru: 3/4 eşya tabana değmedi (stil eşlemeli 32x32 tuvalde taban elmasına oturma tutmuyor); yeni kılavuz yöntemi ya da ikinci araç gerekebilir.
- Oyuncak at: iki denemede de stil denetimi KALDI.
- Yüzey eşyalarının yüzey y aralığı ölçümü yapılmadı (teslim edilen yüzey eşyası yok).
- Araçlarda `seed` yok; "seed değişir" kuralı uygulanamadı.
