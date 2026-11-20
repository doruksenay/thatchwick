unity-import: pending

# 820 İç Sesler nakış yaması pilotu: BAŞARISIZ (kısmen teslim)

Araç: `create_image_pixflux`, 9 çağrı x 1 gen = 9 gen (3'erli paralel), 32x32 tuval (kurdele 64x32), no_background, flat shading, high top-down (yama/dikiş). Tarifteki açıklamalar aynen. Palet zorlaması verilmedi (pixflux yalnızca renk görseli alıyor); sonuç `style.py check` ile denetlendi.
`get_balance` sonrası kalan: 9808 (çalışma başında; diğer şeritler de harcıyor).

Yan yana sayfası: [820-yanyana.png](820-yanyana.png) (yalnızca kurdele; referanssız). Reddedilenler `art/820-red/`.

## Stil denetimi (arayuz)
Hepsi GEÇTİ (palete yakın %90-100, ort. uzaklık 6.5-9.7; grief %90). Ancak iş ölçütleri tutmadı:

| Dosya | Boyut (kırpık) | Renk | Sonuç | Neden |
|---|---|---|---|---|
| ui_patch_base | 28x28 | 35 | RED | keten tonu >4, 24 değil |
| ui_patch_stitch_single | 32x32 | 20 | RED | saydam değil (%100 dolu) |
| ui_patch_stitch_double | 30x30 | 17 | RED | saydam değil (%88) |
| ui_patch_stitch_gold | 30x31 | 51 | RED | saydam değil, koyu çerçeve+desen |
| ui_patch_ribbon | 60x25 | 22 | TESLİM | pivot x 30, y 0 (alttan 12) |
| ui_icon_voice_good_meal | 30x28 | 31 | RED | 16x16 değil, renk >8 |
| ui_icon_voice_nice_chat | 22x18 | 16 | RED | renk >8 |
| ui_icon_voice_grief | 20x28 | 8 | RED | solmuş değil, dik gül |
| ui_icon_voice_inspired | 24x27 | 15 | RED | renk >8 |

`validate_iso.py` izometrik zemin/yapı içindir, UI parçasına uygulanmadı. Döşeme testi: zemin değil, uygulanmadı.

## Sorunlar / öneri
Bkz. iş dosyası `## Sorular`. Masaüstü kopyası: yerel.
