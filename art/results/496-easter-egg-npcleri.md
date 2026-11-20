unity-import: pending

# 496 Easter egg NPC'leri: Johan Wick ve atsız şövalye (84 px, 8 yön durma)

Durum: review. 16/16 dosya teslim edildi (`Assets/Art/Iso/_aday/anim/`). Yan yana: [496-yanyana.png](496-yanyana.png) (soldan: eski köylü, çiftçi 56 px, man_r84, woman_r84, Johan SE, şövalye SE, Johan S, şövalye S).

## Araç ve ayarlar
- `create_character` v3, humanoid, `high top-down`, siyah tek renk dış çizgi, high detail, size 84, 8 yön (415 ve 012b ile aynı). Tarifteki oran metni, karakter tarifi ve 012b ortak eki aynen. Araç stil/kimlik referansı almadığı için `man_r84` referansı kullanılmadı. İki çağrı paralel gönderildi.
  - johan_wick: 8d5fc676-e8ba-4988-ba86-d18df6ab56c8 (2 gen)
  - horseless_knight: f3a16e27-2afe-4bee-9157-9d33eb2be75b (2 gen)
- `reduce_colors` 32 renk, dithering none, karakter başına 8 yön tek çağrıda (ortak palet): 2 x 0,1 gen.
- Toplam harcanan: 4,2 gen. `get_balance` sonrası: 2354,7 (paralel şeritlerin harcamaları dahil).
- Masaüstü kopyası: yerel.

## Dosyalar (`Assets/Art/Iso/_aday/anim/`, 84x84, pivot x=42, y=2 alttan hedefi)
`iso_char_{johan_wick,horseless_knight}_r84_idle_{e,ne,n,nw,w,sw,s,se}.png`

| Yön | Johan opak (g x y) | Johan alt boşluk | Şövalye opak (g x y) | Şövalye alt boşluk |
|---|---|---|---|---|
| e | 43x79 | 2 | 44x78 | 3 |
| ne | 49x80 | 3 | 51x77 | 4 |
| n | 51x78 | 4 | 50x77 | 4 |
| nw | 49x80 | 3 | 51x77 | 4 |
| w | 44x79 | 2 | 41x78 | 3 |
| sw | 51x81 | 1 | 54x79 | 2 |
| s | 54x80 | 2 | 54x78 | 3 |
| se | 51x82 | 1 | 53x79 | 2 |

Tüm dosyalar: yarı saydam piksel 0, renk 28-32 (≤32).

## Doğrulama
- `validate_iso` (`--kind object --max-colors 32`): 16/16 geçti.
- `style.py check --family karakter`: 16 GEÇTİ, UYARI/KALDI yok. Aile kilidi kurulmadı; palet uyumu yalnızca uyarı düzeyinde. Doygunluk/aile: Johan x0,99-1,17, şövalye x0,70-0,84.
- Boy: opak 77-82 px; man_r84 SE ile aynı bantta (78). Alt boşluk 1-4 px (tarifteki 2 px için içerik kaydırılmadı; 415 ile aynı not).
- Siluet: ikisi de köylülerden ayırt edilir (Johan geniş pelerin ve sırt çantası; şövalye miğfer, sürcü ve öne uzatılmış yumruklar).
- Özgünlük: siyah takım, uzun koyu saç, sakal, modern giysi ve silah yok. Johan kısa gri saçlı, tıraşlı, kül rengi yün pelerinli. Geçti.
- Dönem: pelerin, tunik, zincir zırh, kazan miğfer: ortaçağ. Silah yok.

## Sorunlar
- **Johan Wick'te kukuleta takılı çıktı** (tarif: "hood down"); gri saç kukuletanın altından görünüyor. Sırtında tarifte olmayan büyük bir çanta/sırt yükü var. Silüet ve özgünlük şartı bozulmadı, bu yüzden teslim edildi; kullanıcı yan yana sayfasına bakıp karar verir. Ret olursa yeniden üretim ayrı iş (kukuleta ve çanta için tarif sıkılaştırılır).
- Şövalyenin yüzünde kısa sakal var (tarif belirtmiyor); sorun sayılmadı.
- Alt boşluk yönlere göre 1-4 px; Unity'de pivot ayak tabanına göre ayarlanmalı.
- Yürüme ve özel hareket kareleri bu işte yok (iş dosyası notu).
- `reduce_colors` çıktısını indirirken `save_png.py --index` bu uç noktada çalışmadı; `.../download?index=N` URL'siyle her kare ayrı alındı.
