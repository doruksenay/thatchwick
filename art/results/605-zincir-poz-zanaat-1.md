unity-import: pending

# 605 Zincir pozları, zanaat 1/3: saw, sew, spin, weave, bellows (84 px, erkek ve kadın)

- Araç: `animate_character`, `mode=pixminimax`, `frame_count=8`, `keep_first_frame=true`, `end_frame_url` = ilgili yönün durma karesi (karakter rotasyon görseli, https adresi). Tarifteki eylem metinleri aynen. 2 karakter x 5 anahtar x 4 yön = 40 çağrı, çağrı başına 1 generation (makbuz: "1/dir x 1"), toplam **40 gen** (tahmin 40, tavan 52). Aynı anda en çok 3 çağrı; eşzamanlılık hatası olmadı, yeniden deneme yapılmadı.
- Karakterler: `man_r84` (c63fe39d-b168-4732-bf11-b476172c98e8), `woman_r84` (135cb0b1-bd46-4514-8c87-0834b96ef80d), 84x84, `completed`, 8 rotasyon.
- Gruplar (erkek): saw 7c0571df-0b67-4870-8183-166794138940, sew 6bb26383-cd54-46fe-a24e-d5073954ba23, spin 76743a30-c0fb-4ee8-b96f-23f343ad46a1, weave b79955f9-c1b1-4482-83e3-b529b45d7078, bellows 8114eae9-ad0c-4e25-887c-8f9bb34d3a7d.
- Gruplar (kadın): saw e4524e2f-2644-405c-a899-baaec6062a80, sew 5721c38a-9ade-41e7-a647-7db6562368d3, spin aa8581ba-9e03-491d-9c47-459f01a8c8d8, weave 85f0b3ac-3d6f-4919-92d0-3cb09595daa9, bellows a0e9eb70-caef-4fc7-86d1-c4bcb8a5e361.
- `get_balance` sonrası kalan: 9385 / 10000 (Tier 3, plan `art/budget.md` ile aynı; diğer şeritler de harcıyor, sayı yalnızca bu işe ait değil).
- İşleme (018c ile aynı): ham kare 84x112 ve daha geniş/yüksek; yalnızca kaydırma, ölçekleme yok. En alt opak satır y=81, yatay ortalama; alfa ikili (>=128); palet `iso_char_<cinsiyet>_r84_cand_se.png` renkleri, en yakın RGB. Kare 0 ile son kare IoU > 0,95 ise son kare atıldı. Pivot x=42, y=2 (ALTTAN).
- Yan yana sayfası: `art/results/605-yanyana.png` (2 referans + 36 klibin kare 00'ı). GIF: `art/results/605-<man|woman>-<anahtar>.gif` (yönler yan yana, 2x). Ham ve kaydırılmış reddedilenler: `art/results/605-red/`.
- masaüstü kopyası: yerel

## Teslim edilen dosyalar
`Assets/Art/Iso/_aday/anim/iso_char_<man|woman>_r84_<anahtar>_<se|sw|nw|ne>_<NN>.png`, hepsi 84x84, pivot x=42, pivot y=2 (alttan). Toplam **295 PNG, 36 klip**.

| Anahtar | Erkek | Kadın |
|---|---|---|
| saw | sw 8, ne 8 (se, nw YOK) | sw 8, nw 9, ne 8 (se YOK) |
| sew | sw 8, nw 9, ne 8 (se YOK) | se 9, sw 9, nw 9, ne 8 |
| spin | se 9, sw 8, nw 8, ne 8 | 4 x 8 |
| weave | se 8, sw 8, nw 8, ne 9 | 4 x 8 |
| bellows | 4 x 8 | 4 x 8 |

## Teslim edilmeyenler (84x84'e sığmadı, tek deneme kuralı)
Elde tutulan nesne ve salınım tuvalin üstünden/yanından taşıyor; opak piksel kırpmak kural dışı olduğundan teslim edilmedi (dosyalar `art/results/605-red/`):

| Klip | Taşan opak piksel |
|---|---|
| erkek saw SE | 754 |
| erkek saw NW | 6 |
| erkek sew SE | 82 |
| kadın saw SE | 42 |

Öneri: bu dört yön için tek yönlü yeniden üretim (kullanıcı/kod oturumu karar versin) ya da 96x96 tuval kararı. Yeni üretim yapılmadı.

## Doğrulama
| Ölçüt | Değer |
|---|---|
| validate_iso (295 dosya) | 295/295 geçti (84x84, <=32 renk, yarı saydam 0) |
| style.py check karakter | KALDI 0, UYARI 62, GEÇTİ 233. UYARI yalnızca "ailesinden doygun" (x1,25-1,37; SE/SW ağırlıklı: spin_se 8, bellows_se 7, bellows_sw 7, saw_sw 7, sew_sw 7, weave_sw 7, weave_se 6, sew_ne 4, weave_ne 4, spin_sw 3, sew_nw 2). Aile kilidi kurulmadı, palet uyumu uyarı düzeyi. |
| Ayak satırı | her karede en alt opak satır y=81 |
| Kare 0 - son kare IoU | 0,721-1,0; en düşükler: erkek sew SE 0,72 (reddedildi), erkek saw NW 0,81 (reddedildi), erkek spin SE 0,84, erkek sew NW 0,85, kadın sew SE 0,86 (bunlarda son kare atılmadı, 9 kare) |
| Ardışık IoU (en düşük, teslim edilenler) | erkek sew NW 0,58; erkek saw NE 0,78; diğerleri >= 0,73 (hızlı el hareketi) |
| Tepe kırpılması | teslim edilenlerde yok (en küçük tepe y: 1) |

## Sorunlar ve notlar
- Kimlik (yüz, giysi, saç) ve elde tutulan nesne okunurluğu yalnızca yan yana sayfası ve GIF'lerden gözle bakılmalı; otomatik ölçülmedi.
- Elde tutulan nesne (testere, iğne, iplik, mekik, körük kolu) küçük ve bazı karelerde belirsiz olabilir; oyunda taşınan eşya el çapasından ayrıca çizilir.
- El konumları (`hand_anchors.txt` için): otomatik ölçülmedi, gözle okunacak; bu turda yazılmadı.
- 9 kareli klipler (erkek sew NW, spin SE, weave NE; kadın saw NW, sew SE/SW/NW): son kare başa yakın değil; döngü kapanışına bakılmalı.
- Ayak izi/tuval: PixelLab bu işte 84x112 ve daha geniş tuval döndürdü; ayak satırı sabitlendi.
- Önerilen araç ihtiyacı yok; işlem `art/tmp/` altındaki betiklerle yapıldı.
