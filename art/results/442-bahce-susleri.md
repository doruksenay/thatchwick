unity-import: pending

# 442 Bahçe: çimen sıra, güneş saati, güvercinlikler (rapor)

Yan yana sayfası: [442-yanyana.png](442-yanyana.png). Teslim edilmeyenler: `art/results/442-red/`. Masaüstü kopyası: yerel.

## Araç ve ayarlar
`create_map_object`, stil eşleme, `footprint_guide.py` kılavuz + maske (referanssız), `high top-down`, `low detail`, `basic shading`, `single color outline`; tarif metni + "muted natural colors, no red, no pink". 4 çağrı x 1 gen = 4 generation (cost_max 18). Yeniden deneme yok.
Kılavuz tuvalleri: çimen sıra 112x80 h20 (`--n 2 --axis x`), güneş saati 80x72 h36, kerpiç güvercinlik 80x84 h46, kule güvercinlik 144x144 h80 (`--n 2 --m 2`).
`get_balance` sonrası: 3442,9 (paralel şeritler de harcıyor).

## Sonuç özeti
| # | Eşya | Durum | Not |
|---|---|---|---|
| 1 | turf_seat | TESLİM EDİLMEDİ | 50 renk > 48 (validate BAŞARISIZ); stil KALDI (palete yakın %56, palet dışı renkler). Tanınıyor (taş kenarlı çimen kasa). `442-red/` |
| 2 | sundial | teslim | |
| 3 | dovecote (2x2) | TESLİM EDİLMEDİ | validate ve stil geçti, ama kule altında çimen ve döşeme kaplı elmas zemin pişmiş (kılavuz zeminine boyanmış); nesne ayak izi zemin karosu gibi çiziyor. Tek deneme kuralı, yeniden denenmedi. `442-red/`. Kullanıcı isterse kuleyi maskeyle zeminden ayırabilir |
| 4 | dovecote_village | teslim | |

## Dosyalar (Assets/Art/Iso/_aday/)
| Yol | Piksel | Pivot x | Pivot y (alttan) |
|---|---|---|---|
| iso_item_sundial_se.png | 80x72 | 40 | 24 |
| iso_item_dovecote_village_se.png | 80x84 | 40 | 24 |

## Doğrulama (validate_iso, `--max-colors 48`)
| Dosya | Opak sınır kutusu | Renk | Yarı saydam | Alta değme | Sonuç |
|---|---|---|---|---|---|
| sundial | x8-71, y6-64 | 41 | 0 | yok | geçti |
| dovecote_village | x12-67, y7-76 | 35 | 0 | yok | geçti |
| (red) turf_seat | x8-103, y3-72 | 50 | 0 | yok | BAŞARISIZ (50>48) |
| (red) dovecote | x9-134, y9-136 | 37 | 0 | yok | geçti, zemin pişmiş |

## Stil denetimi (`style.py check --family esya`)
| Dosya | Palete yakın | Ort. uzaklık | Doygunluk | Sonuç |
|---|---|---|---|---|
| sundial | %100 | 9,8 | x0,76 | GEÇTİ |
| dovecote_village | %100 | 6,5 | x1,03 | GEÇTİ |
| turf_seat (red) | %56 | 18,9 | x1,05 | KALDI |
| dovecote (red) | %97 | 8,1 | x0,91 | GEÇTİ |

## Sorunlar
- Çimen sıra ve kule güvercinlik için yeniden üretim gerekir (öneri: 442b; çimen sırada palet kayması, kulede zemin pişmesi nedeniyle kılavuz rengini daha ayırt edilir seçmek ya da "no ground, no paving" eklemek).
- Döşeme testi: zemin işi değil, uygulanmadı.
