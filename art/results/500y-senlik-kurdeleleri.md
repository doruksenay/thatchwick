unity-import: pending

# 500y Şenlik yarışma kurdeleleri (rapor): 0/2 teslim, 2 KALDI, failed

Yan yana sayfası: [500y-yanyana.png](500y-yanyana.png). Teslim edilmeyenler: `art/results/500y-red/`. Masaüstü kopyası: yerel.

## Araç ve ayarlar
`create_map_object` stil eşleme, özel maske (403 yöntemi; betik geçici, commit edilmedi). Kurdele: 32x32 tuval, taban elması 20x10, `high top-down`. Rozet: 40x40, "/" paralelkenar (184,160,122), `side`. İkisi `low detail`, `basic shading`, `single color outline`. 2 çağrı, 1 gen/çağrı = 2 generation (cost_max 6). Rozet çağrısı ilk denemede hız sınırı verdi; ~1 dk sonra bir kez yeniden denendi (kuralın istisnası). Tek deneme.
get_balance sonrası: 2358,90 (paralel şeritler de harcıyor).

## Sonuç
| # | Eşya | Durum | Stil denetimi |
|---|---|---|---|
| 1 | show_ribbon | TESLİM EDİLMEDİ | KALDI: palete yakın %77, ort. 13.9, x1.01 (doygunluk sorunu değil, `fit` uygulanmadı). Şekil: krem madalyon + çivit kuyruk; yassı kurdeleden çok madalyon gibi okunuyor ("madalya yok" kuralına aykırı) |
| 2 | show_rosette | TESLİM EDİLMEDİ | KALDI: %28, ort. 25.4, x1.88 doygun. `fit` (x0.61) sonrası %72, hâlâ KALDI (palet dışı). Şekil güzel: çivit/altın pilili rozet; tahta pano ve üç kuyruk yok |

`validate_iso.py` ikisi için geçti (13 ve 17 renk, yarı saydam 0, maske dışı 0).

## Dosyalar
Teslim yok (`Assets/Art/Iso/_aday/` değişmedi).

## Sorunlar ve öneriler
- Rozet `fit` sonrası %72 ile eşiğin altında; kullanıcı göz kararıyla kabul etmek isterse `500y-red/iso_wall_item_show_rosette_sw_ne_fit.png` hazır (pano ve kuyruk eksik).
- Yeniden deneme yeni iş gerektirir. Öneri: kurdele için "flat ribbon, no medal" ve soluk çivit; rozet için "wooden board" vurgusu.
