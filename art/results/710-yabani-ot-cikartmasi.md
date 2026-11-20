unity-import: pending

# 710 Yabani ot çıkartması (bulut şeridi 0)

- Araç: `create_map_object` temel mod, 64x32, high top-down, selective outline, basic shading, low detail; 2 çağrı paralel, çağrı başına 1 gen (toplam 2). Stil eşlemesi kullanılmadı: saydam çıkartma için temel mod (358 ile aynı gerekçe); palet uyumu `fit` ile.
- Kalan (`get_balance`): 889,6.
- Masaüstü kopyası: yerel.
- Yan yana: `art/results/710-yanyana.png`

## Dosyalar (`Assets/Art/Iso/`, 64x32, pivot x 32, y 16 ALTTAN, karo ortası)
| Dosya | Not |
|---|---|
| iso_decal_weeds_a.png | ham hâli KALDI (doygunluk x1,87); `fit` ile doygunluk x0,62'ye indi, GEÇTİ. Ham: `art/710-red/weeds_a_ham.png`. İçerik: yatay sırada 4 tutam, ikisi sarı çiçekli |
| iso_decal_weeds_b.png | UYARI (palete uzak, doygun x1,40), olduğu gibi teslim; tek sarı çiçek, alt kenar boyunca ot şeridi |

## Doğrulama
| Dosya | Palete yakın | Doygunluk/aile | Sonuç |
|---|---|---|---|
| weeds_a | %96 | x1,15 | GEÇTİ |
| weeds_b | %90 | x1,40 | UYARI |

`validate_iso.py --kind object`: ikisi geçti (yarı saydam 0; a: 11 renk, opak y 8-23; b: 31 renk, opak y 1-27).

## Sorunlar
- a, "dağınık tutam" yerine tek sırada dizilmiş 4 tutam; b karonun alt kenarında şerit. İkisi de karo şekline tam oturmuyor; kullanıcı yan yana sayfasına bakıp karar verir.
- `validate_iso.py` tek dosya alıyor. `tools/art` değişmedi.
