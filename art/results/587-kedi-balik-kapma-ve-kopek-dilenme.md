unity-import: pending

# 587 Kedi balık kapma ve köpek dilenme animasyonları

Sonuç: `status: review`. 16 klip-yön üretildi ve teslim edildi (144 PNG). **Hareket kalitesi zayıf, görsel onayda dikkat:** aşağıdaki "Sorunlar". Masaüstü kopyası: yerel (bulut modu).
Yan yana sayfası (kedi sneak/pounce/run_fish ve köpek beg SE, referanslarla): `art/results/587-yanyana.png`

## Araç ve ayarlar
- `animate_character`, `mode=pixminimax`, `frame_count=8`, `keep_first_frame=true` (9 kare: 0 = referans durma karesi, 1-8 hareket), yönler SE, SW, NW, NE. 513'teki yöntemle aynı (dört ayaklıda kabul edilmişti), tarifteki eylem metinleri aynen.
- Karakterler: kedi cat_1_375 `11de8a77-8e44-43c3-b4ab-68f45412dcee`, köpek dog_1 `2ae427e6-4a51-4005-98ae-0e4f7a7cefc0` (379 temel köpeği).
- İlk denemede iş yuvası sınırı (8/10 dolu) vardı; çağrılar yuva boşalınca bölünerek (run_fish önce SE+SW, sonra aynı gruba NW+NE) gönderildi. Yeniden üretim yok.
- Gruplar: sneak 2629c314-54a8-4b44-8617-5c4fd6cc87bd, pounce 0760ebe4-b9d8-4f4f-bd3d-ebb5d94c6a66, run_fish 96c0ae8b-3a54-4595-a653-fa023ec607e5, beg 0823e94c-9187-430f-ae80-4a54661a02cf.
- Sonradan işlem (ücretsiz, 513 ile aynı): kare 0'ın alt satırı ve yatay merkezi, aynı gövdenin `_cand_<yön>` durma görseline oturtuldu (kedi 32x32, köpek 44x44 tuvali), tüm karelere aynı kaydırma. Piksel kaybı 143/144 dosyada yok; `run_fish_ne` karelerinde toplam 4 opak piksel tuval dışında kaldı.

## Generation
| Çağrı | Gen |
|---|---|
| sneak (4 yön) | 4 |
| pounce (4 yön) | 4 |
| run_fish (2+2 yön) | 4 |
| beg (4 yön) | 4 |
| Toplam | 16 (cost_est 16) |

`get_balance` sonrası: 1132,6 (diğer şeritlerin harcaması dahil).

## Dosyalar (`Assets/Art/Iso/_aday/anim/`, 9 kare `_00`-`_08`)
| Ad | Piksel | Pivot x | Pivot y (alttan) |
|---|---|---|---|
| iso_animal_cat_1_{sneak,pounce,run_fish}_{se,sw,nw,ne}_NN | 32x32 | durma karesinin merkezi (cand ile aynı, ~14-15) | 1-3 (cand ile aynı) |
| iso_animal_dog_1_beg_{se,sw,nw,ne}_NN | 44x44 | ~19 (cand ile aynı) | 4 |

## Doğrulama
| Kontrol | Sonuç |
|---|---|
| `validate_iso.py --kind object` (her klip SE ve NE, kare 04) | 8/8 geçti |
| `style.py check --family hayvan` (144 kare) | 144 UYARI, KALDI 0. Kedi: ailesinden soluk (x0,25 civarı, 375 ile aynı); köpek: ailesinden doygun (x1,34-1,38; kabul eşiği içinde, `fit` uygulanmadı) |
| Gölge pişirilmemiş, ışık sol-üstten, zemin lekesi yok | Evet |
| `run_fish`'te balık okunur | SE/SW karelerinde okunur (beyaz-gümüş balık ağızda); NW/NE'de ince ve kısmen gövdenin arkasında |
| Klip başına GIF | **Üretilmedi** (bulutta betik ekleme yasağı nedeniyle yalnızca kare şeridi); kare şeridi yerelde `art/tmp587/strip.png` |

## Sorunlar
- **Kedi pozu durma karesinden kopmuyor:** `sneak` kareleri neredeyse hareketsiz, kedi oturuyor (alçak sürünme yok). `run_fish` oturan kedi ağzında balıkla; koşu/bacak hareketi yok. `pounce` yalnızca son 1-2 karede sıçrıyor (SE, NW'de). Kaynak karakter 375'teki oturan duruş; pixminimax duruşu terk etmiyor. Öneri: dört ayak üstü ayakta bir kedi gövdesi ya da şablon klip (`walk-8-frames`, `running-8-frames`) + balık kare kaplaması, ya da `create_character_state` ile ayakta duruş.
- **Köpek `beg` oturmuyor:** kareler ayakta duruyor (dog_1'in `cand` karesi oturan olsa da pixminimax ayağa kaldırmış), kuyruk ve baş hareketi hafif. Oturan-pozlu bir ikinci deneme gerekebilir.
- Kare 0 referans durma karesidir; döngü 01-08 oynatılabilir (513'teki not).
- `run_fish_ne`: 4 piksel tuval dışında kaldı (balık ucu olabilir).
- Önerim kullanıcıya: kabul etmeden önce `art/tmp587/strip.png` yerine kareleri `Assets/Art/Iso/_aday/anim/` içinde izle; muhtemelen reddedilip yeni iş açılacak.
