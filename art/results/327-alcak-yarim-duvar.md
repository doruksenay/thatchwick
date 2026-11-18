unity-import: pending

# 327 Alçak ve yarım duvar varyantları (bulut işçisi, 2026-11-18)

Yan yana sayfası: [327-yanyana.png](327-yanyana.png)

## Araç ve ayarlar
- `pixellab: false`, **0 generation**. `get_balance` çağrılmadı (harcama yok, iş bütçeden bağımsız).
- Betik `art/tmp/wall_variants.py` (bulutta `tools/art`'a yazılmaz; kalıcı betik gerekirse öneri aşağıda). Sütun bazında: en alttaki opak pikselden yukarı 10 px (`low`) ya da 20 px (`half`) bırakılır, üstü silinir, kesilen üst piksele +45 RGB açık ton kapak konur. Alt kenar çapraz olduğu için üst kenar da çaprazdır (tabana paralel). Tuval 102x115 ve pivot değişmedi.
- Varsayılan alçak duvar (008b) zaten var; yalnızca `iso_wall_wall_half_*` eklendi.
- Masaüstü kopyası: yerel.

## Dosyalar (22; hepsi 102x115, pivot x=32 sw_ne / 70 se_nw, y=19 ALTTAN)
Varsayılan: `iso_wall_wall_half_{sw_ne,se_nw}`; malzemeler plank, stone, cob, timber, ashlar için `_low_` ve `_half_` x iki eksen. Hepsi `Assets/Art/Iso/` altında.

## Doğrulama
- `validate_iso.py --kind object --max-colors 48`: 22/22 geçti, yarı saydam piksel 0.
- Alt kenar kaynakla farkı: 22/22 dosyada 0 (üst kapak satırı hariç).
- `style.py check --family yapi`: KALDI yok; 5 UYARI (ashlar 4 dosya soluk, `ashlar_*_sw_ne` palete uzak; cob sw_ne doygun). Bu uyarılar 326 kaynağı tam duvardan devralınmıştır, kırpma renk değiştirmez (kapak hariç).

| Dosya | Palete yakın | Ort. uzaklık | Doygunluk | Sonuç |
|---|---|---|---|---|
| ashlar half se_nw / sw_ne | %100 | 7.9 / 11.8 | x0.11 / x0.12 | UYARI |
| ashlar low se_nw / sw_ne | %100 | 7.8 / 12.0 | x0.12 / x0.12 | UYARI |
| cob half sw_ne, low sw_ne | %100 | 5.9, 6.5 | x1.27, x1.30 | UYARI |
| diğer 13 dosya | %100 | 1.8-6.5 | x0.71-1.22 | GEÇTİ |

## Sorunlar
- Duvar çaprazı olduğu için "üst kenar düz" ölçütü çapraz-paralel olarak yorumlandı.
- Öneri: betik `tools/art/wall_variants.py` olarak ayrıca bir PR ile alınabilir (`art/tmp/wall_variants.py` içeriği).
