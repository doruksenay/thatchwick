unity-import: pending

# 018a2 Yürüme pilotu yeniden: sekmesiz (SONUÇ: teslim edilmedi, status failed)

- Karakter: man_r84 (c63fe39d), yalnızca SE. Dosya teslim edilmedi (`Assets/Art/Iso` altına yazılmadı).
- Yol 1: `animate_character` mode=template, `walking`, group 36e4bb5a (1 gen). Çıktı 6 kare 84x84.
- Yol 2: mode=pixminimax, frame_count=8, keep_first_frame, end_frame = cand_se, tarifteki açıklama, group b250f0b6 (1 gen). Çıktı 9 kare 84x112.
- Harcanan: 2 gen (tahmin 8, üst sınır 20). get_balance sonrası: 4161,9 (diğer şeritler de harcıyor, yalnızca bu işe ait değil).
- Yan yana (şablon üst, pixminimax alt; ham kareler): art/results/018a2-yanyana.png. Ham kareler: art/results/018a2-red/. (018a ile GIF karşılaştırması yapılmadı: kareler teslim edilmedi.)
- masaüstü kopyası: yerel

## Sekme ölçümü (00'a göre fark; tepe = bbox üst, ayak = bbox alt satırı)
| Yol | Kare | Tepe y | Ayak y | Tepe farkı | Ayak farkı |
|---|---|---|---|---|---|
| şablon | 00 | 4 | 79 | 0 | 0 |
| şablon | 01 | 4 | 76 | 0 | -3 |
| şablon | 02 | 4 | 75 | 0 | -4 |
| şablon | 03 | 4 | 79 | 0 | 0 |
| şablon | 04 | 4 | 80 | 0 | +1 |
| şablon | 05 | 3 | 79 | -1 | 0 |
| pixminimax | 00 | 18 | 95 | 0 | 0 |
| pixminimax | 01 | 17 | 94 | -1 | -1 |
| pixminimax | 02 | 16 | 93 | -2 | -2 |
| pixminimax | 03 | 17 | 92 | -1 | -3 |
| pixminimax | 04 | 18 | 92 | 0 | -3 |
| pixminimax | 05 | 18 | 91 | 0 | -4 |
| pixminimax | 06 | 17 | 90 | -1 | -5 |
| pixminimax | 07 | 17 | 93 | -1 | -2 |
| pixminimax | 08 | 18 | 95 | 0 | 0 |

## Sonuç
- Tepe ölçütü (en çok 3 px) iki yolda da geçti (şablon en çok 1, pixminimax en çok 2).
- Ayak satırı ölçütü (bütün karelerde aynı) iki yolda da KALDI: şablonda 75-80 (5 px), pixminimax'ta 90-95 (5 px). Karakter yürürken boyu kısalıyor (tepe sabit, ayak yukarı çıkıyor); kareleri ayağa göre hizalamak tepeyi 4-5 px oynatır, yani 018a'daki sekmeyi geri getirir. Bütün kareyi kaydırmak tarifle yasak, bu yüzden düzeltilemedi.
- Stil denetimi ve validate_iso çalıştırılmadı: kareler teslim edilmiyor.
- Şablon kareleri görsel olarak iyi (baş sabit, kimlik korunmuş); karar kullanıcı gözüyle verilebilir.

## Sorular
- Ayak ölçütünü gevşetmek kararı kullanıcıda (ör. "en alçak ayak yere basıyor" ya da en çok 4-5 px). İstenirse şablon yürüyüşü teslim edilebilir (kareler art/results/018a2-red/raw_*.png, ek maliyet yok). Ayrıca `walking-6-frames`/`walking-8-frames` şablonları denenebilir (1 gen/yön).
- Öneri: PixelLab yüklemesi için `curl` yok; python ile multipart yükleme çalıştı, `tools/art`'a bir yükleme betiği eklenmesi önerilir.
