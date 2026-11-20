unity-import: pending

# 418p2 Tuzlu et ikonu v3 (durum: review)

Yan yana sayfası: [418p2-yanyana.png](418p2-yanyana.png) (sıra: eski | reddedilen v2 | v3 | meat_v2). Masaüstü kopyası: yerel.

## Araç ve ayarlar
- 1 x `create_map_object` basic 32x32, `high top-down`, `low detail`, `basic shading`, `single color outline` (418p yöntemi); açıklama: koyu kırmızı-kahve kuru et, düzensiz kesik, krem yağ şeridi, çok sayıda iri beyaz-gri tuz kristali. **1 gen** (cost_est 1).
- Ham çıktı 20 renkliydi; Pillow median-cut ile 16 renge indirildi, dither yok, alfa 0/255. Ham: `art/tmp418p2/` (commit edilmez).
- `get_balance` (iş öncesi): 848,6; iş sonrası okunmadı, paylaşılan hesap (~847,6 beklenir).

## Dosya
`Assets/Art/Iso/_aday/icon_item_salted_meat_v3.png`: 32x32 RGBA, pivot x 16, pivot y 0 (alttan).

## Doğrulama
| İkon | Palete yakın | Ort. uzaklık | Sonuç |
|---|---|---|---|
| salted_meat_v3 | %85 | 10,8 | UYARI (palete uzak) |

## Sorunlar
1. Tuz taneleri v2'den belirgin ama hâlâ az (yaklaşık 4-5 açık nokta, brief 6-10 istiyor); biçim silindirik/sosis gibi okunabilir, ucunda krem yağ kesiti var. Çiğ etten koyuluğuyla ayrışıyor. Gözle karar kullanıcıda; olmazsa yeni deneme (`max_attempts: 2`, 1 hak kaldı).
2. Palet uyarısı: UYARI, KALDI değil.
