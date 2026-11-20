unity-import: pending

# 720 Yemek ikonları 2: sonuç (durum: review, 9/13 teslim)

Yan yana sayfası: `art/results/720-yanyana.png` (arayuz ailesinde kabul referansı yok, yalnızca yeni görseller). X4 parşömen/koyu önizleme: `art/results/720-ikonlar-x4.png` (13 ikonun hepsi, alfabetik sırayla).

## Araç ve ayarlar
- `create_image_pixen`, 32x32, `no_background=true`, `outline: single color outline`, `detail: low detail`; 316 ile aynı yöntem (ikon başına bir çağrı). Her açıklamaya `, simple still-life object, no face, no eyes, no creature, no person` eklendi.
- 13 çağrı x 1 gen = **13 gen** (`cost_est` 13, `cost_max` 26). Hız sınırı (10/10, paylaşılan hesap) 8 çağrıda alındı, işler boşalınca yeniden gönderildi (tarifin tek istisnası; reddedilen çağrılar harcama yapmadı).
- Renk indirgeme: ham çıktılar 25-91 renk; Pillow median-cut ile 16 renge indirildi (dither yok, alfa 0/255). Yeniden boyama yok.
- `get_balance` sonrası: 1562,7 (paralel şeritlerin harcaması dahil).
- Masaüstü kopyası: yerel.

## Dosyalar (`Assets/Art/Iso/_aday/`, 32x32 RGBA, pivot x 16, pivot y 0 alttan; ikon, pivot önemsiz)
Teslim edilen 9: icon_item_{soup_cabbage, soup_beet, pork_kavurma, liver_fry, goat_stew, duck_plum, boar_stew, meatballs, sucuk_egg}.png

Teslim edilmeyen 4 (`art/results/720-red/`):
- `soup_crayfish`: stil denetimi KALDI (palet dışı renkler %78) ve kasede yüz çizilmiş.
- `soup_tripe`: kasedeki kremada yüz/göz var (tarife aykırı).
- `beef_yahni`: yumru üzerinde yüz/göz var, kase okunmuyor.
- `pigeon_stuffed`: stil denetimi KALDI (palet dışı renkler %72).

## Doğrulama
| Ölçüt | Sonuç |
|---|---|
| Boyut | 32x32 (13/13) |
| Yarı saydam piksel | 0 (13/13) |
| Renk sayısı | ≤16 (indirgeme sonrası) |
| `style.py check --family arayuz` | teslim edilen 9: 8 GEÇTİ, 1 UYARI (sucuk_egg, palete uzak %84); KALDI 2 (pigeon_stuffed, soup_crayfish; ham hâlleri UYARI idi, indirgeme paleti kaydırdı) |
| Yüz/figür gözü | 3 ikonda yüz var (yahni, tripe, crayfish), teslim edilmedi |
| `validate_iso.py`, döşeme testi | uygulanmadı (izometrik zemin değil, UI ikonu) |

## Sorunlar / öneri
1. Tarifteki "no face" eki pixen'de 3 ikonu yine de yüzlü üretmedi engelleyemedi. 4 ikon için `create_map_object` 32x32 (401/417 yöntemi) ile yeniden üretim önerilir (en çok ~4-8 gen); tek deneme kuralı gereği yapılmadı.
2. Ham hâller `art/tmp720/` altında (commit edilmedi).
