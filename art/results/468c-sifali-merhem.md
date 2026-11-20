unity-import: pending

# 468c Şifalı merhem (durum: review, kısmi: dünya görseli teslim edilmedi)

Yan yana sayfası: [468c-yanyana.png](468c-yanyana.png) (ikon + reddedilen dünya görseli fit hâli). Masaüstü kopyası: yerel.

## Araç ve ayarlar
- **İkon:** `create_map_object` basic, 32x32, `high top-down`, `low detail`, `basic shading`, `single color outline`; açıklama tarifteki + `, no face, no eyes, no creature`. 1 gen. Ham çıktı 30 renkti; median-cut ile 16 renge indirildi (dither yok, alfa 0/255), ardından `style.py fit` (doygunluk x0.90) uygulandı.
- **Dünya:** `create_map_object` stil eşleme, referanssız kılavuz (32x32 tuvalde 12x6 taban elması, alttan 4 px yukarıda) + maske (`art/tmp468c/mk.py`, elle üretildi: `footprint_guide.py` 32x32 dekor tuvali için uygun değildi), 301 ortak eki. 1 gen.
- Toplam **2 gen** (cost_est 3, cost_max 8). `get_balance` sonrası: **2387,9** (paralel şeritler dahil).

## Dosyalar
| Yol | Piksel | Pivot x | Pivot y (alttan) |
|---|---|---|---|
| Assets/Art/Iso/_aday/icon_item_healing_salve.png | 32x32 | 16 | 0 (ikon, önemsiz) |
| iso_item_healing_salve_se.png | TESLİM EDİLMEDİ | | |

## Doğrulama
| Dosya | Renk | Yarı saydam | Stil denetimi | Sonuç |
|---|---|---|---|---|
| icon (fit sonrası) | 16 | 0 | %92 palete yakın, x1.15 | UYARI (palete uzak), teslim |
| icon (ham) | 30 | 0 | %72, x1.28 | KALDI (doygun) |
| dünya (ham) | 28 | 0 | %59, x1.44 | KALDI |
| dünya (fit x0.80) | | 0 | %70, x1.16 | KALDI (palet dışı) |

Dünya ham hâlinin `validate_iso.py` sonucu geçti (32x32, en alt opak satır y=28, taban elmasına değiyor); şekil okunuyor (kil kavanoz, bez kapak, beyaz mürver dalı). Ancak doygunluk düzeltmesinden sonra da palet uyumsuzluğu sürdüğü için teslim edilmedi. Ham ve fit hâlleri `art/results/468c-red/` altında. Zemin olmadığından döşeme testi yok.

## Sorunlar
1. Dünya görseli palet dışı renkler yüzünden KALDI; yeniden üretim (1 gen) ya da kullanıcı kararı gerek. Reddedilen hâlleri `468c-red/` içinde.
2. Bu işte `max_attempts: 2` olsa da çalışma kuralı gereği tek deneme yapıldı.
