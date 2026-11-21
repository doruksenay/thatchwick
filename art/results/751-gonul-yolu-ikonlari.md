unity-import: pending

# 751 Gönül yolu ve aile ikonları: sonuç (durum: review)

Yan yana sayfası: `art/results/751-yanyana.png` (arayuz ailesinin kabul referansı yok; yalnızca yeni görseller).

## Araç ve ayarlar
- `create_image_pixen` 32x32, `no_background=true`, `outline: single color outline`, `detail: low detail`. Gerekçe: 315'te aynı ayarla ikon seti üretildi (pixen küçük sprite'ta temiz); tutarlılık için aynı boyut ve ayar.
- 10 çağrı x 1 gen = **10 gen** (en çok 3 eşzamanlı; hız sınırı yok, yeniden deneme yok). `get_balance` sonrası: 9210 (diğer şeritlerin harcaması dahil).
- 55b ve 706 çıktısı henüz yok (706 pending), `Assets/Art/Ui/` yok; boyut ve çerçeve için 315 setinin 32x32 biçimi alındı. Çıktı yolu: bu işçinin yazma izni `Assets/Art/Iso/` ile sınırlı olduğundan `Assets/Art/Iso/_aday/ui_icon_<ad>.png` (iş dosyasındaki `Assets/Art/Ui/<ad>.png` yolu değil); taşıma kullanıcıda/kod oturumunda.
- Masaüstü kopyası: yerel.

## Dosyalar (hepsi 32x32, pivot x 16, pivot y 0 alttan; ikon, pivot önemsiz)
| Dosya | Stil denetimi | Teslim |
|---|---|---|
| ui_icon_event_asking | GEÇTİ | evet |
| ui_icon_event_henna_night | GEÇTİ | evet |
| ui_icon_mood_forgave | GEÇTİ | evet |
| ui_icon_mood_salty_coffee | GEÇTİ | evet |
| ui_icon_event_wedding | UYARI (palete uzak) | evet |
| ui_icon_mood_regret | UYARI (palete uzak) | evet |
| ui_icon_status_sweetheart | UYARI (palete uzak) | evet |
| event_invitation | KALDI (palet dışı renkler, %71) | hayır, `art/results/751-red/` |
| status_engaged | KALDI (palet dışı renkler, %75) | hayır, `art/results/751-red/` |
| status_ex_spouse | KALDI (palet dışı renkler, %68) | hayır, `art/results/751-red/` |

## Sorunlar
- 3 ikon palet uyumunda KALDI; şekil değil renk sorunu olduğu için `fit` (yalnızca doygunluk) çözmez. Yeniden üretim gerekirse ayrı iş (tek deneme kuralı).
- Konu okunurluğu (ör. tuzlu kahvedeki yüz, kırık yüzük) göz kontrolü kullanıcıda: yan yana sayfasına bak.
