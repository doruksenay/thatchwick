---
id: "783"
title: "Kedi ve küçük köpek animasyonları ölçek setinde: sinsi yürüyüş, sıçrama, balıkla kaçma, kutu, uluma, yürüme"
status: pending
approved_by: user        # kullanıcı kararı 2026-11-21 sohbet: "kedinin ve küçük köpeğin bütün animasyonları ... bunu yap"
priority: 40
family: hayvan
style_gate: false
cost_est: 24
cost_max: 36
depends_on: ["780"]
max_attempts: 1
pixellab: true
supersedes:
  - Assets/Art/Iso/_aday/anim/iso_animal_cat_1_{sneak,pounce,run_fish}_*.png
  - Assets/Art/Iso/_aday/iso_animal_cat_1_box_*.png
  - Assets/Art/Iso/_aday/iso_animal_dog_lapdog_howl_*.png
outputs:
  - Assets/Art/Iso/_aday/iso_animal_cat_1_v2_{sneak,pounce,run_fish,walk}_{se,sw,nw,ne}_<NN>.png
  - Assets/Art/Iso/_aday/iso_animal_cat_1_v2_box_{se,sw}_<NN>.png
  - Assets/Art/Iso/_aday/iso_animal_dog_lapdog_v2_walk_{se,sw,nw,ne}_<NN>.png
  - Assets/Art/Iso/_aday/iso_animal_dog_lapdog_v2_howl_{se,sw}_<NN>.png
  - art/results/783-*.md
created: 2026-11-21
---
# 783 Kedi ve küçük köpek animasyonları (ölçek seti)

## Amaç
Eldeki 126 kedi karesi (587: sinsi yürüyüş, sıçrama, balıkla kaçma; 513: kutu) ve 18 küçük köpek karesi (513: uluma) eski, büyük gövdeden üretildi. Kullanıcı kararı (2026-11-21): bunların hepsi 780'in küçük gövdesinden yeniden üretilir. Yürüme klibi de eklenir: kod artık yürüyen hayvanda `walk` klibini oynatıyor (`IsoArt.AnimalMotion`), yoksa durma görseli kayar.

## Tarif
- Karakterler: 780'de döndürülen `cat_1_v2` ve `dog_lapdog_v2` (kimlikler `art/results/780-*.md`). Kimlik bulunamazsa çağrı yapılmaz, `## Sorular`.
- Yöntem: `animate_character`, `frame_count=8`, `keep_first_frame=true` (587 ve 513 ile aynı mod; dört ayaklıda mod kabul edilmezse dur, `## Sorular`). Yön başına 1 çağrı.

| Gövde | Klip | Yönler | Çağrı | Eylem metni |
|---|---|---|---|---|
| cat_1_v2 | `sneak` | se, sw, nw, ne | 4 | 587 ile aynı: `creeping forward very low with the belly near the ground, tail tip twitching, slow loop` |
| cat_1_v2 | `pounce` | 4 çapraz | 4 | 587: `crouching, wiggling the hindquarters and springing forward to grab something with the mouth` |
| cat_1_v2 | `run_fish` | 4 çapraz | 4 | 587: `running fast with a small silver fish held in the mouth, loop` |
| cat_1_v2 | `box` | se, sw | 2 | 513: `the cat sits upright and compact with paws tucked under, ears twitching, head turning slowly left and right, looking around` + 513'teki kutu kırpması (üst %55 kalır, en alt opak satır pivot satırına) |
| cat_1_v2 | `walk` | 4 çapraz | 4 | `walking calmly forward at a relaxed pace, tail up, loop` |
| dog_lapdog_v2 | `howl` | se, sw | 2 | 513: `the dog sits down on its haunches, raises its muzzle straight up to the sky and howls, ears back, then holds the pose` |
| dog_lapdog_v2 | `walk` | 4 çapraz | 4 | `trotting forward with small quick steps, tail curled up, loop` |

- Toplam 24 çağrı ≈ 24 gen.
- **Kırpma (780 kuralı):** her kare, gövdenin 780'deki durma tuvaliyle **aynı boyda**; pivot satırı (alttan 1 px) ve yatay orta durma karesiyle aynı yerde. Kare tuvale sığmıyorsa (sıçrama) tuval o klip için yukarı ve iki yana eşit genişletilir; alt satır ve yatay orta değişmez. Yeniden örnekleme yok.
- `walk` kareleri 9 gelirse son kare kapanış sayılır (kod 8 karelik döngü oynatır); olduğu gibi teslim edilir.
- Renk: `style.py fit --family hayvan` (kare başına, ücretsiz), palet durma karesinden.
- Dosya adı: `Assets/Art/Iso/_aday/iso_animal_<gövde>_v2_<klip>_<yön>_<NN>.png` (**anim/ değil**: kütüphane hayvan karelerini anim/ altından almaz).

## Doğrulama
- `python tools/art/scale_check.py` her klibin 00 karesinde: kedi 16-24, küçük köpek 18-26 (`box` hariç).
- `validate_iso.py --kind object`, `style.py check --family hayvan` (KALDI kare teslim edilmez; klibin kalan kareleri de teslim edilmez, raporda yazılır).
- Klip başına GIF (`art/results/783-<gövde>-<klip>-<yön>.gif`); kimlik (renk, boy) durma karesiyle aynı; ayak satırı kayması ≤1 px.

## Notlar
- Kod hazır: `walk` (yürürken), `box`, `howl` (sahne) önce `_v2` karelerini kullanır. `sneak`, `pounce`, `run_fish` sahneye bağlanınca (koku ve kedi partisi) `IsoArt.AnimalPose` aynı adları bulur.

<!-- İşçi bundan sonrasına dokunmaz; kullanıcı kararı:
status: review → accepted ya da rejected yap, rejected ise nedenini buraya yaz. -->
