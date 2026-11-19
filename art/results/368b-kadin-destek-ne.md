unity-import: pending

# 368b Kadın destek kurma, kuzeydoğu (ne)

- Araç: `animate_character`, `mode=pixminimax`, `frame_count=8`, `keep_first_frame=true`, `end_frame_url` = `woman_r84` north-east döndürme görseli. Karakter `135cb0b1-bd46-4514-8c87-0834b96ef80d`. 1 deneme, 1 generation (tahmin 3, tavan 8). Animasyon grubu `3c266f8a-b28f-4ec6-a0bf-3bab246900f0`.
- Eylem metni: `lifting a timber beam to shoulder height and fixing it in place, holds the beam at shoulder height, not above the head` (kirişi başın üstüne kaldırmayan tarif; 368'deki tepe kırpılması buradan çözüldü).
- `get_balance` sonrası kalan: 3737,5 -> işlemden önce okunan değer; ardından 1 gen harcandı, defter 3736,5 (diğer şeritler aynı anda harcıyor).
- İşleme (368 ile aynı): ham 84x112; en alt opak satır y=81'e kaydırıldı (ölçekleme yok), palet `iso_char_woman_r84_cand_se.png` 32 renk en yakın RGB, alfa ikili. Kare 0 / kare 8 IoU = 0,72 (> 0,95 değil) -> 9 kare tutuldu.
- Yan yana: art/results/368b-yanyana.png. GIF (kadın ne solda, kadın nw sağda, 2x): art/results/368b-woman-support-ne-nw.gif
- masaüstü kopyası: yerel

## Dosyalar
`Assets/Art/Iso/_aday/anim/iso_char_woman_r84_mine_support_ne_00..08.png`, 9 adet, 84x84, pivot x=42, y=2 (alttan).

## Doğrulama
| Ölçüt | Değer |
|---|---|
| validate_iso --kind object | 9/9 geçti |
| style.py check karakter | 9 GEÇTİ, KALDI 0, UYARI 0 (aile kilidi kurulmadı, palet yakınlığı %96-97, doygunluk x0,93-1,01) |
| Tepe kırpılması | 0 px (en üst opak satır y=3); kriter <= 3 px sağlandı |
| Ayak satırı | her karede y=81 |
| Aynı kare tekrarı | birebir aynı kare yok |
| Kapanış (kare 0 / son kare IoU) | 0,72 |

## Sorunlar ve notlar
- Kapanış zayıf: kare 0 (kirişsiz durma) ile son kare (kiriş omuzda) farklı; 08 -> 00 döngüsü sıçrar. Destek kurma tek seferlik eylem olduğundan oyunda döngü yerine son kareyi tutmak yeterli olabilir (kullanıcı kararı).
- Kareler 4-8 neredeyse statik (kiriş omuzda sabit); hareketin asıl kısmı 0-4. Kimlik (saç, elbise) yan yana sayfada tutarlı görünüyor; yüz bu yönde zaten görünmüyor.
- Okunurluk (kirişin omuzda durması, ellerin yerleşimi) GIF'ten kullanıcı teyidi gerekir.
- Dosya adı iş dosyasındaki `iso_char_woman_r84_support_ne_<NN>` yerine 368'in düzenine uydu (`..._mine_support_ne_<NN>`).
