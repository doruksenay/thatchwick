unity-import: pending

# 820b İç Sesler nakış yaması pilotu: KISMEN TESLİM (review)

Araç: `create_image_pixen` 32x32 (yama, ikon, kurdele 64x32; ilk kurdele `create_image_pixflux`), saydam zemin, low detail, tek renk kontur. Toplam 14 gen (13 + 1 yeniden deneme turu: tek/altın yama, kurdele, sohbet, ilham ikonu). Kalan (`get_balance`): 9647 (diğer şeritler de harcıyor). Çıktılar yerelde Pillow ile içerik kutusuna kırpıldı, yamalar en yakın komşuyla 24x24'e, ikonlar 16x16'ya indirildi, medyan kesimle renk azaltıldı (PixelLab `reduce_colors` kullanılmadı). Masaüstü kopyası: yerel.

Önizleme: [820b-yama-pilotu-x6.png](820b-yama-pilotu-x6.png) (krem ve koyu zemin, 6 kat; üst: tek/çift/altın yama, kurdele, ikonlar; altta sekiz hâl rengi yama + ikon). Yan yana: [820b-yanyana.png](820b-yanyana.png).

## Teslim (Assets/Art/Iso/_aday/)
| Dosya | Boyut | Renk | Pivot (x, y alttan) | Stil | Not |
|---|---|---|---|---|---|
| ui_patch_gold | 24x24 | 6 | 12, 0 | UYARI palete uzak | düz kumaş, altın dikiş ve köşe düğümü okunuyor |
| ui_patch_ribbon_v2 | 60x14 | 6 | 30, 0 | UYARI palete uzak | çatal uçlu düz şerit, parşömen değil; dikişler renk azaltmada kayboldu |
| ui_icon_voice_good_meal (+_32) | 16x16 | 7 | 8, 0 | GEÇTİ | |
| ui_icon_voice_nice_chat (+_32) | 16x16 | 7 | 8, 0 | GEÇTİ | balonlarda yüz çıktı (istenmedi) |
| ui_icon_voice_grief (+_32) | 16x16 | 8 | 8, 0 | GEÇTİ | 32'lik hâl okunuyor (solgun, boynu bükük, düşmüş yaprak); 16'lıkta bulanık, zayıf |
| ui_icon_voice_inspired (+_32) | 16x16 | 7 | 8, 0 | UYARI (--light, %83) | mum + kap, ama alev kapsamı yüksek |

Yarı saydam piksel 0. Palet değişimi testi yapıldı (önizlemenin alt iki satırı): gri-yeşil-sarı-turkuaz-mor-turuncu-mavi-kırmızı hâller çalışıyor; keten tonlarını boyarken kontur ve altın korunuyor.

## Teslim edilmedi (`art/820b-red/`)
- ui_patch_single: gürültülü doku, dikiş okunmuyor (ilk deneme minder gibi çıktı, ikinci düz ama benekli).
- ui_patch_double: düz tek ton; krem dikişler 24x24'e inip 6 renge düşünce kayboldu. Ham 32x32 çift dikişli çıktı iyiydi (art/tmp820b ham, commit edilmedi).
- Sonuç: üç dikiş ayırt edilemiyor; 24x24'te dikiş için sert renk sınırı yok. Ölçüt "üç dikiş birbirinden ayırt ediliyor mu" tutmadı.

## Öneri (## Sorular)
Dikişleri renk azaltmadan önce ayrı bir palet slotuna kilitlemek (ya da yamayı 32x32 teslim edip dikişi koda taşımak) gerekir. Tek/çift için bir tur daha tarif ya da 32x32 ölçü onayı gerekli.
