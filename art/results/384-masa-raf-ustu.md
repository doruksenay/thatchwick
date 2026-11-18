unity-import: pending

# 384 Masa ve raf üstü: oyunlar, oymalar, kutu, bez bebek, el aynası (rapor)

Yan yana sayfası: [384-yanyana.png](384-yanyana.png). Ham kuş: `art/results/384-red/`. Masaüstü kopyası: yerel.

## Araç ve ayarlar
`create_map_object`, stil eşleme, referanssız kılavuz + maske (301/382 yöntemi; `footprint_guide.py` yalnızca 64x32 taban yaptığı için kılavuz `art/tmp384/` altında Python ile üretildi). Taban elması alt kenardan 4 px yukarıda, maske eşya yüksekliği + 1 px. `high top-down`, `low detail`, `basic shading`, `single color outline`, 301 ortak eki + takım eki. 11 çağrı x 1 gen = 11 generation (cost_max 16). `get_balance` iş sonu: 4770,90 (paralel şeritler de harcıyor).
- Hatam: kuşta carving kılavuzu (32x44, taban 16, maske 28 yüksek) kullanıldı; kuş tarifteki 16x20 yerine 32x44 tuvalde, büyük çıktı. Lockbox ilk çağrıda yanlış (32x36) kılavuzla gitti, doğru kılavuzla yeniden üretildi (+1 gen; yanlış çıktı kullanılmadı).
- Düdükte hız sınırı hatası: ~40 sn sonra bir kez yeniden denendi, tuttu.
- Tuvaller alt sınır (32 px) ve taban payı yüzünden tarifteki tuvalden büyük.

## Dosyalar (Assets/Art/Iso/_aday/)
| Yol | Piksel | Pivot x | Pivot y (alttan) | Stil denetimi |
|---|---|---|---|---|
| iso_item_wood_carving_se.png | 32x44 | 16 | 8 | UYARI soluk (x0.54), koyu, at tanınıyor |
| iso_item_carved_bird_se.png | 32x44 | 16 | 8 | UYARI (`fit` x0.87 sonrası; ham KALDI %61, x1.32); büyük ve uzun |
| iso_item_lockbox_se.png | 40x36 | 20 | 10 | GEÇTİ |
| iso_item_tally_stick_se.png | 40x32 | 20 | 10 | GEÇTİ |
| iso_item_dice_knucklebones_se.png | 36x32 | 18 | 9 | GEÇTİ (zar/kemik ayırt etmek zor) |
| iso_item_chess_set_se.png | 48x32 | 24 | 12 | UYARI doygun (x1.32) |
| iso_item_backgammon_se.png | 48x32 | 24 | 12 | UYARI palete uzak; yassı koyu kutu, taşlar belirsiz |
| iso_item_rag_doll_se.png | 32x40 | 16 | 8 | UYARI doygun (x1.36); küçük insan gibi okunuyor |
| iso_item_wooden_whistle_se.png | 32x32 | 16 | 8 | GEÇTİ (çok küçük) |
| iso_item_copper_hand_mirror_se.png | 32x40 | 16 | 8 | UYARI palete uzak, doygun (x1.27); metal disk, sap ince |

Pivotlar kılavuzdaki taban elmasının merkezidir.

## Doğrulama (validate_iso `--kind object --max-colors 48`)
Hepsi geçti: yarı saydam 0, alta değme yok, renk 7-39.
| Dosya | Opak sınır kutusu | Renk |
|---|---|---|
| backgammon | x10-37, y6-23 | 34 |
| bird (ham) | x8-23, y4-39 | 14 |
| carving | x8-23, y5-39 | 7 |
| chess | x8-39, y3-28 | 39 |
| dice | x8-27, y10-21 | 20 |
| doll | x8-23, y3-35 | 18 |
| lockbox | x8-31, y6-30 | 17 |
| mirror | x8-23, y3-35 | 11 |
| tally | x16-23, y3-28 | 7 |
| whistle | x10-21, y12-22 | 13 |

## Sorunlar
- Tavla düz ve belirsiz; kuş ve bebek ölçek/oran olarak tarifin dışında. Karar yan yana sayfasına göre kullanıcıda.
- Işık sol-üstten, gölge pişirilmemiş (gözle). Eşya olduğu için döşeme testi yok.
- Öneri: `footprint_guide.py`'ye `--base-w` eklenmeli.
