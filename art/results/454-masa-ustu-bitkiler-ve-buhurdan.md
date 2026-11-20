unity-import: pending

# 454 Masa üstü saksılar, eğrelti teknesi, buhurdan, kuş yalağı (rapor)

Yan yana sayfası: [454-yanyana.png](454-yanyana.png) (5 teslim + 7 referans). Teslim edilmeyen: `art/results/454-red/` (ham ve `fit_` hâlleri). Masaüstü kopyası: yerel.

## Araç ve ayarlar
`create_map_object`, stil eşleme, referanssız kılavuz + maske (383 yöntemi, Python ile; taban elması alt kenardan 4 px yukarıda: 16x8 küçük saksı ve buhurdan, 24x12 zambak ve lavanta, 32x16 tekne ve yalak; maske eşya yüksekliği + 1 px). `high top-down`, `low detail`, `basic shading`, `single color outline`, tarif açıklaması + 301 ortak eki. 12 çağrı x 1 gen = 12 generation (cost_max 20), ikinci deneme yok, hız sınırı hatası yok. Tuvaller: 32x40, 40x48, 48x40 (tekne), 48x56 (yalak). `get_balance` iş sonu: 3074,90 (paralel şeritler de harcıyor; bu işin payı 12).
Doygunluk düzeltmesi (`style.py fit`): kuş yalağı x0.77, zambak x0.72, nane x0.69. Ham hâller `454-red/ham_*`.

## Sonuç özeti
| # | Eşya | Durum | Not |
|---|---|---|---|
| 1 | zambak saksısı | teslim (fit x0.72) | UYARI: palete uzak |
| 2 | nergis saksısı | teslim | GEÇTİ |
| 3 | menekşe saksısı | TESLİM EDİLMEDİ | palet %46 (mor çiçek); fit sonrası da KALDI (%63) |
| 4 | siklamen saksısı | TESLİM EDİLMEDİ | ham 49 renk (>48) ve palet %72; fit sonrası palet KALDI |
| 5 | kardelen saksısı | teslim | UYARI: soluk x0.60 |
| 6 | çiğdem saksısı | TESLİM EDİLMEDİ | yalnızca palet (%71); doygunluk sorunu yok, fit uygulanmadı |
| 7 | lavanta saksısı | TESLİM EDİLMEDİ | palet %43; fit sonrası %44 KALDI |
| 8 | nane saksısı | teslim (fit x0.69) | UYARI: hâlâ doygun (x1.34) |
| 9 | papatya saksısı | TESLİM EDİLMEDİ | yalnızca palet (%76) |
| 10 | eğrelti teknesi | TESLİM EDİLMEDİ | palet %51; fit sonrası %58 KALDI |
| 11 | buhurdan | TESLİM EDİLMEDİ | yalnızca palet (%79) |
| 12 | kuş yalağı | teslim (fit x0.77) | UYARI: palete uzak |

## Dosyalar (Assets/Art/Iso/_aday/)
Pivot: küçük taban elmasının merkezi (alttan 4 + elmas yüksekliği/2).
| Yol | Piksel | Pivot x | Pivot y (alttan) |
|---|---|---|---|
| iso_item_pot_lily_se.png | 40x48 | 20 | 10 |
| iso_item_pot_narcissus_se.png | 32x40 | 16 | 8 |
| iso_item_pot_snowdrop_se.png | 32x40 | 16 | 8 |
| iso_item_pot_mint_se.png | 32x40 | 16 | 8 |
| iso_item_birdbath_se.png | 48x56 | 24 | 12 |

## Doğrulama (validate_iso, `--max-colors 48`)
Teslim edilen 5 görsel geçti: yarı saydam 0, alta değme yok (alt boşluk 3-6 satır), renk 21-45. Opak sınırlar: zambak x12-27 y4-44; nergis x7-24 y4-36; kardelen x7-24 y4-34; nane x7-24 y4-36; yalak x8-39 y7-49.

## Stil denetimi (`style.py check --family esya`)
| Dosya | Palete yakın | Ort. uzaklık | Doygunluk | Sonuç |
|---|---|---|---|---|
| nergis | %93 | 8.3 | x0.87 | GEÇTİ |
| kardelen | %81 | 10.9 | x0.60 | UYARI |
| yalak (fit) | %92 | 12.3 | x1.15 | UYARI |
| zambak (fit) | %86 | 11.0 | x1.16 | UYARI |
| nane (fit) | %84 | 8.4 | x1.34 | UYARI |
| menekşe (teslim değil, fit) | %63 | 19.4 | x1.15 | KALDI |
| siklamen (teslim değil, fit) | %73 | 14.1 | x1.15 | KALDI |
| lavanta (teslim değil, fit) | %44 | 20.5 | x1.15 | KALDI |
| eğrelti (teslim değil, fit) | %58 | 20.0 | x1.15 | KALDI |
| çiğdem, papatya, buhurdan (ham) | %71-79 | 14.6-17.6 | x1.02-1.15 | KALDI (palet) |

## Sorunlar
- 7 parça stil denetiminde palet dışı renk nedeniyle teslim edilmedi (çiçekler doğal olarak doygun). Yeni iş (454b) gerekir: "muted dusty" çiçek renkleri ve "terracotta pot, dark green leaves" palet ipucu; ya da kullanıcı `art/results/454-red/` dosyalarını gözle değerlendirir.
- Zambak ilk indirmede 423 (Locked) verdi: iş bitmeden indirildi, bitince yeniden indirildi; ek üretim yok.
- Döşeme testi (zemin değil) ve ayna uygunluğu bu işte uygulanmadı.
