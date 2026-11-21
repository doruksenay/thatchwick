unity-import: pending

# 743 İlişki, kavga ve muhabbet ikonları: sonuç (durum: review)

Yan yana sayfası: `art/results/743-yanyana.png` (arayuz ailesinin referansı yok; 0 referans + 7 yeni). Elenen üç ikon: `art/results/743-red/`.

## Araç ve ayarlar
- `create_image_pixen` 32x32, `no_background=true`, `outline: single color outline`, `detail: low detail` (212/497/593 ile aynı, stil eşlemesi yok); ikon başına 1 çağrı, **10 gen** (cost_est 30, tavan 45). Hız sınırı hatası olmadı; ilk indirmede 3 ikon hazır değildi (423), bekleyip tekrar indirildi, yeni üretim yok.
- Ham çıktı: alfa ikili, 12 renge (Pillow mediancut, dither yok). 16x16: en yakın komşu indirme, 12 renk.
- Yol: iş dosyasındaki `Assets/Art/Ui/` yok; 593 ile aynı klasör `Assets/Art/Iso/_aday/` ve `ui_icon_*` adlandırması kullanıldı (`social_*` → `ui_icon_social_*`, `mood_*` → `ui_icon_moodlet_*`).
- `get_balance` sonrası: 9227 (diğer şeritlerin harcaması dahil). Masaüstü kopyası: yerel.

## Teslim edilenler (ikon, pivot x 8 / 16, y 0 alttan)
| Dosya | Boyut | Stil denetimi | Okunurluk |
|---|---|---|---|
| ui_icon_moodlet_beard_rash(.png/_32) | 16, 32 | GEÇTİ | sakallı yüz, kızarık, net |
| ui_icon_moodlet_scared_fight(.png/_32) | 16, 32 | 16 UYARI, 32 GEÇTİ | kulaklarını kapatmış büyük gözlü figür; "çocuk" ayrımı zayıf, tam kadraj |
| ui_icon_social_fight | 16, 32 | GEÇTİ | dövüşen figür, toz/kıvılcım var; yıldız zayıf |
| ui_icon_social_make_up | 16, 32 | GEÇTİ | el sıkışma, net |
| ui_icon_social_sulk | 16, 32 | GEÇTİ | iki figür yan yana, sırt sırta tam okunmuyor |
| ui_icon_moodlet_bruised | 16, 32 | UYARI (palete uzak) | mor göz benzeri, belirsiz |
| ui_icon_social_jab | 16, 32 | 16 GEÇTİ, 32 UYARI | dikenli balon yerine dikenli yüz-küre; zayıf |

## Teslim edilmeyenler (`art/results/743-red/`)
- social_group_chat: stil denetimi KALDI (%82 palete yakın, palet dışı renkler); ayrıca şekil dağınık.
- social_argue: konuşma balonları yerine kafatası benzeri yüzler ve makine; yanlış nesne.
- social_gossip: kulağa fısıldayan ağız yerine tek figür; yanlış nesne.

## Doğrulama
Yarı saydam piksel 0; renk ≤ 12; `validate_iso.py` ikonlara uygulanmaz.

## Sorunlar
- 3 ikon yeniden üretim ister (argue, gossip, group_chat); tek deneme kuralı gereği tekrar denenmedi. Öneri: yeni iş, kılavuz görselli ya da `create_image_pro` ile.
- Kullanıcı bruised, jab ve sulk'ı beğenmeyebilir.
