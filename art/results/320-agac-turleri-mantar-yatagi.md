unity-import: pending

# 320 Yeni ağaç türleri ve mantar yatağı (rapor)

Yan yana sayfası: [320-yanyana.png](320-yanyana.png). Aday dosyalar (teslim edilmedi): `art/results/320-red/`.
Masaüstü kopyası: yerel (bulutta yapılamaz).

## Araç ve ayarlar
`create_map_object`, stil eşleme, referanssız kılavuz + maske (`footprint_guide.py`, tuval 96x112 `--n 1`, yükseklikler 40/76/44/96/40/84; mantar 64x40 yükseklik 14), `high top-down`, `low detail`, `basic shading`, `single color outline`, ortak ek metin. 7 çağrı x 1 gen + willow_young ikinci deneme 1 gen = 8 gen (cost_max 12). Çam genç çağrısı bir kez hız sınırına takıldı, 30 sn sonra yeniden gönderildi; çam olgun maske base64'ü bir kez aktarım hatasıyla reddedildi (ücret yok). `get_balance` sonrası: 931,7 (paralel şeritler de harcıyor).

## Sonuç: hiçbiri teslim edilmedi (Assets'e dosya konmadı)
| Dosya | Opak sınır / renk | Stil denetimi (bitki) | Karar |
|---|---|---|---|
| huş genç | x26-69,y33-94 / 31 | KALDI (%36 palete yakın) | teslim yok |
| huş olgun | x19-71,y12-99 / 52 (>48 BAŞARISIZ) | KALDI (doygun x2.19) | teslim yok |
| çam genç | x24-71,y32-82 / 31 | KALDI (x2.58) | teslim yok; sivri çam değil, yuvarlak katlı çalı |
| çam olgun | x19-67,y9-96 / 17 | KALDI (%52) | teslim yok (şekil iyi, palet dışı) |
| söğüt genç (1) | x34-66,y36-95 / 24 | GEÇTİ | teslim yok: yalnızca iki yaprak öbeği, söğüt olarak tanınmıyor |
| söğüt genç (2. deneme) | x22-65,y35-84 / 43 | UYARI | teslim yok: yapraksız çıplak ağaç, tabanda zemin tüfü ("no ground" ihlali) |
| söğüt olgun | x16-70,y9-104 / 34 | KALDI (%44) | teslim yok; tabanda çim/zemin parçası var |
| mantar yatağı | x14-57,y4-32 / 23 | KALDI (doygun x1.54, palete %94) | teslim yok; kırmızı çerçeveli tepsi |

Yarı saydam piksel hepsinde 0. Döşeme testi zemin olmadığı için yapılmadı; ışık ve ayna ölçümü teslim edilmeyenlerde atlandı. Kullanıcı yan yana sayfasına bakıp stil kapısı kararını gözden geçirebilir (çam olgun ve söğüt olgun şekil olarak kullanılabilir görünüyor, `reduce_colors` ya da palet eşlemesi sonrası tekrar değerlendirilebilir).

## Sorunlar
- Stil kapısı (`style_gate: false` olmasına rağmen işçi kuralı: KALDI teslim edilmez) 7 çıktının 6'sını eledi; yeşil ağaç doygunluğu bitki ailesi referanslarından yüksek.
- Çıktılarda `seed` yok; ikinci deneme yalnızca söğüt gençte kullanıldı.
- Çözüm önerisi: tarife ağaç için bitki ailesi paletinden renk adları ("muted olive green") eklemek ya da referans görselli stil eşleme; ayrı iş gerekir.
