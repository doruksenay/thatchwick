unity-import: pending

# 450 Merdivenler ve korkuluk maskesi (rapor)

Yan yana sayfası: [450-yanyana.png](450-yanyana.png). Teslim edilmeyen: `art/results/450-red/`. Masaüstü kopyası: yerel.

## Araç ve ayarlar
`create_map_object`, stil eşleme, `footprint_guide.py` kılavuz + maske (1x2, `--axis x`, tuval 112x96, yükseklik 40, referanssız), `high top-down`, `low detail`, `basic shading`, `single color outline`, 301 ortak eki. 4 çağrı x 1 gen = 4 generation (cost_max 24 içinde). Yeniden deneme yok; tek istisna: 4. çağrı (taş iniş) üç kez base64 aktarım kesilmesiyle üretime girmedi (ücret yok), kılavuz 2 renkli 1 bit PNG'ye indirilip aynı girdiyle gönderildi.
`get_balance` sonrası: 3098,9 (paralel şeritler de harcıyor).

## Sonuç özeti
| # | Eşya | Durum | Not |
|---|---|---|---|
| 1 | stairs_up_wood | teslim (UYARI), MASKESİZ | doygunluk x1,30. Korkuluk gri değil, gövdeyle aynı kahverengi: `accent_mask` 0 piksel |
| 2 | stairs_down_wood | TESLİM EDİLMEDİ | eşya değil, duvarlı ve zeminli bir oda çıktı (ayak izi dışı duvar). `450-red/` |
| 3 | stairs_up_stone | teslim (UYARI), MASKESİZ | palete uzak (12,3). Korkuluk bakır tonunda, basamak/tuğla renkleriyle örtüşüyor; 0 piksel |
| 4 | stairs_down_stone | teslim, MASKESİZ | GEÇTİ. Korkuluk ince koyu bir çizgi (üstte karmaşık kıvrım), gri değil. Anahtar #C8C8C8 350 piksel seçti ama bunlar basamak üst yüzeyi (220,191,171): yanlış, maske yazılmadı |

Maske neden yok: model "pale grey" korkuluğu çizmedi, korkuluk gövde renginde. Anahtar rengi görselden seçmek (ör. up_stone 168,102,102) basamak/tuğla piksellerini de yakaladığı için temiz maske vermiyor; yanlış maske üretmemek için yazılmadı. Öneri: korkuluğu ayrı nesne olarak üretmek ya da `_accent` olmadan gövde rengini kullanmak.

## Dosyalar (Assets/Art/Iso/_aday/)
| Yol | Piksel | Pivot x | Pivot y (alttan) |
|---|---|---|---|
| iso_item_stairs_up_wood_1x2_se.png | 112x96 | 56 | 32 |
| iso_item_stairs_up_stone_1x2_se.png | 112x96 | 56 | 32 |
| iso_item_stairs_down_stone_1x2_se.png | 112x96 | 56 | 32 |

## Doğrulama (validate_iso, `--max-colors 48`)
| Dosya | Opak sınır | Renk | Yarı saydam | Alta boşluk | Sonuç |
|---|---|---|---|---|---|
| up_wood | x13-99, y10-86 | 27 | 0 | 9 | geçti |
| up_stone | x13-101, y2-88 | 47 | 0 | 7 | geçti |
| down_stone | x14-100, y12-88 | 25 | 0 | 7 | geçti |
| (red) down_wood | x9-102, y7-87 | 21 | 0 | 8 | geçti (ama oda) |

## Stil denetimi (`style.py check --family esya`)
| Dosya | Palete yakın | Ort. uzaklık | Doygunluk | Sonuç |
|---|---|---|---|---|
| up_wood | %100 | 6,1 | x1,30 | UYARI |
| up_stone | %98 | 12,3 | x0,82 | UYARI |
| down_stone | %100 | 6,3 | x0,67 | GEÇTİ |
| down_wood (red) | %100 | 9,6 | x0,49 | UYARI |

Işık: sol/sağ ortalama parlaklık up_wood 54/61, up_stone 107/121, down_stone 88/111 (sağ biraz açık; gölge pişirilmemiş). Zemin değil, döşeme testi yok.

## Sorunlar
- Hiçbir görselde #C8C8C8 korkuluk yok; 4 maskenin hiçbiri üretilemedi.
- down_wood oda çıktı; istenirse yeniden iş açılır.
