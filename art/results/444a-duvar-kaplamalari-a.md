unity-import: pending

# 444a Yeni duvar kaplamaları A (bulut işçisi, şerit 1, 2026-11-19)

Yan yana sayfası: [444a-yanyana.png](444a-yanyana.png) (referanslar + masonry x2, blue_wash sw_ne, stencil x2 ilk ikisi sw_ne/se_nw, mural sw_ne, star_tile sw_ne)

## Araç ve ayarlar
- `inpaint_image_pro_flash`, "Modify current layer", `no_background=true`, `seed=0` (328 ile aynı yöntem).
- Maliyet kalemi: 5 gen/çağrı (96 px'e kadar). 10 çağrı 50 gen olacağı ve `cost_max` 30'u aşacağı için **iki ekseni tek 96x96 kaynağa yan yana koydum** (sw_ne 48x96 kırpma solda, se_nw 48x96 sağda; yüz maskesi betikle üretildi, kenar direkleri ve üst kapak dışarıda). Böylece 5 çağrı x 5 = **25 generation**, iki eksen aynı malzemeyi tek çağrıda alıyor. Çıktı bölünüp 102x115 tuvale kaynağın siluetiyle geri kondu (silüet dışına piksel yazılmadı).
- `get_balance` sonrası kalan: **3382,9** (diğer şeritlerin harcaması dahil). Hız sınırı hatası yok.
- Masaüstü kopyası: yerel.

## Dosyalar (hepsi 102x115, pivot x=32 sw_ne / 70 se_nw, y=19 ALTTAN)
| Dosya | Renk | Stil | Durum |
|---|---|---|---|
| iso_wall_finish_masonry_sw_ne | 45 | UYARI (palete uzak, doygunluk x1.39) | teslim |
| iso_wall_finish_masonry_se_nw | 44 | UYARI (x1.40) | teslim |
| iso_wall_finish_blue_wash_sw_ne | 32 | UYARI (%93, x1.00) | teslim |
| iso_wall_finish_blue_wash_se_nw | 38 | KALDI (palete yakın %65) | teslim edilmedi |
| iso_wall_finish_stencil_sw_ne | 28 | GEÇTİ ama içerik yok (yalnızca çıplak sıva, desen boyanmamış) | teslim edilmedi |
| iso_wall_finish_stencil_se_nw | 35 | KALDI (%75, x1.31) | teslim edilmedi |
| iso_wall_finish_mural_sw_ne / se_nw | 61 / 63 (>48) | KALDI (%78 / %57) | teslim edilmedi |
| iso_wall_finish_star_tile_sw_ne / se_nw | 48 / 47 | KALDI (%55 / %57, x1.66) | teslim edilmedi |

Teslim edilmeyenler `art/444a-red/` altında. Hepsinde şekil tanınıyor ama "palet dışı renk" de var; `fit` yalnızca tek başına doygunluk sorunu için izinli olduğundan uygulanmadı.

## Sorunlar / öneri
- Tek deneme kuralı gereği yeniden denenmedi. Eksik: stencil (iki eksen), mural, star_tile, blue_wash se_nw. Yeni iş gerekir (ör. önce `reduce_colors`/`fit`, ya da palet kısıtlı açıklama; iki ekseni ayrı çağrılarla, star_tile için daha soluk turkuaz).
- Masonry rengi sarımsı yeşile kaçtı (kireç sıva değil); kullanıcı yan yana sayfasına baksın.
- Dikiş testi (komşu duvarla) gözle yapılmadı; alçak/yarım varyantlar (402 betiği) bu iş için üretilmedi.
