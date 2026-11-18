unity-import: pending

# 302 Dalga 1: oturma, uyku ve masa (rapor)

Yan yana sayfası: [302-yanyana.png](302-yanyana.png). Teslim edilmeyenler: `art/results/302-red/`. Masaüstü kopyası: yerel.

## Araç ve ayarlar
`create_map_object`, stil eşleme, `footprint_guide.py` kılavuzu + maske (referanssız), `high top-down`, `low detail`, `basic shading`, `single color outline`, 301'deki ortak ek + takım eki (Köylü: rough light pine wood, rope lashing; Zanaatkâr: dark oak wood with iron fittings, neat craftsmanship). Çağrı başı 1 gen.
Çağrılar: ilk 8 çağrı (ilk denemede base64 aktarımı kesildiği için 7'si üretime girmeden reddedildi, ücret yok; kılavuz 1 bit palet PNG'ye indirilip yeniden gönderildi) + 2 ikinci deneme (yatak: mor/magenta kayma; kütük masa: pembe ahşap) = 10 generation (cost_max 18 içinde). Araçta `seed` yok, ikinci deneme aynı kılavuz ve maskeyle.
`get_balance` iş sonu: 1001,7 (paralel şeritler de harcıyor).

## Sonuç özeti
| # | Eşya | Durum | Not |
|---|---|---|---|
| 1 | bench_long | teslim | |
| 2 | mattress_straw | teslim | 7 renk, tüm yüzler düz (sol=sağ parlaklık 154) |
| 3 | bed_frame | TESLİM EDİLMEDİ | 1. deneme mor/magenta yorgan (kırmızı %68, KALDI); 2. deneme palet dışı %42 (KALDI). Tanınıyor, yorgan kırmızı-sarı; kullanıcı `302-red/` içine bakabilir |
| 4 | cradle | TESLİM EDİLMEDİ | stil KALDI (doygunluk x1,94 aileden çok doygun); tanınıyor ama turuncu; ikinci deneme koşulu (tanınmama, ayak izi, yasak renk) oluşmadığı için yeniden denenmedi |
| 5 | child_bed | teslim (UYARI) | palete uzak (uzaklık 11,3) |
| 6 | stump_table | teslim (2. deneme) | 1. deneme pembe ahşap + KALDI; 2. deneme GEÇTİ, üst yüzey düz, halat bağ |
| 7 | prep_counter | teslim | |
| 8 | armchair_cushion | teslim | gerçek yön SE |

## Dosyalar (Assets/Art/Iso/_aday/)
| Yol | Piksel | Pivot x | Pivot y (alttan) |
|---|---|---|---|
| iso_item_bench_long_2x1_se_nw.png | 112x80 | 56 | 32 |
| iso_item_mattress_straw_se.png | 80x56 | 40 | 24 |
| iso_item_child_bed_se.png | 80x72 | 40 | 24 |
| iso_item_stump_table_se.png | 80x64 | 40 | 24 |
| iso_item_prep_counter_2x1_se_nw.png | 112x88 | 56 | 32 |
| iso_item_armchair_cushion_se.png | 80x88 | 40 | 24 |

## Doğrulama (validate_iso, `--max-colors 48`; hepsi geçti)
| Dosya | Opak sınır kutusu | Renk | Yarı saydam | Alta değme | Maske dışı | Kılavuz kalıntısı |
|---|---|---|---|---|---|---|
| bench | x9-102, y7-68 | 23 | 0 | yok | 0 | 0 |
| mattress | x8-71, y5-48 | 7 | 0 | yok | 0 | 0 |
| child_bed | x9-70, y8-62 | 39 | 0 | yok | 0 | 0 |
| stump_table | x10-69, y8-54 | 21 | 0 | yok | 0 | 0 |
| prep_counter | x12-100, y3-79 | 33 | 0 | yok | 0 | 0 |
| armchair | x14-65, y7-80 | 32 | 0 | yok | 0 | 0 |
| (red) bed 2. deneme | x10-101, y4-80 | 25 | 0 | yok | 0 | 0 |
| (red) cradle | x8-71, y10-61 | 29 | 0 | yok | 0 | 0 |

## Stil denetimi (`style.py check --family esya`)
| Dosya | Palete yakın | Ort. uzaklık | Doygunluk | Sonuç |
|---|---|---|---|---|
| bench | %100 | 9,2 | x0,61 | GEÇTİ |
| mattress | %100 | 6,2 | x1,17 | GEÇTİ |
| child_bed | %100 | 11,3 | x0,88 | UYARI |
| stump_table | %100 | 6,1 | x0,80 | GEÇTİ |
| prep_counter | %100 | 7,6 | x1,02 | GEÇTİ |
| armchair | %100 | 3,7 | x0,83 | GEÇTİ |
| bed_frame (teslim edilmedi) | %58 | 17,4 | x0,92 | KALDI |
| cradle (teslim edilmedi) | %100 | 9,1 | x1,94 | KALDI |

## Palet ve ışık (ort. parlaklık: üst / sol / sağ; ton, doygunluk)
| Dosya | Üst | Sol | Sağ | Ton° | Doy. |
|---|---|---|---|---|---|
| bench | 91 | 83 | 80 | 194 | 0,36 |
| mattress | 210 | 154 | 154 | 89 | 0,28 |
| child_bed | 111 | 86 | 121 | 142 | 0,45 |
| stump_table | 99 | 69 | 77 | 11 | 0,54 |
| prep_counter | 104 | 83 | 68 | 58 | 0,47 |
| armchair | 41 | 48 | 72 | 29 | 0,43 |
⚠ Ton ölçümü gürültülü (koyu/açık ahşap + kumaş karışık); tek ahşap tonu için görsele bakılmalı. Sağ yüzü sol yüzden açık olanlar (child_bed, stump_table, armchair) ışık sırası uyarısı: ⚠ kullanıcı bakmalı. Takım uyumu (aynı takım yan yana) için ortalama ton tabloda; Köylü (bench, mattress, stump) ve Zanaatkâr (child_bed, counter, armchair) renkleri sayfada karşılaştırılmalı.

## Ayna uygunluğu (sol/sağ şekil simetrisi)
mattress ve stump_table %100 (tek sprite her yöne), bench %40, child_bed %76, counter %74, armchair %72 (ayna ile SE↔SW; aynada ışık sağ-üstten, Assets'e konmadı). Armchair gerçek yönü SE.

## Sorunlar
- Yüzey üst y aralığı ve yuva işareti ölçülmedi (otomatik betik yok); yan yana sayfasına bakılmalı.
- Tarifteki "dark brown wood" eki Köylü "light pine" ile çelişiyor; bench belirgin koyu mor-kahve çıktı.
- Teslim edilmeyen yatak ve beşik için kullanıcı kararı gerekir (yatak 1. ve 2. deneme `302-red/`).
