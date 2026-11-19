unity-import: pending

# 395e Kömür yığını (rapor)

Sonuç: teslim edildi, **0 generation** (tarifin 1. adımı, ücretsiz yerel yol). Yan yana sayfası: [395e-yanyana.png](395e-yanyana.png) (sıra: yeni sonuç, 395d aday 2, 364 kireç ocağı; ardından referanslar). Masaüstü kopyası: yerel (bulutta yapılamaz).

## Yöntem (PixelLab çağrısı yok)
Kaynak: `art/results/395d-red/clamp2.png`.
1. Zemin yaması: alt/yan kenarlardan başlayan parlak yeşil bölge flood-fill ile silindi (1434 piksel), kalan küçük kırıntı bileşenleri (<30 px) temizlendi. Yığının içeriği yeniden boyanmadı.
2. Kalan yeşil (tepedeki çim, benekler) hem koyulaştırıldı hem doygunluğu düşürüldü.
3. `style.py fit --family esya` (doygunluk x0.84).
4. Palet dışı kalan 624 piksel (uzaklık > 14) en yakın aile paleti rengine eşlendi (`reduce_colors` yerine yerel eşleme; 24 renk).
5. İçerik 12 px aşağı kaydırıldı (alt boşluk 19 → 7, diğer eşyalarla aynı).

Kireç ocağı çim tonuna birebir eşleme yerine aile paletine eşleme yapıldı; çim sonuçta zeytin-kahve oldu (yeşil kalmadı).

## Harcama
Çağrı yok. get_balance: 3562,5 (diğer şeritlerle ortak bakiye).

## Dosyalar
| Yol | Piksel | Pivot x | Pivot y (alttan) |
|---|---|---|---|
| Assets/Art/Iso/_aday/iso_item_charcoal_clamp_f_2x2_se.png | 128x96 | 64 | 7 |

## Doğrulama
| Denetim | Sonuç |
|---|---|
| `validate_iso.py --kind object` | geçti (24 renk, alt boşluk 7) |
| `style.py check --family esya` | %100 palete yakın, ort. uzaklık 4.9, doygunluk x0.88, GEÇTİ |
| Ham (clamp2) | %52, KALDI |
| Işık | sol-üstten, gölge pişirilmemiş, zemin yaması yok |

## Sorunlar / notlar
- Yığın gövdesi mor-kahve (koyu toprak), tepe çimi kahverengiye döndü; yeşil çim yok. Kullanıcı görsel olarak değerlendirmeli.
- Duman izi yukarıda tuval kenarına değiyor (y=12 sonrası kaydırmayla sığdı).
- Önerilen ek: yeşil çim istenirse yalnızca tepe yaması için kireç ocağı yeşiline özel eşleme.
