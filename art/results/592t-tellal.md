unity-import: pending

# 592t Tellal (84 px, 8 yön durma)

Durum: review. 8/8 dosya teslim edildi (`Assets/Art/Iso/_aday/anim/iso_char_crier_r84_idle_{e,ne,n,nw,w,sw,s,se}.png`). Yan yana: [592t-yanyana.png](592t-yanyana.png) (eski köylü, çiftçi 56 px, man_r84, woman_r84, Johan, şövalye, tellal SE ve S).

## Araç ve ayarlar
- `create_character` v3, humanoid, `high top-down`, siyah tek renk dış çizgi, high detail, size 84, 8 yön (012b/496 yöntemi); tarifteki oran metni, karakter tarifi ve 012b ortak eki aynen. Araç referans almadığı için kimlik referansı kullanılmadı. Karakter id: 4330bc31-f9c1-4ba9-9ea1-04a12f738dd3 (2 gen).
- `reduce_colors` 32 renk, dithering none, 8 yön tek çağrı: 0,1 gen.
- Toplam harcanan: 2,1 gen. `get_balance` sonrası: 1171,6 (paralel şeritlerin harcamaları dahil).
- Masaüstü kopyası: yerel.

## Dosyalar (84x84, pivot x=42, y=2 alttan hedefi)
| Yön | Opak (g x y) | Alt boşluk | Renk | Yarı saydam |
|---|---|---|---|---|
| e | 43x78 | 3 | 32 | 0 |
| ne | 52x77 | 4 | 32 | 0 |
| n | 60x74 | 6 | 30 | 0 |
| nw | 52x77 | 4 | 31 | 0 |
| w | 42x79 | 3 | 32 | 0 |
| sw | 53x80 | 2 | 32 | 0 |
| s | 58x78 | 3 | 32 | 0 |
| se | 51x79 | 2 | 32 | 0 |

## Doğrulama
- `validate_iso` (`--kind object --max-colors 32`): 8/8 geçti.
- `style.py check --family karakter`: 8 GEÇTİ (palete yakın %97-98, doygunluk x0,97-1,06), UYARI/KALDI yok.
- Boy: opak 74-80 px, man_r84 (78) ile aynı bantta. Alt boşluk n yönünde 6 px (diğerleri 2-4); içerik kaydırılmadı, Unity'de pivot ayak tabanına göre ayarlanmalı.
- Siluet: bordo tabard, çan ve tüylü şapkayla köylülerden ayırt ediliyor.

## Sorunlar
- **Şapka tarife uymuyor:** "yumuşak koyu keçe takke" yerine geniş kenarlı, tüylü şapka çıktı (16.-17. yy görünümü). Üç köşeli şapka değil ama ortaçağdan biraz geç duruyor; kullanıcı yan yana sayfasına bakıp karar versin. Ret olursa şapka tarifi sıkılaştırılarak ayrı iş.
- Giysi omuzlu, kabarık kollu (geç Rönesans izi); tabard yerine ceket gibi duruyor.
- Ağız açıklığı bu boyda seçilmiyor.
- Yalnızca durma kareleri (iş notu).
