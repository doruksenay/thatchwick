unity-import: pending

# 796 Erkek güneybatı yoğurma (1 klip): BAŞARISIZ, teslim yok

- Araç: `animate_character`, `mode=pixminimax`, `frame_count=8`, `keep_first_frame=true`, `end_frame_url` = man_r84 south-west rotasyon karesi, yön `south-west`. Tarifteki eylem metni aynen. Tek çağrı, grup `41ede75c-226d-48bf-b388-2f9d67a625a4`, job `d1bbdce0-de7e-4534-8307-73cbf88051a9`, 1 gen (tahmin 1, tavan 2).
- `get_balance` sonrası: 9984 (Tier 3, plan `art/budget.md` ile uyumlu). Diğer şeritler de harcadığı için bu sayı yalnızca bu işe ait değil.
- masaüstü kopyası: yerel

## Sonuç
Ham kareler 84x112, 9 kare. Boyut sorunu yok: içerik 37x78 piksel, 84x84'e kırpmasız sığdı (ayak satırı y=81, palet cand_se 32 renk, kırpılan piksel 0). Ama klip **hareketsiz**: kare 2-8, kare 1'den en çok 16 piksel farklı; karakter durma pozunda kalıyor, yoğurma yok (kareler gözle de kontrol edildi). Kare 0 - 8 IoU 0,999. Böyle bir klip koda "yoğurma" diye verilirse oyuncu donmuş karakter görür.

Tek deneme kuralı gereği yeniden üretilmedi. Teslim edilmedi: `Assets/Art/Iso` altına dosya konmadı. İşlenmiş kareler yalnızca çalışma klasöründe (`art/tmp/796/out`, git dışı); PixelLab tarafında animasyon `knead_sw` (grup yukarıda) duruyor, silinmedi.

## Doğrulama
| Ölçüt | Değer |
|---|---|
| Boyut | 84x84 sığdı, kırpma 0 |
| Ayak satırı | y=81 |
| Hareket (kare 1-8 arası fark) | 3-16 px: YETERSİZ |
| validate_iso / style check / GIF | koşulmadı (teslim edilmeyecek) |

## Öneri
Yeniden deneme kullanıcı kararı. Seçenekler: (a) `direction_prompts` ile hareketi güneybatıya göre açık yazmak (ör. "both forearms push forward and pull back repeatedly"), (b) `mode=v3` (~3 gen), (c) `skeleton-v3` özel pozlar (3 gen).
