unity-import: pending

# 389 Çatkı Evi mobilyası (9) (rapor)

Yan yana sayfası: [389-yanyana.png](389-yanyana.png). Masaüstü kopyası: yerel.

## Araç ve ayarlar
`create_map_object`, stil eşleme, `footprint_guide.py --axis x` kılavuzu + maske (referanssız), `high top-down`, `low detail`, `basic shading`, `single color outline` (301 ile aynı). Açıklama = tablodaki metin + takım eki (Köylü: rough light pine, rope lashing; Zanaatkâr: dark oak with iron fittings; Tüccar: painted wood, brass; Asil: dark walnut, wrought iron) + 301 ortak eki. Araçta `seed` yok. Çağrı başı 1 generation, 9 çağrı = 9 gen (cost_max 18 içinde), ikinci deneme yapılmadı.
Olaylar: clothes_press çağrısı bir kez hız sınırına (rate limit) takıldı; bekleyip yeniden denendi (izin verilen tek istisna). Sonraki 2 deneme benim base64 aktarım hatamdan reddedildi (üretime girmedi, ücret yok).
`get_balance` iş sonu: 4607,9 (paralel şeritler de harcıyor).

## Dosyalar (Assets/Art/Iso/_aday/)
| Yol | Piksel | Pivot x | Pivot y (alttan) |
|---|---|---|---|
| iso_item_settle_bench_2x1_se.png | 112x104 | 56 | 32 |
| iso_item_chest_bench_2x1_se.png | 112x80 | 56 | 32 |
| iso_item_trestle_table_3x1_se.png | 144x104 | 72 | 40 |
| iso_item_aumbry_se.png | 80x104 | 40 | 24 |
| iso_item_milking_stool_se.png | 80x56 | 40 | 24 |
| iso_item_x_chair_se.png | 80x88 | 40 | 24 |
| iso_item_walnut_table_3x1_se.png | 144x104 | 72 | 40 |
| iso_item_clothes_press_2x1_se.png | 112x120 | 56 | 32 |
| iso_item_bed_end_chest_2x1_se.png | 112x88 | 56 | 32 |

Kılavuz tuvalleri: settle 112x104 (h56), chest_bench 112x80 (h30), trestle 144x104 (h28, n=3), aumbry 80x104 (h64), stool 80x56 (h20), x_chair 80x88 (h44), walnut 144x104 (h30, n=3), clothes_press 112x120 (h64), bed_end_chest 112x88 (h26).

## Doğrulama (validate_iso `--kind object --max-colors 48 --footprint`; hepsi geçti)
| Dosya | Opak sınır kutusu | Renk | Yarı saydam | Alta değme |
|---|---|---|---|---|
| settle_bench | x9-102, y7-96 | 17 | 0 | yok |
| chest_bench | x18-100, y7-72 | 26 | 0 | yok |
| trestle_table | x8-135, y3-93 | 32 | 0 | yok |
| aumbry | x10-69, y10-96 | 15 | 0 | yok |
| milking_stool | x18-61, y8-48 | 35 | 0 | yok |
| x_chair | x13-65, y10-80 | 13 | 0 | yok |
| walnut_table | x9-134, y3-94 | 15 | 0 | yok |
| clothes_press | x11-101, y0-112 | 37 | 0 | yok (üst kenara değiyor, y0) |
| bed_end_chest | x10-101, y5-80 | 31 | 0 | yok |

## Stil denetimi (`style.py check --family esya`)
| Dosya | Palete yakın | Ort. uzaklık | Doygunluk | Sonuç |
|---|---|---|---|---|
| settle_bench | %100 | 4,1 | x1,00 | GEÇTİ |
| chest_bench | %100 | 8,3 | x0,44 | UYARI (ailesinden soluk) |
| trestle_table | %100 | 6,1 | x0,90 | GEÇTİ |
| aumbry | %99 | 6,1 | x0,51 | UYARI (ailesinden soluk) |
| milking_stool | %100 | 6,5 | x0,87 | GEÇTİ |
| x_chair | %100 | 7,4 | x0,73 | GEÇTİ |
| walnut_table | %100 | 6,2 | x0,74 | GEÇTİ |
| clothes_press | %99 | 9,3 | x0,62 | GEÇTİ |
| bed_end_chest | %100 | 4,8 | x0,91 | GEÇTİ |

## Sorunlar (kullanıcı görsele bakmalı)
- Görsel gözlem: chest_bench Köylü "açık çam" yerine pembemsi-mor kahve çıktı; x_chair (Asil ceviz) bordo/kırmızımsı; clothes_press çok koyu mor-kahve ve parlak sarı (pirinç) bantlı. Renk kayması "kırmızı/pembe ahşap" kuralına yakın; ikinci deneme koşulu (tanınmama, ayak izi, boş) oluşmadığı için yeniden denenmedi.
- trestle_table ve milking_stool Köylü masa/tabure ile uyumlu kırmızı-kahve; takım içi uyum yan yana sayfasında bakılmalı.
- clothes_press opak alan üst kenara (y0) değiyor; kırpma gerekmedi ama pivot/yükseklik için kullanıcı baksın.
- Zemin döşeme testi (zemin değil) ve ışık parlaklık ölçümü yapılmadı; ışık sırası sayfada gözle bakılmalı. Ayna uygunluğu ölçülmedi.
- Eşya → dosya eşlemesi ve pivotlar kod oturumu/kullanıcı işi (`IsoArt.cs`, `IsoPivots.txt`).
- Öneri: araç eksik değil; base64 aktarımı büyük kılavuzlarda hataya açık (elle kopyalama), ileride kılavuzu aracın kendisi üretebilse iyi olur.
