unity-import: pending
# 012 Karakter seti K2 (8 karakter, 56 px): sonuç

- İş: 012, durum `review`. Görsel onay kullanıcıda; 013 bu onaya bağlı.
- Tarih: 2026-11-08. Çalışma tavanı 20 gen (kullanıcı kararı), harcanan 16,8.

## Araç ve ayarlar
**1. Üretim.** `create_character`, 8 çağrı, sırayla (her biri bitmeden sonraki başlatılmadı).
- `mode=v3`, `body_type=humanoid`, `view=high top-down` (2:1 izometriğin ~30°'sine en yakın), `outline=single color black outline`, `detail=high detail`. Referans yok (tarif öyle).
- Açıklama: tarifteki karakter metni + ortak ek, birebir.
- `size`: yetişkin 60, yaşlı 56, çocuk 40 (tarif).
- Çağrıdan önce okunan maliyet: "v3 from scratch ... 2 for 16-90". Tahminle aynı.

**2. Dolgu (yaşlı ve çocuk).** `tools/art/char_pad.py` (bu işte eklendi, PixelLab'e dokunmaz).
- Ölçekleme yok, yalnızca saydam piksel eklenir; piksel ve alfa birebir kopya.
- Yatay: üretici tuvali 60'ta ortalanır (56 için +2, 40 için +10). Böylece üreticinin yön başına yatay yerleşimi korunur.
- Dikey: en alt opak satır, farmer_a2_56'nın aynı yöndeki en alt opak satırına (s/se/e/w/sw: y=58, ne/n/nw: y=57) getirildi.
- Aynalama yok. Yetişkinler (60x60) olduğu gibi kaldı.

**3. Renk azaltma.** `reduce_colors`, karakter başına bir çağrı, 8 yön birlikte (s, se, e, ne, n, nw, w, sw sırası), `num_colors=32`, `dithering=none`. Girdi dolgulu ham karelerdi; `get_upload_url` ile yüklenip `upload:<id>` olarak verildi, base64 yok. Çıktı sırası girdiyle aynı; her karenin alfası aynı adlı ham kareyle bayt bayt eşleşti (64/64).

| Çağrı | size | Id | Generation |
|---|---|---|---|
| create_character woman_a | 60 | cecb26b5-b874-4832-9222-12d31468c7d7 | 2 |
| create_character elder_m | 56 | 25cb8b0e-fb1a-403d-86b1-e47053392a0d | 2 |
| create_character elder_f | 56 | 250aed2e-2304-4fa1-8385-a8f56c85d7fc | 2 |
| create_character boy_a | 40 | 264e22ee-3ff5-42e9-9dc8-006b13b9b051 | 2 |
| create_character girl_a | 40 | d84f5155-87ee-482c-acce-8a521c508acb | 2 |
| create_character woodcutter_a | 60 | ba46cda4-1f24-4921-9722-415930459cfe | 2 |
| create_character farmer_f | 60 | 17a2c70c-6603-4a05-83f7-c4de39537354 | 2 |
| create_character craftsman_a | 60 | 37f77e9a-78f7-44dd-8d14-57631f8992ca | 2 |
| reduce_colors woman_a | 60 | 0960799f-9152-4264-9e08-d30608c11935 | 0,1 |
| reduce_colors elder_m | 60 | 192b3a05-6238-4327-baba-c5c3959d08a4 | 0,1 |
| reduce_colors elder_f | 60 | f80a6174-0d6d-47c2-8ef6-7fd7be098d92 | 0,1 |
| reduce_colors boy_a | 60 | 749dc7c9-e527-4204-bccb-5b0218cc58d9 | 0,1 |
| reduce_colors girl_a | 60 | 0c146d14-bd44-4cd6-b3b3-9b27d748dca9 | 0,1 |
| reduce_colors woodcutter_a | 60 | 90a395f7-8948-4b0a-b01a-6164a641ff22 | 0,1 |
| reduce_colors farmer_f | 60 | 8b222fd8-5f8c-4adb-8a67-4a47d5eeddf3 | 0,1 |
| reduce_colors craftsman_a | 60 | 46546d8d-ffd8-454c-ae56-096126405a5a | 0,1 |
| **Toplam** | | | **16,8** (cost_est 16,8, cost_max 20) |

Bakiye (`get_balance`): iş öncesi 1890,6, sonrası 1873,8. Fark 16,8 (bu çalışmada başka oturumun harcaması görünmüyor). Eşzamanlı iş sınırına takılmadı, ret yok.

## Dosyalar
Hiçbir `.meta` yazılmadı; `cp -n`, üzerine yazma yok. Hepsi 60x60.
- Renk azaltılmış: `Assets/Art/Iso/_aday/iso_char_<ad>_56_cand_{s,se,e,ne,n,nw,w,sw}.png`, 8 karakter, 64 dosya.
- Ham (dolgulu): `Assets/Art/Iso/_aday/_ham/iso_char_<ad>_56_cand_<yön>.png`, 64 dosya.
- `<ad>`: woman_a, elder_m, elder_f, boy_a, girl_a, woodcutter_a, farmer_f, craftsman_a.
- Betik: `tools/art/char_pad.py` (yeni). Ölçüm ve önizleme: `tools/art/char_preview.py` (değişmedi).

**Masaüstü (kullanıcının istisnası):**
- `C:\Users\User\OneDrive\Desktop\art\karakter\2026-11-08_<ad>\`: 8 renk azaltılmış yön, `_onizleme_x4.png`, `_ham\` altında 8 ham yön.
- Not: `_onizleme_x4.png` önce yanlışlıkla `_ham\` altına da kopyalandı; masaüstünde silme yasağı nedeniyle orada fazladan bir kopya duruyor (içerik aynı). Silmek kullanıcının kararı.
- `C:\Users\User\OneDrive\Desktop\art\karakter\2026-11-08_K2_toplu_x4.png`: köylü, farmer_a2_56 ve 8 karakterin SE kareleri tek satırda.
- `C:\Users\User\OneDrive\Desktop\art\_indeks.md`: sonuna 8 satır.

## Doğrulama: SE (renk azaltılmış)
Pivot x/y 010/011 yöntemi; y ALTTAN.

| Karakter | Hedef | Gerçek boy | ±3 | Genişlik | Renk ham → azaltılmış | Yarı saydam | Pivot x | Pivot y |
|---|---|---|---|---|---|---|---|---|
| woman_a | 58 | 58 | tuttu | 27 | 62 → 32 | 0 | 30.5 | 1 |
| elder_m | 54 | 54 | tuttu | 31 | 50 → 31 | 0 | 31.0 | 1 |
| elder_f | 54 | 54 | tuttu | 32 | 50 → 32 | 0 | 31.0 | 1 |
| boy_a | ~39 | 38 | tuttu | 18 | 47 → 31 | 0 | 30.5 | 1 |
| girl_a | ~39 | 38 | tuttu | 19 | 34 → 31 | 0 | 31.0 | 1 |
| woodcutter_a | 58 | 58 | tuttu | 39 (balta dahil) | 52 → 32 | 0 | 32.0 | 1 |
| farmer_f | 58 | 60 | tuttu (+2) | 35 | 51 → 32 | 0 | 31.5 | **0** |
| craftsman_a | 58 | 58 | tuttu | 35 | 64 → 32 | 0 | 31.5 | 1 |
| farmer_a2_56 (ref.) | 56 | 58 | — | 32 | 57 → 32 | 0 | 31.5 | 1 |

- Çocuklar yetişkinin %65,5'i (38/58), hedef %65-70 içinde. Yaşlılar 54, yetişkinden 4 px kısa.
- `validate_iso.py <SE> --kind object --max-colors 32`: 8 SE dosyasının hepsi geçti. farmer_f "alt boşluk 0 satır" ile geçti.

## Doğrulama: 8 yön
Renk (azaltılmış) / yarı saydam: 8 karakterin 64 karesinde renk 13-32, yarı saydam 0. Her karakterin 8 yönü tek ortak palet kullanıyor (elder_m 31, diğerleri 32).

Pivot y (ALTTAN), sıra s, se, e, ne, n, nw, w, sw:

| Karakter | s | se | e | ne | n | nw | w | sw |
|---|---|---|---|---|---|---|---|---|
| woman_a | 2 | 1 | 1 | 3 | 3 | 3 | 1 | 1 |
| elder_m | 1 | 1 | 1 | 2 | 2 | 2 | 1 | 1 |
| elder_f | 1 | 1 | 1 | 2 | 2 | 2 | 1 | 1 |
| boy_a | 1 | 1 | 1 | 2 | 2 | 2 | 1 | 1 |
| girl_a | 1 | 1 | 1 | 2 | 2 | 2 | 1 | 1 |
| woodcutter_a | 1 | 1 | 1 | 2 | 3 | 2 | 1 | 1 |
| farmer_f | 1 | 0 | 0 | 2 | 3 | 2 | 0 | 0 |
| craftsman_a | 1 | 1 | 1 | 2 | 3 | 2 | 1 | 1 |
| farmer_a2_56 (ref.) | 1 | 1 | 1 | 2 | 2 | 2 | 1 | 1 |

Dolgulu dört karakter (yaşlılar, çocuklar) tanım gereği referansla aynı. Yetişkinler üreticiden geldiği gibi; 0-3 px oynuyor. Yön başına pivot önerisi (010/011) geçerli.

Diğer yönlerde boy: woman_a 55-58, elder_m 52-54, elder_f 53-54, boy_a 38, girl_a 37-38, woodcutter_a 56-58, farmer_f 57-60, craftsman_a 57-58.

## Tunik tonu (SE, gövde bandı baskın renk)
| Karakter | Ana giysi | Paletten sapma |
|---|---|---|
| woman_a | #817060 gri-kahve elbise; #cbbb9d keten önlük | Yok. "Yeşil-kahve" istendi, gözle gri-zeytin kahve okunuyor. |
| elder_m | #7b502a kahve yün tunik; gri kukuleta/pelerin | Yok (s 0,66, koyu). |
| elder_f | #744038 / #402127 koyu kırmızı-kahve elbise; #84775b şal | Yok. |
| boy_a | #ebdeaa açık bej tunik | Hafif: setin en parlak giysisi (v 0,92). Doygun değil. |
| girl_a | #7a8a95 mavi-gri elbise | Yok; setteki tek soğuk ton, tarifte istendi. |
| woodcutter_a | #535520 koyu zeytin tunik | Yok (koyu, s 0,62). |
| farmer_f | #c8b798 krem bluz; kahve etek; hasır şapka | Hafif: "okra bluz" istendi, krem-bej çıktı. |
| craftsman_a | #774b28 deri önlük; zeytin yelek; #727c84 mavi-gri kollar | Hafif: "gri-kahve kollar" istendi, mavi-gri çıktı. |

Referans farmer_a2_56 tuniği #c98f2b doygun okra; yeni sekizin hiçbiri ondan daha doygun ya da parlak değil.

## Ayırt edilebilirlik (toplu görsel)
- **Boy ve yaş:** üç kademe net: yetişkin 58-60, yaşlı 54 (kambur, pelerinli/şallı geniş silüet), çocuk 38 (iri baş, kısa gövde).
- **Silüet:** farmer_f geniş hasır şapka ve sepet; elder_m değnek ve kukuleta; elder_f başörtülü yuvarlak şal silüeti; woodcutter_a geniş omuz ve elde balta; craftsman_a önlük; woman_a topuz.
- **Renk:** girl_a mavi-gri, boy_a açık bej, elder_f kırmızı-kahve, woodcutter_a koyu zeytin ile ayrışıyor.
- **Benzer çiftler:**
  - woman_a ile farmer_f: ikisi de krem önlük + kahve; ayıran yalnızca şapka ve sepet. Şapkasız uzak görünümde karışabilir.
  - woodcutter_a ile craftsman_a: ikisi de koyu saçlı, iri gövdeli, zeytin üst. Sakal/balta ve deri önlük ayırıyor; craftsman_a "slim" istendi ama woodcutter kadar iri çıktı.
  - boy_a ile girl_a: renkte ayrışıyor, silüette çok yakın.

## Tutarlılık (8 yön)
- woman_a: topuz, önlük, kese 8 yönde var. Önlük arka yönlerde (ne, n, nw) görünmüyor, doğal.
- elder_m: değnek 8 yönde var; s/se/sw'de ekranın solunda, e/ne/n'de sağında, nw/w'de omuz arkasında kısmen görünüyor. Sakal ve kukuleta tutarlı.
- elder_f: başörtüsü ve şal tutarlı. Sepet yok; yerine yan çanta (s, se, e, w, sw). Arka yönler (ne, n, nw) neredeyse yalnızca şal: n karesi 13 renk.
- boy_a: tutarlı. "Yırtık yaka" ve "yamalı pantolon" 38 px'te seçilmiyor.
- girl_a: tutarlı. Örgü yalnızca arka yönlerde (ne, n, nw) iki saç kuyruğu olarak okunuyor; önden düz uzun saç.
- woodcutter_a: balta 8 yönde var ama **elde taşınıyor, kalçada kılıfta değil**. Balta başı yer yer çift ağızlı/kazma gibi okunuyor. **Ten açık-orta (yüz #e59864, gölge #a6543b) çıktı; "dark skin" tutmadı.**
- farmer_f: şapka ve sepet 8 yönde var; sepet hep sol kolda. **Şapka tuvalin üst satırına (y=0) değiyor** (s, se, sw, n; satır 0'da 5 opak piksel) ve se/e/w/sw'de ayak alt satırda (pivot y 0). Kesik görünmüyor ama tuvalde pay yok.
- craftsman_a: önlük, kemer, cep aletleri tutarlı; tıraşlı.
- Hepsinde gölge pişirilmemiş, zemin saydam. Işık sol-üstten (sol omuz açık, sağ yan koyu).

## Sorunlar ve sorular
1. **woodcutter_a teni** istenenden açık; balta elde. Tek deneme kuralı nedeniyle yeniden üretilmedi.
2. **farmer_f** 60 px, tuvalin dört satırını da kullanıyor (üstte şapka, altta ayak). Kabul edilirse Unity'de taban payı 0.
3. **woman_a/farmer_f** ve **woodcutter_a/craftsman_a** çiftleri yakın; renk ayrımı istenirse düzeltme (ör. renk kaydırma) ayrı iş olur.
4. Tarifte elder_f metni "small basket or none" içeriyordu; metin olduğu gibi gönderildi, üretici sepet yerine çanta çizdi.
5. Masaüstünde her karakterin `_ham\` klasöründe fazladan `_onizleme_x4.png` kopyası var (yukarıda).
