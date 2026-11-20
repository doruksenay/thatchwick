---
id: "461"
title: "P2 baş parçası pilotu: 2 saç kütlesi, 1 ön, 1 sakal, 1 başlık, 5 yön"
status: pending
approved_by: pending     # kullanıcı kararı 2026-11-19: kota sıfırlanınca (2026-12-18); o gün user yap
family: karakter
style_gate: true
cost_est: 40
cost_max: 60
depends_on: ["460a", "460b"]
max_attempts: 1
pixellab: true
outputs:
  - Assets/Art/Iso/katman/lay_mass_short_<yön>_fr.png
  - Assets/Art/Iso/katman/lay_mass_shoulder_<yön>_{fr,bk}.png
  - Assets/Art/Iso/katman/lay_front_straight_<yön>_fr.png
  - Assets/Art/Iso/katman/lay_beard_full_beard_<yön>_fr.png
  - Assets/Art/Iso/katman/lay_hat_straw_<yön>_{fr,bk}.png
  - art/results/461-bas-parcasi-pilotu.md
created: 2026-11-20
---
# 461 P2 baş parçası pilotu

## Amaç
Belge 43 §4.2 katman B: saç, sakal ve başlık her kare için değil, yalnız 5 yönde (s, se, e, ne, n) çizilir; oyun her karede baş çapasına oturtur, batı yönleri aynalar. Bu pilot yaklaşımın tutup tutmadığını gösterir. Tutarsa 43b'deki bütün baş parçaları (16 kütle, 14 ön, 12 eklenti, 10 süs, 14 bıyık, 18 sakal, 5 favori, 16 başlık) aynı tarifle üretilir.

## Tarif
- **Taban:** 460a/460b'nin durma kareleri, 5 yön: `Assets/Art/Iso/_ham/katman/lay_src_{m,f}_orta_idle_{s,se,e,ne,n}_00.png`. Saçlar kadın, sakal erkek mankende; başlık ikisinde de denenir (ayrı dosya değil, erkek yeter).
- **Araç:** `inpaint` (020 portre yöntemi: `output_method="Modify current layer, only changes"`), maske başın çevresi (betikle: baş çapasından 24 px yukarı, 40 px aşağı, 20 px sağa/sola; uzun saçta 60 px aşağı).
- **Renk (bağlayıcı):** saç ve sakal referans saç rampasında çizilir: `#3B2213 #5C3519 #7A4B25 #A4793D` (`python tools/art/ramps.py` REFERENCE hair); başlık gri rampada: `tools/art/ramps.py` REFERENCE cloth. Açıklama eki: `, drawn only in these brown tones, will be recoloured in game, dark outline #2A1E1E, pixel art, no dithering`.
- Parçalar:
  1. `mass short`: `short cropped hair close to the scalp, straight` (kadın ve erkek mankende aynı baş; erkek yeter).
  2. `mass shoulder`: `wavy shoulder-length hair falling to the shoulders`.
  3. `front straight`: `straight-cut fringe across the forehead` (short kütlesi uygulanmış kare üstüne).
  4. `beard full_beard`: `full thick beard covering the jaw and chin`.
  5. `hat straw`: `wide straw hat`.
- **Çıkarma (0 gen):** her inpaint sonucu için `python tools/art/manken.py extract --base Assets/Art/Iso/katman/lay_body_<m|f>_orta_idle_<yön>_00.png --src <lay_src kare> --edited <sonuç> --out-dir Assets/Art/Iso/katman --name lay_<yuva>_<kimlik>_<yön>`.

## Doğrulama
- `python tools/art/ramps.py check --ref hair` (saç, sakal) ve `--ref cloth` (başlık): GEÇTİ şart.
- Unity'de (kullanıcı): yaratma ekranında kısa saç, omuz boyu saç, kâkül, sakal ve hasır şapka seçilince karakter 8 yönde ve yürürken doğru görünüyor; saç kaymıyor; K ve KD yönlerinde ön/arka katman doğru; batı yönleri aynada doğru.
- Dört beden (460c) üstünde saç aynı yere oturuyor.

## Notlar
- Tuval sözleşmesi: 64x96, çapa (32, 24) = kel kafanın en üst pikseli (`LayerTypes.cs` PartCanvas).
- Dosya adı yuvaları: mass, front, extra, orn, must, beard, side, hat.

<!-- İşçi bundan sonrasına dokunmaz; kullanıcı kararı:
status: review → accepted ya da rejected yap, rejected ise nedenini buraya yaz. -->
