unity-import: pending

# 395c Dördüncü tur: çatkı, kandil, kömür yığını (rapor)

Sonuç: **hiçbir parça teslim edilmedi** (`Assets/Art/Iso` altına dosya konmadı). Yan yana sayfası: [395c-yanyana.png](395c-yanyana.png). Aday dosyalar: `art/results/395c-red/`. Masaüstü kopyası: yerel (bulutta yapılamaz).

## Araç ve ayarlar
- Çatkı: `inpaint_image_pro_flash` (80x96, mask 14,44 50x52, tarifteki metin, `no_background`). 1 çağrı, 5 gen.
- Kandil: `create_map_object` stil eşleme, `background_image` = mum (48x56), oval %60, `low top-down`. Hız sınırı hatası bir kez, bekleyip yeniden denendi. Sonra yerel 32 renk + `fit` (0 gen; base64 kısıtı yüzünden `reduce_colors` aracı yerine yerel).
- Kömür yığını: `create_map_object` stil eşleme, `background_image` = kireç ocağı (base64 uzunluğu için 24 renge indirilmiş bağlam kopyası), oval %70. Şekil tutmadığı için renk indirme yapılmadı.
- Harcama: yaklaşık 15 (çatkı 5 kesin; iki nesne çağrısı ayrı okunamadı, bakiye diğer şeritlerle ortak). `get_balance` sonrası: 4067,70.

## Sonuç
| Parça | Durum | Neden |
|---|---|---|
| ahşap çatkı | TESLİM EDİLMEDİ | stil GEÇTİ (%99, x1.03) ama çapraz kafes ve koyu bordo dikmeler duruyor; "iki düz taze dikme + kiriş" ölçütü karşılanmadı |
| kandil | TESLİM EDİLMEDİ | küçük kil kandil yerine büyük alevli ateş çanağı çıktı; stil KALDI (%80, x1.10) |
| kömür yığını | TESLİM EDİLMEDİ | kubbe yığın yerine üstü süslü davul/kütük şeklinde; stil UYARI (%87, x0.82); şekil tanınmıyor |

## Stil denetimi (`style.py check --family esya`)
| Dosya | Palete yakın | Ort. uzaklık | Doygunluk | Sonuç |
|---|---|---|---|---|
| support_wood_d | %99 | 5.9 | x1.03 | GEÇTİ (şekil KALDI) |
| mine_lamp_d (32 renk + fit) | %80 | 18.4 | x1.10 | KALDI |
| charcoal_clamp_d | %87 | 11.4 | x0.82 | UYARI (şekil KALDI) |

`validate_iso.py`: kandil geçti (48x56, alt boşluk 11 satır).

## Sorular
- Çatkı: iki ardışık inpaint kafesi kaldırmadı; öneri: yeni `create_1_direction_object` ya da workbench ile elle temiz dikme çizimi.
- Kandil: mum bağlamı alev/çanak üretiyor; öneri: `create_1_direction_object` ile "clay oil lamp" ya da daha küçük maske.
- Kömür yığını: kireç ocağı bağlamı davula yönlendirdi; öneri: bağlamsız `create_map_object` (basic) ya da düz toprak zemin üzerinde maske.
- Araç notu: `create_map_object` `background_image` yalnızca satır içi base64 alıyor; `upload:` kabul etmiyor.
