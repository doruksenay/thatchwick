# Stil kilidi

Asset'lerin birbirine benzemesi için üç katman. Hiçbiri tek başına yetmez. Otomatik denetim kaba sapmayı yakalar; ince stil farkını pilot kabulü ve yan yana sayfası çözer.

## 1. Aile ve pilot
Her iş bir aileye aittir (`family`: zemin, yapi, esya, bitki, karakter, hayvan, arayuz). Her ailenin bir stil pilotu var (`style_gate: true`):

| Aile | Pilot | Durum (2026-11-13) |
|---|---|---|
| zemin | 001 | blocked: çimen adayı teslim edildi, kabulün bekleniyor |
| yapi | 007 | review (008a kit parçaları sohbette kabul edildi) |
| esya | 300, 301 | 300 review (masa, sandalye, mum, saksı sohbette kabul); 301 pending |
| bitki | 310 | pending (301'e bağlı) |
| karakter | 012 | review |
| hayvan | 100 | review |
| arayuz | 200 | pending |

Aile türevleri pilota `depends_on` ile bağlıdır, pilot kabul edilmeden başlamaz. 302-306'daki "301 sonucu görülmeden başlatılmaması önerilir" notu 2026-11-13'te bağımlılığa çevrildi.

## 2. Referanslar ve palet
- `references.json`: aile başına referans görseller (`kabul` ya da `aday`). PixelLab stil eşlemesi bunları kullanır. Pilot kabul edilince yeni çıktıları buraya ekle ve `aday`ları `kabul` yap.
- `palette.json`: tüm referanslardan çıkarılmış 64 renklik ana palet (`--png` ile `palette.png` önizlemesi yazılır, commit edilmez). Referans listesi değişince yeniden üret:
  `python tools/art/style.py palette`

## 3. Denetim ve yan yana sayfası
- `python tools/art/style.py check --family <aile> <png>...`:
  - palete ortalama uzaklık (uyarı > 10, kalır > 16);
  - palete yakın piksel oranı (kalır < %80);
  - aile referanslarına göre doygunluk (uyarı > 1,25 kat, kalır > 1,5 kat ya da uyarı < 0,6 kat).
- Eşikler 2026-11-13'te ölçüldü:
  - Kabul görmüş adaylar 4-12 uzaklıkta.
  - Tonu kaydırılmış ya da doygunluğu 1,8-2,2 kat artırılmış kopyalar kalıyor ya da uyarı alıyor.
  - Pembe tonlu raf adayı (`iso_item_shelf_wood_2x1_se_nw`) "palete uzak" uyarısı aldı; gözle de referanslardan ayrışıyor.
- `python tools/art/style.py sheet --family <aile> --out art/results/NNN-yanyana.png <png>...`: referanslar (R) ve yeni görseller (Y) 4 kat büyütülmüş, alt kenarları aynı çizgide. Kabul kararı buna bakılarak verilir.

PNG'ler Git LFS'te. Yerelde `git lfs pull` yapılmadıysa betikler "cannot identify image" hatası verir.
