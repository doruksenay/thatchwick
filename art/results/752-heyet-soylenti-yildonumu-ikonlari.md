unity-import: pending

# 752 İhtiyar heyeti, söylenti, bilgi ve yıldönümü ikonları

- Araç: `create_map_object` 32x32, `view: side`, `single color outline`, `basic shading`, `medium detail` (455a yöntemi), ortak ek ve palet metniyle. 10 çağrı, 10 generation, tek deneme. Hız sınırı hatası yok.
- Son işlem: yarı saydam 0 (alfa eşiği 128), 16 renkten fazlaysa 16 renge indirme (medyan kesim, titreşimsiz).
- `get_balance` sonrası: 9220 (paralel şeritlerle ortak).
- Yan yana sayfası: [752-yanyana.png](752-yanyana.png) (arayuz ailesinin kabul referansı yok, yalnız yeni dosyalar).
- Masaüstü kopyası: yerel.

## Yol sapması
İş dosyası `Assets/Art/Ui/` yazıyor; bu yol işçi yetkisi dışında ve 55b işi kuyrukta yok. 455a'daki gibi `Assets/Art/Iso/_aday/` altına `ui_icon_` önekiyle teslim edildi. Kabulden sonra kod oturumu taşır.

## Dosyalar (hepsi 32x32; pivot alttan ortalı, x=16, y=2)
| Dosya (`Assets/Art/Iso/_aday/`) | Asıl çıktı adı | Not |
|---|---|---|
| ui_icon_event_council.png | event_council | çınar ve üç figür; okunur |
| ui_icon_status_council_member.png | status_council_member | asa ve çelenk; sade, zayıf |
| ui_icon_event_petition.png | event_petition | rulo ve mühür; iyi |
| ui_icon_event_imece.png | event_imece | kürek ve kazma var, **el ele eller yok** |
| ui_icon_social_rumor.png | social_rumor | duman balonu ve karalama; iyi |
| ui_icon_social_deny_rumor.png | social_deny_rumor | çizili balon; iyi (UYARI: palete uzak) |
| ui_icon_info_secret.png | info_secret | dudağa parmak yerine kalkık el/parmak, okunur |
| ui_icon_event_anniversary.png | event_anniversary | yüzükler, çiçek; takvim yaprağı belirsiz |
| ui_icon_mood_forgot_anniversary.png | mood_forgot_anniversary | solmuş çiçek ve yaprak; iyi |

## Teslim edilmeyen
- **info_known** (açık göz ve kâğıt): çıktı bir nokta ve lekeden ibaret, tanınmıyor. `art/results/752-red/` altında (ham hâli de). Yeniden üretim için ayrı iş gerekir (ör. `create_image_pixflux` 32x32 `no_background=true`).

## Doğrulama
`style.py check --family arayuz`: 8 GEÇTİ, 1 UYARI (social_deny_rumor, %89 palet, palete uzak), 1 KALDI yok (info_known GEÇTİ ama şekil nedeniyle elendi). Renk sayıları 5-16, yarı saydam 0. İkonlar karo değil: `validate_iso.py` ve döşeme testi uygulanmadı.

## Sorunlar
- info_known tanınmıyor, imece'de eller eksik; tek deneme kuralı gereği yeniden üretilmedi.
- Ortak "kırmızı" çizgi (deny_rumor) paletin dışına kayıyor.
