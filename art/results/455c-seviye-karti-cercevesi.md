unity-import: pending

# 455c Seviye kartı çerçevesi ve beceri çubuğu

- Araç: `create_image_pixflux`, tarifteki açıklamalar, `flat shading`, `lineless`, `view: high top-down`. Kart `no_background=true`.
- Çubuk 64x8 istendi, ancak araç alan tabanı 32x32 (1024 px) olduğu için 64x16 istendi (64x8 = 512 px reddedilir).
- Kart: 1 çağrı, 1 generation. Çubuk: ilk çağrı "boş görüntü" 502 verdi (ücretlenmedi). İş dosyası `max_attempts: 2` olduğundan ikinci çağrı `no_background=false` ile yapıldı, 1 generation. Toplam 2.
- `get_balance`: işe başlarken 3034,9 (paralel şeritlerle ortak). Sonrasını okumadım; fark ~2.
- Yan yana sayfası: [455c-yanyana.png](455c-yanyana.png). Arayüz ailesinin kabul referansı yok, yalnız yeni dosya var. Kart 240x100 ve çubuk 110x7 ölçekli önizlemesi yapılmadı.
- Masaüstü kopyası: yerel.

## Dosyalar
| Dosya | Boyut | Pivot (x, y alttan) | Durum |
|---|---|---|---|
| `Assets/Art/Iso/_aday/ui_skill_card_frame.png` | 64x64 | 32, 0 (UI, 9-dilim; pivot önemsiz) | teslim, UYARI |
| ui_skill_bar_frame | (64x16) | - | **teslim edilmedi** |

## Doğrulama
| Dosya | Stil denetimi | Not |
|---|---|---|
| ui_skill_card_frame | UYARI (kilit kurulmadı, "palete uzak"; palete yakın %100, ort. uzaklık 10,2) | 16 renk, yarı saydam yok; çerçeve 2 px kenar boşluklu (bbox 2,2-62,62) |

Kart merkez alanı (14-50 aralığı) 5 renk içeriyor, tam düz değil. Çerçeve ince bordo dikiş ve köşe düğümleriyle okunuyor. Kenar 12 px yerine yaklaşık 3-4 px: 9-dilimde kenar değerini kod tarafı 4 px alırsa esneme sorunsuz olur. Gözle bakılmadı: köşe düğümleri küçük.

## Sorunlar
- **ui_skill_bar_frame teslim edilmedi.** Çıktı tam opak 64x16; yalnız üst ve alt kenar dikişi var (sol/sağ uç başlığı yok), 3-dilim için kullanılamaz. Maskeyle 64x8'e kırpmak içeriği değiştirmek olurdu. İkinci deneme hakkı da kullanıldı. Öneri: ayrı iş olarak uç başlıklı çubuk, ya da betikle `pixellab: false` çizim.
- Kart merkezinde hafif doku var; esnetmede tekrar etmez ama "düz" ölçütüne tam uymaz.
