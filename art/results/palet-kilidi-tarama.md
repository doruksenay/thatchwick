# Palet kilidi: ilk tarama (2026-11-20)

Kaynak: donmuş paletler `art/style/palette.json` (dünya, 112 renk) ve `art/style/palette_arayuz.json` (ikon ve arayüz, 128 renk); ikisi de `art/queue`'daki kabul edilmiş işlerin çıktılarından ton dilimli üretildi. Karar: kullanıcı 20.11.2026, Project belgesi 64c §1 (seçenek a).
Kapsam: oyunun görsel kütüphanesindeki (`Assets/Resources/IsoArt/IsoArtLibrary.asset`) bütün görseller, 2.354 dosya. Eşik `style.py check` ile aynı: palete ortalama uzaklık 16'dan büyükse ya da palete yakın piksel %80'in altındaysa KALIR. Doygunluk aileye göre denetlendiği için bu tabloda yok. **Hiçbir görsel değiştirilmedi.**

## Özet

| | Eski palet (64 renk) | Yeni paletler |
|---|---|---|
| KALIR | 272 | 43 |
| UYARI | 174 | 112 |
| KALIR, kabul edilmiş iş çıktısı | — | 35 |

## Kalanlar

Kabul: A = kabul edilmiş işin çıktısı, - = aday ya da incelemede.

| Palet | Grup | Görsel | Palete yakın | Ortalama uzaklık | Kabul |
|---|---|---|---|---|---|
| arayuz | icon | `icon_item_cloth` | %43 | 23.4 | A |
| arayuz | icon | `icon_item_smoked_cheese` | %19 | 22.5 | A |
| arayuz | icon | `icon_item_game_meat` | %50 | 21.4 | A |
| arayuz | icon | `icon_item_pear` | %21 | 20.7 | A |
| arayuz | icon | `icon_item_salted_meat` | %34 | 20.5 | A |
| arayuz | icon | `icon_item_cheese` | %79 | 19.4 | A |
| arayuz | icon | `icon_item_resin` | %42 | 19.4 | A |
| arayuz | icon | `icon_item_serbet` | %61 | 19.3 | A |
| arayuz | icon | `icon_item_turnip` | %64 | 19.3 | A |
| arayuz | icon | `icon_item_meat` | %57 | 18.6 | A |
| arayuz | icon | `icon_item_salted_poultry` | %54 | 17.9 | A |
| arayuz | icon | `icon_item_wheat` | %64 | 17.4 | A |
| arayuz | icon | `icon_item_cabbage` | %53 | 17.2 | A |
| arayuz | icon | `icon_item_wrap_fish` | %59 | 16.8 | A |
| arayuz | icon | `icon_item_pumpkin` | %67 | 16.8 | A |
| arayuz | icon | `icon_item_salgam` | %71 | 16.6 | A |
| arayuz | icon | `icon_glass` | %72 | 15.9 | - |
| arayuz | icon | `icon_item_smoked_fish` | %42 | 15.8 | A |
| arayuz | icon | `icon_item_salted_fish` | %60 | 14.7 | A |
| arayuz | icon | `icon_item_cherry` | %79 | 14.6 | A |
| arayuz | icon | `icon_item_pipe_clay` | %75 | 13.8 | A |
| arayuz | icon | `icon_item_soup_chicken` | %63 | 13.7 | A |
| arayuz | icon | `icon_item_jam_berry` | %77 | 12.3 | A |
| arayuz | ui | `ui_mine_water` | %49 | 22.1 | - |
| arayuz | ui | `ui_icon_house_our_blood` | %67 | 18.8 | A |
| arayuz | ui | `ui_icon_act_warm` | %83 | 16.6 | A |
| arayuz | ui | `ui_icon_tool_conduit_ditch` | %73 | 16.5 | A |
| arayuz | ui | `ui_icon_tool_conduit_flume` | %66 | 13.8 | A |
| arayuz | ui | `ui_icon_misc_heart` | %61 | 13.2 | A |
| arayuz | ui | `ui_icon_weather_sun` | %76 | 12.6 | A |
| arayuz | ui | `ui_icon_act_chop` | %57 | 12.5 | A |
| arayuz | ui | `ui_icon_need_energy` | %75 | 11.6 | A |
| arayuz | ui | `ui_icon_house_wedding` | %79 | 8.2 | A |
| dunya | iso_char | `iso_char_farmer_a_64_cand_n` | %78 | 10.3 | - |
| dunya | iso_crop | `iso_crop_turnip_3_se` | %51 | 16.2 | A |
| dunya | iso_fx | `iso_fx_light_ring` | %0 | 0.0 | A |
| dunya | iso_ground | `iso_ground_dirt_set_cand3` | %100 | 19.0 | - |
| dunya | iso_ground | `iso_ground_dirt_set_cand7` | %100 | 19.0 | - |
| dunya | iso_ground | `iso_ground_dirt_set_cand4` | %100 | 18.9 | - |
| dunya | iso_ground | `iso_ground_dirt_set_cand8` | %100 | 18.4 | - |
| dunya | iso_ground | `iso_ground_grass_autumn_a` | %67 | 15.2 | - |
| dunya | iso_item | `iso_item_threshing_floor_3x3_se` | %92 | 19.8 | A |
| dunya | iso_wall | `iso_wall_item_torch_wall_sw_ne` | %79 | 10.7 | A |

## Önerilen işlem (kullanıcı onayına)

1. **Kabul edilmiş ikonlar (31 dosya, `icon_` ve `ui_`, A işaretli):** yeniden üretim adayı. Tahmini 1 gen / ikon, toplam ≈ 31 gen. Aynı tarif; stil referansı olarak arayüz ailesinin kabul edilmiş ikonları. İş dosyası onaydan sonra açılır.
2. **Bitki ve eşya:** önce `style.py fit --family <aile>` (0 gen; yalnızca doygunluğu indirir), yan yana sayfayla karar. Kurtulmazsa yeniden üretim.
3. **Işık ve efekt** (`iso_fx_light_ring`, `iso_wall_item_torch_wall_sw_ne`): ışık kaynağı kuralı gereği `check --light` ile denetlenir; işlem gerekmez.
4. **Kabul edilmemiş adaylar** (kar ve sonbahar karoları, karakter adayı): kabul kararı verilirken denetlenir. Kar ve sonbahar karoları kabul edilirse paletin bu tonları taşıması için `style.py palette --set dunya --yeniden "<karar>"` ile yeniden üretilmesi önerilir.
