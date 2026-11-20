---
id: "780"
title: "Hayvan ölçeği pilotu: kedi ve küçük köpek, küçük gövde yöntemi (8 yön)"
status: pending
approved_by: user        # kullanıcı kararı 2026-11-21 sohbet: "(b) hepsi birlikte", "küçük tuval: bunu da yap"; tester raporu 66 ölçek tablosu
priority: 40
family: hayvan
style_gate: true         # 781-783 ve 485k bu yöntemin tutmasına bağlı
cost_est: 4
cost_max: 10
depends_on: []
max_attempts: 1
pixellab: true
supersedes:              # kabul edilince tools/art/archive_superseded.py bunları Assets/Art/Iso/_arsiv/ altına taşır
  - Assets/Art/Iso/_aday/iso_animal_cat_1_cand_*.png
  - Assets/Art/Iso/_aday/iso_animal_dog_lapdog_cand_*.png
outputs:
  - Assets/Art/Iso/_aday/iso_animal_cat_1_v2_{se,sw,nw,ne,s,w,n,e}.png
  - Assets/Art/Iso/_aday/iso_animal_dog_lapdog_v2_{se,sw,nw,ne,s,w,n,e}.png
  - art/results/780-*.md
created: 2026-11-21
---
# 780 Hayvan ölçeği pilotu: kedi ve küçük köpek

## Amaç
Tester raporu 66 (Project belgesi `claude/66-tester-oturumu-2-asset-kalitesi-ve-tutarlilik.md`) ve `docs/gorsel-yonerge.md` ölçek tablosu: kedi 28 px (hedef 16-24), küçük köpek 30 px (hedef 18-26). Karakter 72-80 px yanında kedi bir kuzu boyunda görünüyor. Kullanıcı kararı (2026-11-21): bütün küçük hayvanlar ve kedi ile küçük köpeğin bütün animasyonları doğru ölçekte yeniden çizilir; çizim küçük gövdeyle yapılır (yeniden örnekleme yok); eski görseller silinmez, `_arsiv` klasöründe saklanır.

Bu iş yöntemin pilotudur: kedi (`cat_1`, tekir, beyaz ayaklı; 375 kabul) ve küçük köpek (`dog_lapdog`, 484). Animasyonlar (783) bu işte döndürülen karakterlerden üretilir.

## Tarif

### Sorun ve yöntem ("küçük gövde")
PixelLab araçlarının en küçük tuvali 32 px (pixflux ve v3 referansı; 375 ve 106 raporları). 32'lik tuvalde kedi tuvali doldurup 28-30 px çıkıyor. Hedef boy 32'nin altında olduğu için hayvan **32'lik tuvalde küçük çizdirilir**, tuval sonradan kırpılır:

1. **Tek kare (güney):** `create_map_object`, 32x32 saydam tuval, kılavuz + maske (301 yöntemi; kılavuz ve maske geçici klasörde Python ile). Maske alt ortada, hedef boyda bir dikdörtgen: kedi **22 en x 20 boy**, küçük köpek **24 en x 22 boy**; tabanı alt kenardan 2 px yukarıda. `view="high top-down"`, `outline="single color black outline"`, `detail="medium detail"`, `shading="basic shading"`. Stil referansı: eski görselin SE yönü (`iso_animal_cat_1_cand_se.png`, `iso_animal_dog_lapdog_cand_se.png`), görünüm aynı kalsın diye.
2. Açıklama metni:
   - kedi: `small farm cat seen straight from the front facing the viewer, brown tabby striped coat, white paws, standing on all four legs`
   - küçük köpek: `small medieval lapdog seen straight from the front facing the viewer, fluffy cream coat, short legs, curled tail, standing on all four legs`
   - Ortak ek (106): `, small black eyes, earthy muted medieval palette, desaturated, flat simple shading, light from top-left, no ground, no shadow, no dirt under feet, transparent background, no dithering, no noise`
   - Ek (375): `, standing directly on transparent background, no ground disc, no floor shadow, no grey patch under the paws`
