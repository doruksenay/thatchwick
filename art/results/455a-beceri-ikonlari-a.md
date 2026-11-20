unity-import: pending

# 455a Beceri ikonları A (18)

- Araç: `create_map_object` 32x32, `view: side`, `single color outline`, `basic shading`, `medium detail` (399a/407y yöntemi), tarifteki ortak ek ve palet metniyle. Tek deneme.
- Hız sınırı: herbalism, salvage ve smithing çağrıları 429 aldı; bekleyip bir kez yeniden gönderildi (smithing iki kez; başarısız çağrılar iş açmadı).
- Harcama: 18 çağrı ≈ 18 generation. `get_balance` sonrası 3039,9; bakiye paralel şeritlerle ortak olduğundan iş başına fark buradan okunamaz.
- Son işlem: yarı saydam 0 (alfa eşiği 128), 16 renge indirme (medyan kesim, titreşimsiz; 16'dan fazla rengi olanlarda). Ham çıktılar `art/tmp455a/` altında (commit edilmez).
- Yan yana sayfası: [455a-yanyana.png](455a-yanyana.png) (arayuz ailesinin kabul referansı yok, yalnız yeni dosyalar).
- Masaüstü kopyası: yerel.

## Dosyalar (hepsi 32x32; pivot alttan ortalı, x=16, y=2)
Hepsi `Assets/Art/Iso/_aday/` altında: ui_icon_skill_ + farming, woodcutting, cooking, carpentry, animalhusbandry, riding, fishing, leatherwork, weaving, trade, herbalism, engineering, salvage, masonry, mining, smithing, pottery, brewing (.png).

## Doğrulama
`style.py check --family arayuz`: kilit kurulmadı, palet uyumu yalnızca uyarı. KALDI yok.

| Sonuç | Dosyalar |
|---|---|
| GEÇTİ (12) | animalhusbandry, brewing, carpentry, cooking, engineering, masonry, pottery, riding, salvage, smithing, weaving, woodcutting |
| UYARI (6) | farming (%65 palet), herbalism (%61 palet), fishing, leatherwork, mining, trade (palete uzak) |

Not: GEÇTİ 12, UYARI 6 (toplam 18). Renk sayıları 9-16, yarı saydam 0 (betikle sayıldı). `validate_iso.py` örnek dosyada (farming) geçti; ikonlar karo değil, ayak izi denetimi anlamsız.

## Göz kontrolü (x3, koyu zemin)
Hepsi tanınır. Zayıflar: **salvage** (sandık okunuyor, levye/çivi belirsiz), **fishing** (olta çizgisi ince, balık küçük), **weaving** (iğ makaraya benziyor). Karışma riski: herbalism ile (455b) toplayıcılık/bahçıvanlık, 455b bitince 35'lik sayfada bakılmalı. Ateşli ikonlar (cooking) için `--light` denetimi yapılmadı.

## Sorunlar
- Tek deneme kuralı gereği zayıf üç ikon yeniden üretilmedi; gerekirse pixflux ikinci denemesi için ayrı iş.
- Kabul sonrası `Assets/Resources/UI/Icons/` kopyası kullanıcı/kod oturumuna ait (yasaklı yol değil ama kabul bekliyor).
