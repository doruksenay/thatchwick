unity-import: pending

# 444t Duvar, kapı, korkuluk ve çit tamamlama (bulut işçisi, şerit 1, 2026-11-20)

Yan yana sayfası: [444t-yanyana.png](444t-yanyana.png) (3 referans + 18 yeni; sayfada teslim edilmeyenler de var). Büyütülmüş inceleme için yalnızca gözle bakıldı.

## Araç ve ayarlar
- `inpaint_image_pro_flash`, `Modify current layer`, `no_background=true`, `seed=0`. 10 çağrı x 5 = **50 generation** (cost_max 50; araç açıklaması 96 px için 5). Hız sınırı hatası yok. `get_balance` sonrası kalan: **2598,9** (diğer şeritler dahil).
- İki eksen tek 96x96 kaynağa yan yana konup tek çağrıda üretildi (444a yöntemi), çıktı 102x115 tuvale geri bölündü; finish/kapıda silüet dışı sildi.
- Kaynaklar: finish: `iso_wall_wall_cob_*` (yüz maskesi, kenar ve üst kapak dışarıda); kapı: `iso_wall_door_*` (kapı kanadı maskesi, 446a ile aynı); korkuluk: **kabul edilmiş `iso_wall_railing_wood_*`** (biçim korunsun diye); çit: `iso_wall_fence_wattle_*`. `blue_wash` ve `tapestry` se_nw için sol yarıya kabul edilmiş sw_ne konup yalnızca sağ yarı maskelendi (aynı doku/renk).
- Masaüstü kopyası: yerel.

## Teslim edilenler (`Assets/Art/Iso/_aday/`, 102x115)
Pivot: x sw_ne 32, se_nw 70; y ALTTAN 19.
| Dosya | Renk | Stil | Sonuç |
|---|---|---|---|
| iso_wall_finish_masonry_sw_ne_v2 | 43 | GEÇTİ (%100, x0.88) | düzgün sıralı gri-bej taş, kırmızı çizgi yok |
| iso_wall_finish_masonry_se_nw_v2 | 31 | GEÇTİ (%100, x0.78) | |
| iso_wall_finish_blue_wash_se_nw | 42 | UYARI (palete uzak, %91) | sw_ne ile aynı |
| iso_wall_finish_tapestry_se_nw | 45 | UYARI (doygun x1.50) | sw_ne ile aynı dokuma |
| iso_wall_finish_stencil_sw_ne | 39 | UYARI (doygun x1.32) | küçük tekrarlı motif |
| iso_wall_finish_stencil_se_nw | 42 | GEÇTİ (x1.21) | |
| iso_wall_door_green_sw_ne / se_nw | 43 / 45 | GEÇTİ (%84 / %81) | kalaslı kapı, koyu yeşil |
| iso_wall_fence_hedge_sw_ne / se_nw | 11 / 12 | UYARI (palete uzak, x0.63-0.66 soluk) | kutu biçimli budanmış çit |
| iso_wall_railing_black_sw_ne_v2 / se_nw_v2 | 9 / 12 | UYARI (soluk, siyah) | düzgün tırabzan, dikme, kuşak |

`validate_iso.py --kind object --max-colors 48`: teslim edilenlerin hepsi geçti.

## Teslim edilmeyenler (`art/444t-red/`, 6 dosya)
- `mural` sw_ne/se_nw: 60 / 58 renk > 48 (validate KALDI), stil UYARI; ayrıca duvarı tek resim gibi dolduruyor (tarif tersini istiyor).
- `star_tile` sw_ne/se_nw: stil KALDI (palet dışı %80).
- `door_blue` sw_ne/se_nw: stil KALDI (palet dışı %79-80); şekil düzgün kapı kanadı, palet sorunu (açık mavi).
Doygunluk dışı palet sorunu `fit` ile çözülmediği için uygulanmadı. Tek deneme (max_attempts) kuralı gereği tekrar yok.

## Yapılmayanlar (bütçe)
`railing_red`, `railing_green`, `railing_iron_red` (6 dosya) üretilmedi: tek iş tavanı 50 gen, 13 çağrı gerekirdi (65). Yeni iş gerekir: `railing_wood`/`railing_iron` kaynağıyla (bu işteki `art/tmp` yöntemi), kırmızı ve yeşil için palete yakın soluk tonlar.

## Sorunlar / öneri
- Döşeme testi duvar işi olmadığı için yapılmadı; ışık/gölge gözle denetlendi (sol-üst, pişmiş gölge yok).
- Öneri: `tools/art`'a iki-eksen-tek-çağrı kaynak/maske betiği (`art/tmp/prep.py` mantığı) eklensin.
