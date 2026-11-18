unity-import: pending

# 369 Arayüz: kazı uyarıları ve görev ikonları: sonuç (durum: review)

Yan yana sayfası: `art/results/369-yanyana.png` (arayuz ailesinin kabul referansı yok, yalnızca yeni görseller).

## Araç ve ayarlar
- `create_image_pixen`, 24x24, `no_background=true`, `outline: single color outline`, `detail: low detail`. Gerekçe: 315 ile aynı yöntem; pixen küçük sprite'ta temiz ve 24x24'ü doğrudan destekliyor (komşu indirme gerekmedi).
- 8 çağrı paralel, çağrı başına 1 gen, toplam **8 gen** (cost_est 8, cost_max 12). Hız sınırı yok, yeniden deneme yok. `get_balance` sonrası: **4265,9** (diğer şeritlerin harcaması dahil).
- Masaüstü kopyası: yerel.

## Dosyalar
Hepsi 24x24, pivot x 12, pivot y 0 (alttan; ikon, pivot önemsiz). Teslim yeri `Assets/Art/Iso/_aday/` (iş dosyasındaki `Assets/Art/Icons/` işçi yazma yolları dışında; 315 de aynı yere teslim edildi).

| Dosya | Renk | Yarı saydam | Opak sınırı (x,y) |
|---|---|---|---|
| ui_mine_creak | 48 | 0 | 2-22, 3-21 |
| ui_mine_water | 30 | 0 | 2-22, 3-21 |
| ui_mine_smoke | 34 | 0 | 3-21, 3-20 |
| ui_mine_dark | 56 | 0 | 7-17, 1-22 |
| ui_mine_reshuffle | 36 | 0 | 3-21, 3-21 |
| ui_duty_mine | 45 | 0 | 2-22, 2-22 |
| ui_duty_steward | 60 | 0 | 1-24, 2-22 |
| ui_cart | 37 | 0 | 3-21, 3-21 |

## Stil denetimi (`style.py check --family arayuz`, kilit kurulmadı: yalnızca uyarı)
| Dosya | Palete yakın | Sonuç |
|---|---|---|
| ui_cart | %75 | UYARI |
| ui_duty_mine | %92 | GEÇTİ |
| ui_duty_steward | %99 | GEÇTİ |
| ui_mine_creak | %83 | UYARI |
| ui_mine_dark | %73 | UYARI |
| ui_mine_reshuffle | %75 | UYARI |
| ui_mine_smoke | %74 | UYARI |
| ui_mine_water | %37 | UYARI |

KALDI yok. `validate_iso.py` ikon için anlamlı değil (yalnızca ui_cart denendi: geçti).

## Sorunlar
1. Renk sayısı 30-60: 315'teki ≤12 ölçütünü hepsi aşıyor (315 de aşmıştı).
2. `ui_duty_mine`: kazmadan çok balta/çift başlı alet gibi; okunurluk göz kontrolüyle orta. Gerekirse yeniden üretim (1 gen).
3. `ui_duty_steward`: anahtar ve parşömen ayrı iki nesne, 24 px'te sıkışık (x 1-24, kenara değiyor).
4. `ui_mine_dark`: mum dar (10 px) ve sönmüş mum ucunda mavi alev benzeri parıltı var; duman telciği okunuyor ama "sönmüş" anlamı zayıf.
5. `ui_mine_reshuffle`: dönen ok renkli gökkuşağı çerçeve; ötekilerle dil farkı.
6. `ui_mine_water`: palet uyumu düşük (%37), parlak mavi.
7. Ölçülü ikon dili (kontur tonu, çerçeve) setler arasında tam birleşik değil; kullanıcı kararı yan yana sayfasına bakarak.
