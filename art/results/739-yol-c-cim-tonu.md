unity-import: pending

# 739 Yol geçiş seti c çeşidi: çim tonu düzeltmesi

- Araç: PixelLab çağrılmadı (`pixellab: false`). Python/Pillow ile yalnızca çim piksellerine (G-R>=8 ve G-B>=8) kanal başına sabit kaydırma; hedef `iso_ground_grass_a` çim ortalaması (115,4 / 178,8 / 95,0). Desen, kontrast, toprak pikselleri ve saydamlık aynı (alfa farkı 0). `style.py fit` kullanılmadı: o doygunluk ölçekler, burada istenen kanal kaydırmasıydı.
- Harcanan generation: 0. Kalan (get_balance): 809,6.
- Yan yana (üst sıra eski, alt sıra yeni): art/results/739-yanyana.png
- 12 karoluk yol önizlemesi ve döşeme testi yapılmadı; kullanıcı yan yana sayfasına bakarak karar verir. Çim tonları ana çimle ±1 içinde.

| Dosya | Boyut | Çim önce (R,G,B) | Çim sonra | validate_iso ground |
|---|---|---|---|---|
| wang_00_c_v2 | 64x32 | 101,6 159,0 86,1 | 115,6 179,0 95,1 | geçti |
| wang_01_c_v2 | 64x32 | 101,3 159,0 85,9 | 115,3 179,0 94,9 | geçti |
| wang_02_c_v2 | 64x32 | 106,0 163,9 89,6 | 115,0 178,9 94,6 | geçti |
| wang_03_c_v2 | 64x32 | 99,7 157,5 84,6 | 115,7 178,5 94,6 | geçti |
| wang_04_c_v2 | 64x32 | 105,0 162,7 88,9 | 115,0 178,7 94,9 | geçti |
| wang_05_c_v2 | 64x32 | 103,2 161,0 87,4 | 115,2 179,0 95,4 | geçti |
| wang_06_c_v2 | 64x32 | 105,7 163,5 89,5 | 115,7 178,5 94,5 | geçti |
| wang_07_c_v2 | 64x32 | 100,2 158,1 85,0 | 115,2 179,1 95,0 | geçti |
| wang_08_c_v2 | 64x32 | 103,4 161,3 87,6 | 115,4 179,3 94,6 | geçti |
| wang_09_c_v2 | 64x32 | 102,3 159,9 86,7 | 115,3 178,9 94,7 | geçti |
| wang_10_c_v2 | 64x32 | 106,7 164,7 90,3 | 115,7 178,7 95,3 | geçti |
| wang_11_c_v2 | 64x32 | 101,4 159,5 85,9 | 115,4 178,5 94,9 | geçti |
| wang_12_c_v2 | 64x32 | 104,8 162,4 88,8 | 115,8 178,4 94,8 | geçti |
| wang_13_c_v2 | 64x32 | 102,9 160,4 87,2 | 115,9 178,4 95,2 | geçti |
| wang_14_c_v2 | 64x32 | 107,5 165,1 91,1 | 115,5 179,1 95,1 | geçti |

Pivot: zemin karosu, merkez x=32, y=16 (alttan 16).

## Sorunlar
- Kuyrukta iki dosya 739 numarasını taşıyor (`739-kavga-toz-bulutu-efekti.md` pending, `739-yol-gecis-c-cim-tonu.md`). Yalnızca `running` olan yapıldı; kavga işi dokunulmadı, numara çakışmasını kullanıcı çözmeli.
- Masaüstü kopyası: yerel.
