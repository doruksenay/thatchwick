unity-import: pending

# 562 Ata tohumu rozetleri, ata çeşitleri, hasat aletleri: sonuç (durum: review, kısmi)

Yan yana sayfası: `art/results/562-yanyana.png` (12 ikonun hepsi; arayuz ailesinde referans yok).

## Araç ve ayarlar
- `create_image_pixen`, 32x32, `no_background=true`, `outline: single color outline`, `detail: low detail`; her açıklamanın sonuna tarifteki "no face, no eyes, no creature, no person" eki. Gerekçe: 316 ile aynı yöntem (ikon başına bir çağrı).
- 12 çağrı x 1 gen = **12 gen** (`cost_est` 12, `cost_max` 24). 2 çağrı 10/10 hız sınırı hatası aldı (harcama yok), bir sonraki turda yeniden gönderildi (tarifin tek istisnası).
- `get_balance` sonrası: 1952,7 (paralel şeritler dahil).
- Renk indirgeme: ham 23-54 renk, Pillow median-cut 16 renk, dither yok, alfa 0/255; yeniden boyama yok. Ham hâller yerelde `art/tmp562/` (commit edilmedi).
- `map_object` ile yeniden üretim yapılmadı: tek deneme kuralı (`max_attempts: 1`); öneri aşağıda.
- Masaüstü kopyası: yerel.

## Teslim edilenler (`Assets/Art/Iso/_aday/`, 32x32 RGBA, pivot x 16, pivot y 0 alttan; ikon, pivot önemsiz)
| Dosya | Stil denetimi | Not |
|---|---|---|
| ui_icon_heirloom_seed.png | GEÇTİ | bordo buğday başağı, keten yama |
| ui_icon_heirloom_seed_gold.png | UYARI (palete uzak %84) | altın başak, kurdele yok sayılabilir (yuvarlak altın çerçeve) |
| ui_icon_tool_sickle.png | UYARI (palete uzak) | tanınıyor |
| ui_icon_tool_scythe.png | GEÇTİ | tanınıyor |
| ui_icon_field_ripe.png | GEÇTİ | başak; kıvılcım belirsiz |
| ui_icon_field_thirsty.png | GEÇTİ | fide + su damlası, tanınıyor |

## Teslim EDİLMEYENLER (`art/562-red/`)
| Dosya | Sebep |
|---|---|
| icon_item_carrot_purple.png | `style.py check` KALDI (palet dışı renkler) ve yüz çizilmiş, havuç yerine pancara benziyor |
| icon_item_carrot_yellow.png | yüzlü (kawaii), "no face" eki dinlenmedi |
| icon_item_radish_black.png | tek göz/bomba gibi, turp değil; tuvali dolduruyor |
| icon_item_cabbage_red.png | `style.py check` KALDI (palet dışı renkler, %65); şekil tanınıyor ama tuvale taşıyor. `fit` referans yok diye çalışmadı |
| icon_item_harvest_doll.png | yüzlü insan figürü (talimata aykırı), saman bebeği değil |
| ui_icon_field_mix_risk.png | GEÇTİ ama okunmuyor: iki tohum başı yerine yaprak/kanat; uyarı noktası yok |

## Doğrulama
| Ölçüt | Sonuç |
|---|---|
| Boyut | 32x32 (12/12) |
| Yarı saydam piksel | 0 |
| Renk | ≤16 |
| `style.py check --family arayuz` | 12 ikon: 2 KALDI (cabbage, carrot_purple), 3 UYARI, 7 GEÇTİ |
| `validate_iso.py` | uygulanmadı (izometrik değil) |

## Sorunlar / öneri
1. 6/12 ikon teslim edilmedi. Önerilen yeniden üretim işi (yeni iş, ~6-12 gen): ürün ikonları ve saman bebeği için `create_map_object` 32x32, `view: side`, `detail: low detail` (401/417 yöntemi); turp tarifini "round radish with black skin, root tail, green leaves" diye açmak; mix_risk için iki ayrı başak + ünlem noktası daha açık tarif.
2. Pixen yüz eğilimi "no face" ekine rağmen sürdü (carrot, doll, radish).