3. **Güney ve zemin kontrolü:** 106 kuralları (yüz önden, kayma ≤3 px; zemin lekesi varsa teslim yok).
4. **v3 döndürme:** `create_character`, `mode="v3"`, `n_directions=8`, `body_type="quadruped"`, şablon kedi `cat`, köpek `dog`; referans 1. adımın 32x32 karesi (tuval küçültülmeden). `size` verme.
5. **Boy ölçümü (karar noktası):** döndürmeden sonra 8 yönün pivot üstü boyu `python tools/art/scale_check.py <dosyalar>` ile ölçülür.
   - Hedef içindeyse devam.
   - v3 hayvanı yeniden tuvali dolduracak kadar büyüttüyse (boy > hedef): **bir kez** referansı 40x40 saydam tuvalin alt ortasına yerleştirip (piksel ölçeği değişmeden; yalnızca boş kenar eklenir) döndürmeyi yinele. Yine tutmazsa dur, raporda iki ölçümü yaz ve `## Sorular` ekle (yeniden örnekleme yapılmaz).
6. Renk: `python tools/art/style.py fit --family hayvan <girdi> <çıktı>` (ücretsiz), sonra `check`.

### Kırpma kuralı (ücretsiz; bütün ölçek setleri 780-787 için aynı)
Kod bu setlerin pivotunu tuvalden alır (`IsoArt.Foot`): **yatayda tuvalin ortası, dikeyde alttan 1 px**. Bu yüzden:
- Bir gövdenin bütün kareleri (8 yön, sonra gelecek animasyon kareleri de) **aynı tuval boyunda** olur.
- Tuval: 8 yönün birleşik dolu kutusu; yatayda kaynağın pivot sütununa (SE karesinin ayak orta noktası) göre simetrik genişletilir, eni çift sayıya yuvarlanır; üstte 1 px boşluk.
- Durma karesinin en alt dolu satırı (ayak tabanı) tuvalin alttan 2. satırı olur (alttan 1 px boşluk).
- Sonuç tuval boyutu rapora yazılır; animasyon işi (783) aynı boyu kullanır.

### Dosya adı
`Assets/Art/Iso/_aday/iso_animal_<gövde>_v2_<yön>.png` (`_cand` yok; `cp -n`, `.meta` yazma, var olan dosyanın üzerine yazma).

### Karakter kimlikleri
Döndürülen iki karakterin PixelLab kimliği rapora yazılır: 783 (animasyonlar) bunları kullanır.

## Doğrulama
- `python tools/art/scale_check.py <8 yön>`: kedi 16-24, küçük köpek 18-26 (TAMAM olmayan yön teslim edilmez).
- `python tools/art/validate_iso.py --kind object <dosya>`, `python tools/art/style.py check --family hayvan <dosya>` (KALDI teslim edilmez).
- Yan yana sayfası (`art/results/780-yanyana.png`, 4 kat): 84 px yetişkin (`iso_char_man_r84_idle_se`), eski kedi ve köpek, yeni kedi ve köpek, 64x32 karo konturu; hepsi aynı zemin çizgisinde.
- Kimlik: yeni kedi eski kediyle aynı tür ve renkte görünmeli (tekir, beyaz ayak).

## Notlar
- Masaüstü kopyası: 106 kuralı (yerelde). Bulutta atlanır.
- Kod hazır (PR feat/hayvan-olcek-v2): `IsoArt.Animal` ve `AnimalBody` önce `_v2` setine bakar; dosya gelince oyunda kendiliğinden kullanılır.
- Arşiv: iş `accepted` olunca kullanıcı `python tools/art/archive_superseded.py --job 780 --apply` çalıştırır (eski görseller `_arsiv/`e taşınır, silinmez).

<!-- İşçi bundan sonrasına dokunmaz; kullanıcı kararı:
status: review → accepted ya da rejected yap, rejected ise nedenini buraya yaz. -->
