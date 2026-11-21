unity-import: pending

# 782 Yaban hayvanları ölçek seti: teslim yok (tek deneme tutmadı)

Sonuç: `status: failed`. Hiçbir PNG `Assets/Art/Iso/` altına konmadı. Masaüstü kopyası: yerel.
Yan yana sayfası (referanslar + tilki SE/S, kurt SE/S, martı ham): `art/results/782-yanyana.png`. Adaylar `art/782-red/` altında.

## Araç ve ayarlar
- 780 yöntemi: `create_map_object` stil eşleme, özel maske, `high top-down`, `single color outline`, `medium detail`, `basic shading`; kılavuz = eski SE görselinin hedef kutuya küçültülmüş hâli gri zeminde. Tilki 32 tuval / maske 26x22, kurt 40 tuval / maske 30x32, martı 32 tuval / maske 16x13 (alt kenardan 2 px yukarı). Açıklamalar 780'deki ortak ek ve 375 ekiyle.
- Döndürme: `create_character` v3, quadruped `dog`, `high top-down`, referans = tek karenin indirme URL'si (tuval küçültülmedi). Martı döndürülmedi.
- Kırpma: tuval yalnızca saydam piksel eklenerek/kesilerek ayarlandı (içerik boyanmadı). Tilki tuvali 22x25, kurt 32x30; pivot x tuval ortası, SE tabanı alttan 2. satır.
- Hız sınırı hatası olmadı.

## Karakter kimlikleri
- Tilki: `46e7ce2c-3055-41c7-9918-f8d61425c72a` (map object `f2d23b09-db1d-46f1-a119-8cce45a624a2`)
- Kurt: `1b1e83c8-d1ae-4b44-a054-b1a4851a8348` (map object `29662b07-ef60-491f-adcc-4a200fdd0615`)
- Martı: yalnızca tek kare, map object `54f6e222-6cde-4f21-b26a-87325fcd6827`

## Tek kare kontrolü
| Aday | Boy x en | Güney yüzü | Karar |
|---|---|---|---|
| tilki | 19 x 15 | önden ama oturuyor (ayakta değil) | döndürüldü |
| kurt | 27 x 25 | önden 3/4, ayakta | döndürüldü |
| martı | 9 x 13 | yandan (sağa bakıyor), tuvalde 6 px havada | döndürülmedi |

## Doğrulama
| Gövde | Boy (scale_check) | Stil denetimi (fit sonrası, doygunluk x0,62-0,75) | Karar |
|---|---|---|---|
| tilki | 8/8 TAMAM (19-23, hedef 19-26) | e, n, ne, nw, w GEÇTİ; s, se, sw KALDI (palet dışı renkler, %76-79) | teslim yok |
| kurt | 8/8 KALDI (26-27, hedef 28-36) | denetlenmedi | teslim yok |
| martı | tek kare, ölçülmedi | - | teslim yok |

Ham tilki doygunluğu x1,53-1,85 (KALDI); `fit` ile x1,16-1,18'e indirildi. Tilki SE `validate_iso.py --kind object`: geçti (opak 186, yarı saydam 0, renk 26, alt boşluk 2).

## Sorunlar ve öneri
- Kurt: v3 hayvanı 32'lik maskeye rağmen 27 px çizdi (hedef 28-36, 1-2 px eksik); yeniden örnekleme yapılmaz.
- Tilki: oturur pozda; S/SE/SW turuncu tonları palet dışı. Kullanıcı 5 geçen yönü ve 3 kalanı görsel olarak değerlendirebilir (`art/782-red/fox/`).
- Martı: yandan ve havada; 780'deki köpekle aynı sorun. Öneri: 782b işinde simetrik ön görünüşlü kılavuz ve "head facing the viewer, symmetrical" vurgusu; kurt için maske yüksekliği 34-36.

## Generation
| Çağrı | Gen |
|---|---|
| create_map_object x3 | 3 |
| create_character v3 ref x2 | 2 |
| Toplam | 5 (cost_max 14) |

`get_balance` sonrası: 9483 (paralel şeritlerin harcaması dahil).
