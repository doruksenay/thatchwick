unity-import: pending

# 579 Servis kapları 1/2 (rapor): 2/6 birleşim teslim (kazan, dolma tepsisi)

Yan yana sayfası: [579-yanyana.png](579-yanyana.png) (10 yeni kare + 7 referans). Doluluk şeridi (4→0): [579-serit.png](579-serit.png) (üst: kazan, alt: dolma). Teslim edilmeyenler: `art/results/579-red/` (`pot-serit.png`, `roast-serit.png`, `fish-serit.png`). Masaüstü kopyası: yerel.

## Araç ve ayarlar
- 578 ile aynı yöntem: `create_map_object`, stil eşleme, `high top-down`, `basic shading`, `low detail`, `single color outline`, tarif açıklaması + ortak ek. Tuval 32x32; kılavuz tabanda 24x12 elmas (#786046), maske tabanı kapsayan dikdörtgen. 6 `_4` çağrısı paralel.
- `_3…_0`: `_4` kare arka plan, yalnızca kabın içi (elips maske); araç yalnızca maskeli nesneyi döndürdüğü için kap `_4`'ten alındı, iç üretilen kareden elle kompozitlendi (kap 5 karede birebir aynı; tarifteki "kabı elle eşitle").
- Başarısız/tekrar: börek tahtası `_4` boş döndü ("every pixel transparent", ücret yok); tarifteki tek deneme kuralı gereği yeniden denenmedi, börek birleşimi yok. Dolma/kızartma/balık arka plan verisinde aktarma hatası (kesilmiş base64, çağrı reddedildi, ücret yok): veri 6-16 renge indirilip aynı çağrı yeniden gönderildi (üretim hatası değil, taşıma hatası).
- Toplam: 25 başarılı çağrı ≈ 25 generation (cost_est 30, cost_max 60). Bakiye (`get_balance`, paralel şeritler dahil): 1861,7.
- Kazan `_2`: ailesinden çok doygun (x1,51, KALDI) → `style.py fit` x0,76 → GEÇTİ. Ham hâli `579-red/cauldron_2_ham.png`. Kap rengi bu karede diğer dört kareye göre hafif farklı (alfa farkı yine 0).

## Sonuç
| Birleşim | Durum | Not |
|---|---|---|
| kazan çorba `_4…_0` | teslim | `_3…_1` krem çorba yüzeyi düz disk; doluluk farkı zayıf okunuyor, `_0` boş koyu iç net. `_4`'te kenarda turuncu parıltı artığı var (araç çıktısı) |
| dolma tepsisi `_4…_0` | teslim | dolu→bakır; sıra okunuyor (`_3` seyrek, `_2` dağınık koyu, `_1` tek parça, `_0` düz bakır) |
| güveç `iso_serve_pot_soup` | TESLİM EDİLMEDİ | `_3…_1` hepsi dolu kırmızı disk, seviye düşüşü okunmuyor |
| kızartma tepsisi | TESLİM EDİLMEDİ | `_3` pembe düz blok (yanlış nesne), `_2` karışık koyu leke |
| balık tabağı | TESLİM EDİLMEDİ | `_4` balık tanınmıyor (gri-beyaz lekeler), `_3` kırmızı turuncu blok, tabağa uymuyor |
| börek tahtası | ÜRETİLMEDİ | `_4` boş döndü, tek deneme kuralı |

## Dosyalar (Assets/Art/Iso/_aday/)
| Yol | Piksel | Pivot x | Pivot y (alttan) |
|---|---|---|---|
| iso_serve_cauldron_soup_{4,3,2,1,0}.png | 32x32 | 16 | 10 |
| iso_serve_tray_dolma_{4,3,2,1,0}.png | 32x32 | 16 | 10 |

Pivot = taban elmasının merkezi (578 ve fruit_basket ile aynı kural).

## Doğrulama
| Dosya | validate_iso (--max-colors 48) | Stil (esya) | Kap alfa farkı (`_4`'e göre) |
|---|---|---|---|
| cauldron_4 | geçti | UYARI (x1,46 doygun) | — |
| cauldron_3 | geçti | UYARI (x1,45) | 0 px |
| cauldron_2 | geçti | GEÇTİ (fit x0,76; ham x1,51 KALDI) | 0 px |
| cauldron_1 | geçti | UYARI (x1,29) | 0 px |
| cauldron_0 | geçti | GEÇTİ (x1,18) | 0 px |
| dolma_4 | geçti | GEÇTİ (x1,23) | — |
| dolma_3 | geçti | UYARI (x1,30) | 0 px |
| dolma_2 | geçti | UYARI (x1,31) | 0 px |
| dolma_1 | geçti | GEÇTİ (x1,21) | 0 px |
| dolma_0 | geçti | UYARI (palete uzak, x1,34) | 0 px |

Kap dış çizgisi 5 karede birebir (tarif: en çok 1 px). Zemin değil, döşeme testi yok. Buhar, parlama, gölge, el yok. Işık sol-üstten.

## Sorunlar ve öneriler
- Maskeli iç üretim yöntemi seviye düşüşünü güvenilir vermiyor (578'de de aynı). Öneri: kalan dört birleşim (güveç, kızartma, balık, börek) için yöntem değişikliği: `_4`'ü tuvalde küçültülmüş yemek katmanlarıyla ayrı üretmek (yemek tek başına, kapsız) ve kapla kompozitlemek; ya da kullanıcı kararıyla 580 (2/2) ile birlikte bir yeniden iş açılması.
- Araçta `seed` yok, maske dışı dondurma yok. Büyük base64 argümanı uzun karelerde kesiliyor; renk sayısı düşürmek çözdü.
- Kullanıcı `579-serit.png` ve `579-yanyana.png`'ye bakmalı; kazan `_3…_1` farkı zayıf.
