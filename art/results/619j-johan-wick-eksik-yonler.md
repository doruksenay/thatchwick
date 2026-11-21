unity-import: pending

# 619j Johan Wick: eksik yönler

- Araç: `animate_character`, `mode=pixminimax`, `frame_count=8`, `keep_first_frame=true`, `end_frame_url` = yönün rotasyon görseli (510 ile aynı). Tarifteki metinler, yön başına tek çağrı, 1 gen/çağrı. Toplam **4 gen** (tahmin 4, tavan 8). Eşzamanlılık: en çok 3 çağrı; kuzeybatı sevme, ilk üçten biri bitince gönderildi. Hız sınırı hatası yok.
- Animasyon adları: `walk2b` (sw yürüme), `pet2` (sevme; eski `pet` grubuna eşlenmesin diye). Gruplar: walk2b sw `848e02ae-...`, pet2 se `cc387d8f-...`, sw `004f7111-...`, nw `45ae1810-...`.
- `get_balance` sonrası: 9345 / 10000 (diğer şeritler aynı anda harcıyor). Defter: `art/ledger/619j.md`.
- İşleme (510 / 018e kuralı): yalnızca kaydırma; en alt opak satır idle karesinin satırına (82, nw için 80) çekildi, yatay ofset kare 0 ayak merkezinden (0 / -2). Ölçekleme yok, alfa ikili, palet = karakterin 8 durma karesi. Kare 0 ile son kare IoU > 0,95 olduğu için hepsinde son kare atıldı (8 kare). Kırpılan opak piksel: 0.
- Yan yana sayfası: `art/results/619-yanyana.png`. masaüstü kopyası: yerel

## Teslim edilen (24 PNG, 84x84, pivot x=42, pivot y=2 alttan)
`Assets/Art/Iso/_aday/anim/iso_char_johan_wick_r84_<klip>_<yön>_<NN>.png`

| Klip | Yön | Kare | Kafa farkı (üst satır max-min) | Kafa alçalması (kare 0'a göre) | En düşük ardışık IoU |
|---|---|---|---|---|---|
| walk2 | sw | 8 | 3 (ölçüt ≤ 3, geçti) | - | 0,90 |
| pet | se | 8 | 13 | 13 (≥ 8) | 0,85 |
| pet | sw | 8 | 8 | 8 (≥ 8) | 0,84 |

## Teslim EDİLMEYEN (`art/results/619-red/`, 8 PNG)
| Klip | Neden |
|---|---|
| pet nw | Kafa en çok 7 px alçalıyor (ölçüt ≥ 8). Sırt dönük, çanta önde; hareket okunuyor ama ölçüt tutmadı. Kod nw'de eldeki yöne düşer |

## Doğrulama
| Ölçüt | Değer |
|---|---|
| validate_iso (`--kind object --max-colors 32`) | 24/24 geçti |
| style.py check karakter | KALDI 0, UYARI 0, GEÇTİ 24 (doygunluk x1,19-1,21, palet yakınlığı %100) |
| Karede hayvan | yok (yan yana ve iletişim sayfasında gözle kontrol; se ve sw'de el aşağıda uzanıyor, köpek yok) |
| Ayak satırı | her karede sabit (82) |

## Sorunlar ve notlar
- pet nw için ölçüt 1 px kaçtı (7/8). İstenirse yeniden üretim işi ya da `art/results/619-red/` karelerini kabul etme kararı kullanıcıda.
- Kimlik (kapüşon, pelerin, sırt çantası) yan yana sayfasında korunmuş görünüyor; otomatik ölçüm yok.
- Betikler `art/tmp/` altında (işleme, kare indirme); `tools/art`'a dokunulmadı.
