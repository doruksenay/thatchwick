unity-import: pending

# 405 Mevsim ve hayvan bölmesi süsleri (rapor): 7/10 teslim, review

Yan yana sayfası: [405-yanyana.png](405-yanyana.png). Teslim edilmeyen: `art/results/405-red/`. Masaüstü kopyası: yerel.

## Araç ve ayarlar
`create_map_object`, stil eşleme, referanssız kılavuz + maske, `low detail`, `basic shading`, `single color outline`, 301 ortak eki. Duvar eşyaları `side`, duvar eki ve takım eki (306 gibi); yer süsleri `high top-down`. 10 çağrı x 1 gen = **10 generation** (paralel; ilk garland çağrısı base64 kopyalama hatasıyla sunucuda reddedildi, ücret yok, aynı kılavuzla yeniden gönderildi). Tek deneme.
Kılavuzlar: duvar eşyaları için `art/tmp405/mk.py` (duvar rengi (184,160,122) "/" eğimli paralelkenar, maske +1 px; commit edilmez). Dal: 32x32 dekor kılavuzu (301). Hasat yığını: `footprint_guide.py --w 80 --h 64 --n 1 --height 20`.
Doygunluk `fit`: perde (x0.67), kapı çelengi (x0.62). Ham hâlleri `405-red/`de.
`get_balance` iş sonu: 4138,9 (paralel şeritler de harcıyor; bu işin payı 10).

## Sonuç özeti
| # | Eşya | Durum | Not |
|---|---|---|---|
| 1 | bahar dalı (kil testi) | teslim | UYARI soluk (x0.54), 32x32 çok küçük |
| 2 | bahar çelengi | teslim | |
| 3 | hasat yığını | TESLİM EDİLMEDİ | stil KALDI (palete yakın %67, x1.52); şekil buğday demeti değil, balya benzeri dikdörtgen blok (balya ortaçağ sonrası). Yeni tarif gerek |
| 4 | kış yeşilliği | TESLİM EDİLMEDİ | stil KALDI (palet dışı renkler, %77); `fit` düzeltmedi |
| 5 | kapı çelengi | teslim (doygunluk fit) | UYARI palete uzak (%81); 61 renk > 48 (raporlandı); kırmızı fiyonk korundu |
| 6 | isim tabelası | teslim | |
| 7 | nal panosu | teslim | 50 renk > 48 (raporlandı) |
| 8 | eyer mandalı | teslim | UYARI palete uzak (%83); mandal ve ip okunuyor, panel yok |
| 9 | saman süs | TESLİM EDİLMEDİ | mısır koçanına benziyor (Yeni Dünya bitkisi, dönem kuralı) ve stil KALDI. Yeni tarif gerek (örgülü başak tanrıçası, "plaited wheat ears") |
| 10 | kümes perdesi | teslim (doygunluk fit) | ham x1.71 KALDI idi |

## Dosyalar (Assets/Art/Iso/_aday/; pivot = görselin merkezi, ALTTAN; dal için küçük taban elması merkezi)
| Dosya | Boyut | Pivot x | Pivot y (alttan) |
|---|---|---|---|
| iso_item_spring_branch_se.png | 32x32 | 16 | 7 |
| iso_wall_item_spring_garland_sw_ne.png | 64x48 | 32 | 24 |
| iso_wall_item_door_wreath_sw_ne.png | 48x64 | 24 | 32 |
| iso_wall_item_name_plaque_sw_ne.png | 48x48 | 24 | 24 |
| iso_wall_item_horseshoe_display_sw_ne.png | 64x48 | 32 | 24 |
| iso_wall_item_saddle_peg_sw_ne.png | 48x64 | 24 | 32 |
| iso_wall_item_coop_curtain_sw_ne.png | 64x64 | 32 | 32 |

## Doğrulama (validate_iso, `--max-colors 48`)
| Dosya | Renk | Yarı saydam | Sonuç |
|---|---|---|---|
| dal | 11 | 0 | geçti |
| çelenk (bahar) | 35 | 0 | geçti |
| kapı çelengi | 61 | 0 | renk sınırı aşıldı, raporlandı |
| tabela | 19 | 0 | geçti |
| nal panosu | 50 | 0 | renk sınırı aşıldı, raporlandı |
| eyer mandalı | 47 | 0 | geçti |
| perde | 34 | 0 | geçti |

## Stil denetimi (`style.py check --family esya`, teslim edilenler)
| Dosya | Palete yakın | Ort. uzaklık | Doygunluk | Sonuç |
|---|---|---|---|---|
| dal | %96 | 11.1 | x0.54 | UYARI |
| bahar çelengi | %91 | 9.7 | x1.05 | GEÇTİ |
| kapı çelengi (fit) | %81 | 12.6 | x1.19 | UYARI |
| tabela | %99 | 7.3 | x1.12 | GEÇTİ |
| nal panosu | %96 | 7.9 | x0.97 | GEÇTİ |
| eyer mandalı | %83 | 10.7 | x1.24 | UYARI |
| perde (fit) | %100 | 5.1 | x1.17 | GEÇTİ |
| hasat yığını (teslim değil) | %67 | 14.1 | x1.52 | KALDI |
| kış yeşilliği (teslim değil) | %77 | 14.5 | x1.16 (fit sonrası) | KALDI |
| saman süs (teslim değil) | %71 | 16.1 | x1.48 | KALDI |

## Sorunlar
- Üç eşya teslim edilmedi (hasat yığını, kış yeşilliği, saman süs); yeni tarif ve yeniden deneme kullanıcı kararı. Saman süs "mısır" okunduğu için dönem kuralı da çiğnendi.
- Işık yönü, ayna ve duvara oturma yalnızca yan yana sayfasından göz kararıdır; sol/sağ parlaklık ölçülmedi. Perde "kümes yuva kutusu önünde" okunmuyor, açık kitap/paravan gibi (göz kararı).
- Dal 32x32 ve soluk; masa üstü yuvasında çok küçük olabilir.
- `tools/art/wall_guide.py` hâlâ yok; kılavuz betiği `art/tmp405/mk.py` (commit edilmez).
