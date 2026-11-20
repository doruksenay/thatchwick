unity-import: pending

# 497 Easter egg moodlet, başarım ve Tuhaflıklar ikonları: sonuç (durum: review)

Yan yana sayfası: `art/results/497-yanyana.png` (arayuz ailesinin kabul referansı yok; 0 referans + 24 yeni). Toplu önizleme x4: `art/results/497-ikonlar-x4.png` (parşömen), `art/results/497-ikonlar-x4-koyu.png` (#333333).

## Araç ve ayarlar
- `create_image_pixen` 32x32, `no_background=true`, `outline: single color outline`, `detail: low detail`, 212 ile aynı; ikon başına 1 çağrı = **1 gen**, toplam **14 gen** (cost_est 14). Yön: varsayılan `parsomen`; stil eşlemesi yok.
- Hız sınırı: 10/10 eşzamanlı iş; 4 çağrı reddedildi (harcama yok), işler bitince bir kez yeniden gönderildi (3 başarım + sekme ikonu).
- 32x32 ham: alfa ikili, moodlet için 12, başarım/sekme için 16 renge (Pillow mediancut, dither yok). Moodlet 16x16: en yakın komşu indirme, alfa ikili, 12 renk.
- `get_balance` sonrası: 2338,7 (diğer şeritlerin harcaması dahil). Masaüstü kopyası: yerel.

## Dosyalar (`Assets/Art/Iso/_aday/`; ikon, pivot önemsiz: pivot x merkez, pivot y 0 alttan)
- Moodlet (16x16 ve `_32` 32x32): ui_icon_moodlet_{animal_spoke,confidant,not_crazy,believed,family_tale,nobody_believes,odd_chat,thoughtful,let_down,scarecrow_chat}
- 32x32: ui_icon_achievement_{animal_spoke,runs_in_family,question_the_sword}, ui_icon_tab_oddities

## Doğrulama
| Ölçüt | Sonuç |
|---|---|
| Boyut | moodlet 16x16 + `_32`; başarım ve sekme 32x32 |
| Yarı saydam | 0 |
| Renk | moodlet 11-12 (≤12), başarım/sekme 15 (≤16) |
| `validate_iso.py` | ikonlara uygulanmaz, atlandı |
| Stil denetimi | KALDI yok; GEÇTİ: 13, UYARI (palete uzak / kilit kurulmadı): 11 |
| Taç ayrımı | let_down kurbağasında taç yok, tab_oddities kurbağasında taç var |
| Yazı/harf | belirgin harf yok; sembol işaretleri (onay, soru, çarpı) sembol |

## Sorunlar
1. 16x16 hâller 212'deki gibi zayıf: `believed`, `nobody_believes`, `odd_chat` balonları 16'da ancak 32'de ayırt ediliyor; 3x göz kontrolünde `_32` sürümleri öneririm.
2. `believed` onay işareti yeşil yerine kahve/turuncuya yakın çıktı (kontur renginde); ayırt edilebilir ama tarife tam uymuyor.
3. `let_down` kurbağasının üzgün ağzı belirgin değil; `tab_oddities` tacı net.
4. `achievement_runs_in_family` yaprak üstündeki pati izleri küçük/belirsiz; `achievement_question_the_sword`'da soru işareti küçük.
5. Kabul edilince `Assets/Resources/UI/Icons/` kopyası kullanıcı kararından sonra yapılır.
