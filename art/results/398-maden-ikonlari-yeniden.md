unity-import: pending

# 398 Ürün ikonları yeniden (6): sonuç (durum: review)

Yan yana sayfası: `art/results/398-yanyana.png` (arayuz ailesinde kabul referansı yok; yalnızca 6 yeni ikon. Kabul edilmiş 17 ikon bu çalışmada yan yana konmadı: işçi ortamında `_aday` altından ayrı sayfa için `367-yanyana.png`'e bakın).

## Araç ve ayarlar
- `create_image_pixen`, 32x32, `no_background=true`, `outline: single color outline`, `detail: low detail`; 367 ile aynı yöntem. Fark: isteme "no face" yerine yalnızca cansız nesne tarifi ("inanimate", "game inventory item icon"), 367 önerisine uygun.
- 6 çağrı x 1 gen = **6 gen** (`cost_est` 6, `cost_max` 10). Hız sınırı hatası yok; çağrılar paralel gönderildi.
- `get_balance` sonrası: 4378,9 (paralel şeritlerin harcaması dahil).
- Renk indirgeme: Pillow median-cut 16 renk, dither yok, alfa 0/255 (yeniden boyama yok). Ham hâller `art/tmp398/` altında (commit edilmez).
- **Çıktı yolu:** iş dosyası `Assets/Art/Icons/` yazıyor; işçi yalnızca `Assets/Art/Iso/` altına yazabilir, bu yüzden 316/367 gibi `Assets/Art/Iso/_aday/` altına konuldu.
- Masaüstü kopyası: yerel.

## Dosyalar (`Assets/Art/Iso/_aday/`, 32x32 RGBA, pivot x 16, pivot y 0 alttan; ikon, pivot önemsiz)
icon_flint.png, icon_gypsum.png, icon_limestone.png, icon_sand.png, icon_slag.png, icon_torch.png

## Doğrulama
| Ölçüt | Sonuç |
|---|---|
| Boyut | 32x32 (6/6) |
| Yarı saydam piksel | 0 |
| Renk sayısı | ≤16 |
| Yüz / göz | Hiçbirinde yok (367 sorunu giderildi) |
| `style.py check --family arayuz` | KALDI yok. GEÇTİ: flint. UYARI (kilit kurulmadı / palete uzak): gypsum (%16 palete yakın), limestone (palete uzak), sand, slag, torch |
| `validate_iso.py` | uygulanmadı (UI ikonu) |

## Göz kontrolü / sorunlar (kullanıcı kararı için)
- flint: koyu gri kırık yumru, tanınıyor.
- gypsum: mavimsi-beyaz sivri kristal; tebeşirimsi değil, kristal okunuyor ama renk tarife göre soğuk.
- limestone: krem yuvarlak blok, **katman çizgileri yok**; zayıf, tarife uyuyor ama ayırt edici değil.
- sand: açık bej yığın, tanınıyor.
- slag: koyu delikli yumru ama **turuncu-kırmızı parlama** var (lav gibi); tarif "siyah-kahve camsı" idi. Zayıf.
- torch: sopa, bez ve alev, tanınıyor.
- Öneri: limestone ve slag zayıfsa tek tek `create_image_pro` ile yeniden iş yazılabilir.
- Tek deneme kuralı gereği yeniden üretim yapılmadı.
